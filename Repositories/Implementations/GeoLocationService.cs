using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Services.Interfaces;
using IpBlockingApi.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace IpBlockingApi.Services.Implementations;

/// <summary>
/// Calls ipapi.co to resolve geolocation information for an IP address.
/// Uses a typed <see cref="HttpClient"/> managed by the DI container.
/// </summary>
public sealed class GeoLocationService : IGeoLocationService
{
    private readonly HttpClient _httpClient;
    private readonly GeoLocationSettings _settings;
    private readonly GeoLocationRateLimiter _rateLimiter;
    private readonly ILogger<GeoLocationService> _logger;

    public GeoLocationService(
        HttpClient httpClient,
        IOptions<GeoLocationSettings> options,
        ILogger<GeoLocationService> logger,
        GeoLocationRateLimiter rateLimiter)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClient = httpClient;
        _rateLimiter = rateLimiter;
        _httpClient = httpClient;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("IpBlockingApi/1.0");
    }

    /// <inheritdoc/>
    public async Task<IpLookupResponse?> LookupAsync(
        string ipAddress, CancellationToken ct = default)
    {
        if (!ValidationHelper.IsValidIpAddress(ipAddress))
        {
            _logger.LogWarning("Geo lookup rejected — invalid IP format: {Ip}", ipAddress);
            return null;
        }

        if (!_rateLimiter.TryConsume())
        {
            _logger.LogWarning("Internal rate limit reached — geo lookup skipped for IP: {Ip}", ipAddress);
            return null;
        }

        var path = string.IsNullOrWhiteSpace(_settings.ApiKey)
            ? $"{ipAddress.Trim()}/json/"
            : $"{ipAddress.Trim()}/json/?key={_settings.ApiKey}";

        try
        {
            _logger.LogInformation("Geo lookup → {Ip}", ipAddress);

            var response = await _httpClient.GetAsync(path, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Geo API rate limit reached (429) for IP: {Ip}", ipAddress);
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Geo API returned HTTP {Status} for IP: {Ip}",
                    (int)response.StatusCode, ipAddress);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonConvert.DeserializeObject<IpapiResponse>(json);

            if (result is null || result.HasError)
            {
                _logger.LogWarning("Geo API error for IP {Ip}: {Reason}",
                    ipAddress, result?.Reason ?? "null response");
                return null;
            }

            return new IpLookupResponse
            {
                IpAddress = result.Ip,
                CountryCode = result.CountryCode?.ToUpperInvariant() ?? string.Empty,
                CountryName = result.CountryName,
                Isp = result.Org,
                City = result.City,
                Region = result.Region
            };
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Geo API request timed out for IP: {Ip}", ipAddress);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during geo lookup for IP: {Ip}", ipAddress);
            return null;
        }
    }


    // ── Private DTO ────────────────────────────────────────────────────────────
    // Maps the raw ipapi.co JSON response onto typed properties.
    private sealed class IpapiResponse
    {
        [JsonProperty("ip")] public string Ip { get; set; } = string.Empty;
        [JsonProperty("city")] public string City { get; set; } = string.Empty;
        [JsonProperty("region")] public string Region { get; set; } = string.Empty;
        [JsonProperty("country")] public string CountryCode { get; set; } = string.Empty;
        [JsonProperty("country_name")] public string CountryName { get; set; } = string.Empty;
        [JsonProperty("org")] public string Org { get; set; } = string.Empty;
        [JsonProperty("error")] public bool HasError { get; set; }
        [JsonProperty("reason")] public string? Reason { get; set; }
    }
}
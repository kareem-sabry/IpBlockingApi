using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Models;
using IpBlockingApi.Repositories.Interfaces;
using IpBlockingApi.Services.Interfaces;

namespace IpBlockingApi.Services.Implementations;

/// <inheritdoc cref="IIpService"/>
public sealed class IpService : IIpService
{
    private readonly IGeoLocationService _geoService;
    private readonly ICountryRepository _countryRepo;
    private readonly ILogRepository _logRepo;
    private readonly ILogger<IpService> _logger;

    public IpService(
        IGeoLocationService geoService,
        ICountryRepository countryRepo,
        ILogRepository logRepo,
        ILogger<IpService> logger)
    {
        _geoService = geoService;
        _countryRepo = countryRepo;
        _logRepo = logRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IpLookupResponse?> LookupIpAsync(
        string ipAddress, CancellationToken ct = default)
    {
        if (!ValidationHelper.IsValidIpAddress(ipAddress))
        {
            _logger.LogWarning("Lookup rejected — invalid IP format: {Ip}", ipAddress);
            return null;
        }

        return await _geoService.LookupAsync(ipAddress, ct);
    }

    /// <inheritdoc/>
    public async Task<BlockCheckResponse?> CheckBlockAsync(
        string ipAddress, string userAgent, CancellationToken ct = default)
    {
        var geo = await _geoService.LookupAsync(ipAddress, ct);

        if (geo is null)
        {
            _logger.LogWarning("Block check skipped — geo lookup failed for IP: {Ip}", ipAddress);
            return null;
        }

        var isBlocked = _countryRepo.IsBlocked(geo.CountryCode);

        // Log every attempt — blocked or not — as required by the spec.
        _logRepo.AddLog(new BlockedAttemptLog
        {
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow,
            CountryCode = geo.CountryCode,
            IsBlocked = isBlocked,
            UserAgent = userAgent
        });

        _logger.LogInformation(
            "Block check complete: IP={Ip} Country={Code} IsBlocked={Blocked}",
            ipAddress, geo.CountryCode, isBlocked);

        return new BlockCheckResponse
        {
            IpAddress = ipAddress,
            CountryCode = geo.CountryCode,
            CountryName = geo.CountryName,
            IsBlocked = isBlocked,
            CheckedAt = DateTime.UtcNow
        };
    }
}
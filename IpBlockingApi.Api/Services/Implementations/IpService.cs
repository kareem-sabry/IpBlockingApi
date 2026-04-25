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
    public async Task<BlockCheckResponse> CheckBlockAsync(
        string ipAddress, string userAgent, CancellationToken ct = default)
    {
        var geo       = await _geoService.LookupAsync(ipAddress, ct);
        var checkedAt = DateTime.UtcNow;

        if (geo is null)
        {
            _logRepo.AddLog(new BlockedAttemptLog
            {
                IpAddress   = ipAddress,
                Timestamp   = checkedAt,
                CountryCode = "XX",
                IsBlocked   = false,
                UserAgent   = userAgent
            });

            _logger.LogWarning(
                "Block check: geo lookup failed for IP {Ip} — logged as not blocked", ipAddress);

            return new BlockCheckResponse
            {
                IpAddress   = ipAddress,
                CountryCode = "XX",
                CountryName = "Unknown",
                IsBlocked   = false,
                CheckedAt   = checkedAt
            };
        }

        var isBlocked = _countryRepo.IsBlocked(geo.CountryCode);

        _logRepo.AddLog(new BlockedAttemptLog
        {
            IpAddress   = ipAddress,
            Timestamp   = checkedAt,
            CountryCode = geo.CountryCode,
            IsBlocked   = isBlocked,
            UserAgent   = userAgent
        });

        _logger.LogInformation(
            "Block check: IP={Ip} Country={Code} IsBlocked={Blocked}",
            ipAddress, geo.CountryCode, isBlocked);

        return new BlockCheckResponse
        {
            IpAddress   = ipAddress,
            CountryCode = geo.CountryCode,
            CountryName = geo.CountryName,
            IsBlocked   = isBlocked,
            CheckedAt   = checkedAt
        };
    }
}
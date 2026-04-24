using IpBlockingApi.DTOs.Responses;

namespace IpBlockingApi.Services.Interfaces;

/// <summary>
/// Abstraction for resolving geolocation details from an IP address
/// via a third-party provider (e.g., ipapi.co).
/// </summary>
public interface IGeoLocationService
{
    /// <summary>
    /// Resolves geolocation details for <paramref name="ipAddress"/>.
    /// Returns <c>null</c> when the lookup fails (invalid IP, rate limit,
    /// provider outage).
    /// </summary>
    Task<IpLookupResponse?> LookupAsync(string ipAddress, CancellationToken ct = default);
}
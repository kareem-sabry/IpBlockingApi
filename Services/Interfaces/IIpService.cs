using IpBlockingApi.DTOs.Responses;

namespace IpBlockingApi.Services.Interfaces;

/// <summary>
/// Handles IP address geolocation lookups and caller block-status checks.
/// </summary>
public interface IIpService
{
    /// <summary>
    /// Resolves geolocation details for <paramref name="ipAddress"/>.
    /// Returns <c>null</c> when the IP is invalid or the geo provider is unavailable.
    /// </summary>
    Task<IpLookupResponse?> LookupIpAsync(string ipAddress, CancellationToken ct = default);

    /// <summary>
    /// Determines whether the country resolved from <paramref name="ipAddress"/> is blocked.
    /// The attempt is always written to the log regardless of the result.
    /// Returns <c>null</c> when geolocation fails.
    /// </summary>
    Task<BlockCheckResponse?> CheckBlockAsync(
        string ipAddress, string userAgent, CancellationToken ct = default);
}
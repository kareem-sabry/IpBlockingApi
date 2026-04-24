using IpBlockingApi.DTOs.Requests;
using IpBlockingApi.DTOs.Responses;

namespace IpBlockingApi.Services.Interfaces;

/// <summary>
/// Business logic for managing permanently and temporarily blocked countries.
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Permanently blocks a country by its ISO country code.
    /// Returns a failure result when the country is already permanently blocked.
    /// </summary>
    Task<(bool Success, string? Error, BlockedCountryResponse? Data)> BlockCountryAsync(
        BlockCountryRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes a country from the permanent block list.
    /// Returns a failure result when the country is not currently blocked.
    /// </summary>
    (bool Success, string? Error) UnblockCountry(string countryCode);

    /// <summary>
    /// Returns a paginated, optionally filtered list of permanently blocked countries.
    /// <paramref name="search"/> is matched case-insensitively against code and name.
    /// </summary>
    PagedResponse<BlockedCountryResponse> GetBlockedCountries(
        int page, int pageSize, string? search);

    /// <summary>
    /// Temporarily blocks a country for <c>DurationMinutes</c> minutes.
    /// Returns a failure result on validation error or when a temporal block already exists.
    /// </summary>
    Task<(bool Success, string? Error, TemporalBlockResponse? Data)> TemporalBlockAsync(
        TemporalBlockRequest request, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> when the country is blocked by either a permanent
    /// or an active (non-expired) temporal block.
    /// </summary>
    bool IsBlocked(string countryCode);
}
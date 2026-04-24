using IpBlockingApi.Models;

namespace IpBlockingApi.Repositories.Interfaces;

/// <summary>
/// Contract for all in-memory country block storage operations.
/// Covers both permanent and temporal blocking collections.
/// </summary>
public interface ICountryRepository
{
    // ── Permanent blocks ──────────────────────────────────────────────────────

    /// <summary>
    /// Adds a country to the permanent block list.
    /// Returns <c>false</c> if it is already present.
    /// </summary>
    bool AddPermanentBlock(BlockedCountry country);

    /// <summary>
    /// Removes a country from the permanent block list.
    /// Returns <c>false</c> if the country was not found.
    /// </summary>
    bool RemovePermanentBlock(string countryCode);

    /// <summary>Returns the permanent block entry, or <c>null</c> if not found.</summary>
    BlockedCountry? GetPermanentBlock(string countryCode);

    /// <summary>Returns all permanently blocked countries.</summary>
    IEnumerable<BlockedCountry> GetAllPermanentBlocks();

    /// <summary>Returns <c>true</c> if the country is permanently blocked.</summary>
    bool IsPermanentlyBlocked(string countryCode);

    // ── Temporal blocks ───────────────────────────────────────────────────────

    /// <summary>
    /// Adds a country to the temporal block list.
    /// Returns <c>false</c> if a temporal block for this country already exists.
    /// </summary>
    bool AddTemporalBlock(TemporalBlock block);

    /// <summary>
    /// Removes a country from the temporal block list.
    /// Returns <c>false</c> if not found.
    /// </summary>
    bool RemoveTemporalBlock(string countryCode);

    /// <summary>Returns the temporal block entry, or <c>null</c> if not found or expired.</summary>
    TemporalBlock? GetTemporalBlock(string countryCode);

    /// <summary>Returns all active (non-expired) temporal blocks.</summary>
    IEnumerable<TemporalBlock> GetAllTemporalBlocks();

    /// <summary>
    /// Returns <c>true</c> if the country has an active (non-expired) temporal block.
    /// Automatically cleans up the entry if it has expired.
    /// </summary>
    bool IsTemporallyBlocked(string countryCode);

    /// <summary>
    /// Scans the temporal block collection and removes all expired entries.
    /// Returns the number of entries removed.
    /// </summary>
    int RemoveExpiredTemporalBlocks();

    // ── Combined ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns <c>true</c> if the country is blocked either permanently or via
    /// an active temporal block.
    /// </summary>
    bool IsBlocked(string countryCode);
}
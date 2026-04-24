using System.Collections.Concurrent;
using IpBlockingApi.Models;
using IpBlockingApi.Repositories.Interfaces;

namespace IpBlockingApi.Repositories.Implementations;

/// <summary>
/// Thread-safe in-memory repository backed by <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// Manages both the permanent block list and the temporal block list independently.
/// </summary>
public sealed class CountryRepository : ICountryRepository
{
    private readonly ConcurrentDictionary<string, BlockedCountry> _permanentBlocks
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks
        = new(StringComparer.OrdinalIgnoreCase);

    // ── Permanent blocks ──────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool AddPermanentBlock(BlockedCountry country)
    {
        var key = Normalize(country.CountryCode);
        country.CountryCode = key;
        return _permanentBlocks.TryAdd(key, country);
    }

    /// <inheritdoc/>
    public bool RemovePermanentBlock(string countryCode)
        => _permanentBlocks.TryRemove(Normalize(countryCode), out _);

    /// <inheritdoc/>
    public BlockedCountry? GetPermanentBlock(string countryCode)
    {
        _permanentBlocks.TryGetValue(Normalize(countryCode), out var country);
        return country;
    }

    /// <inheritdoc/>
    public IEnumerable<BlockedCountry> GetAllPermanentBlocks()
        => _permanentBlocks.Values.ToList();

    /// <inheritdoc/>
    public bool IsPermanentlyBlocked(string countryCode)
        => _permanentBlocks.ContainsKey(Normalize(countryCode));

    // ── Temporal blocks ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool AddTemporalBlock(TemporalBlock block)
    {
        var key = Normalize(block.CountryCode);
        block.CountryCode = key;
        return _temporalBlocks.TryAdd(key, block);
    }

    /// <inheritdoc/>
    public bool RemoveTemporalBlock(string countryCode)
        => _temporalBlocks.TryRemove(Normalize(countryCode), out _);

    /// <inheritdoc/>
    public TemporalBlock? GetTemporalBlock(string countryCode)
    {
        var key = Normalize(countryCode);
        _temporalBlocks.TryGetValue(key, out var block);

        if (block is { IsExpired: true })
        {
            _temporalBlocks.TryRemove(key, out _);
            return null;
        }

        return block;
    }

    /// <inheritdoc/>
    public IEnumerable<TemporalBlock> GetAllTemporalBlocks()
        => _temporalBlocks.Values
            .Where(b => !b.IsExpired)
            .ToList();

    /// <inheritdoc/>
    public bool IsTemporallyBlocked(string countryCode)
    {
        var key = Normalize(countryCode);

        if (!_temporalBlocks.TryGetValue(key, out var block))
            return false;

        if (block.IsExpired)
        {
            _temporalBlocks.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public int RemoveExpiredTemporalBlocks()
    {
        var expiredKeys = _temporalBlocks
            .Where(kv => kv.Value.IsExpired)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in expiredKeys)
            _temporalBlocks.TryRemove(key, out _);

        return expiredKeys.Count;
    }

    // ── Combined ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool IsBlocked(string countryCode)
        => IsPermanentlyBlocked(countryCode) || IsTemporallyBlocked(countryCode);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Normalize(string code)
        => code.Trim().ToUpperInvariant();
}
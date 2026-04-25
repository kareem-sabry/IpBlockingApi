using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Requests;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Models;
using IpBlockingApi.Repositories.Interfaces;
using IpBlockingApi.Services.Interfaces;

namespace IpBlockingApi.Services.Implementations;

/// <inheritdoc cref="ICountryService"/>
public sealed class CountryService : ICountryService
{
    private readonly ICountryRepository _countryRepo;
    private readonly ILogger<CountryService> _logger;

    public CountryService(
        ICountryRepository countryRepo,
        ILogger<CountryService> logger)
    {
        _countryRepo = countryRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<(bool Success, string? Error, BlockedCountryResponse? Data)> BlockCountryAsync(
        BlockCountryRequest request, CancellationToken ct = default)
    {
        var code = Normalize(request.CountryCode);

        if (!CountryNameLookup.IsKnownCode(code))
            return Fail<BlockedCountryResponse>(
                $"Unknown or unsupported country code: '{code}'.");

        if (_countryRepo.IsPermanentlyBlocked(code))
        {
            _logger.LogWarning("Duplicate permanent block attempt: {Code}", code);
            return Fail<BlockedCountryResponse>(
                $"Country '{code}' is already in the permanent block list.");
        }

        var country = new BlockedCountry
        {
            CountryCode = code,
            CountryName = CountryNameLookup.GetName(code),
            BlockedAt = DateTime.UtcNow
        };

        _countryRepo.AddPermanentBlock(country);
        _logger.LogInformation("Permanent block added: {Code}", code);

        return Ok(MapToBlockedCountryResponse(country));
    }

    /// <inheritdoc/>
    public (bool Success, string? Error) UnblockCountry(string countryCode)
    {
        var code = Normalize(countryCode);

        if (!_countryRepo.RemovePermanentBlock(code))
        {
            _logger.LogWarning("Unblock attempt for country not in list: {Code}", code);
            return (false, $"Country '{code}' is not in the permanent block list.");
        }

        _logger.LogInformation("Permanent block removed: {Code}", code);
        return (true, null);
    }

    /// <inheritdoc/>
    public PagedResponse<BlockedCountryResponse> GetBlockedCountries(
        int page, int pageSize, string? search)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _countryRepo.GetAllPermanentBlocks().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.CountryCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.CountryName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var all = query.OrderBy(c => c.CountryCode).ToList();

        return new PagedResponse<BlockedCountryResponse>
        {
            Items = all.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(MapToBlockedCountryResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        };
    }

    /// <inheritdoc/>
    public Task<(bool Success, string? Error, TemporalBlockResponse? Data)> TemporalBlockAsync(
        TemporalBlockRequest request, CancellationToken ct = default)
    {
        var code = Normalize(request.CountryCode);

        if (!CountryNameLookup.IsKnownCode(code))
            return Fail<TemporalBlockResponse>(
                $"Unknown or unsupported country code: '{code}'.");

        if (_countryRepo.IsTemporallyBlocked(code))
        {
            _logger.LogWarning("Duplicate temporal block attempt: {Code}", code);
            return Fail<TemporalBlockResponse>(
                $"Country '{code}' is already temporarily blocked.");
        }

        var now = DateTime.UtcNow;
        var block = new TemporalBlock
        {
            CountryCode = code,
            CountryName = CountryNameLookup.GetName(code),
            BlockedAt = now,
            ExpiresAt = now.AddMinutes(request.DurationMinutes),
            DurationMinutes = request.DurationMinutes
        };

        _countryRepo.AddTemporalBlock(block);
        _logger.LogInformation("Temporal block added: {Code} for {Min} min", code, request.DurationMinutes);

        return Ok(MapToTemporalBlockResponse(block));
    }

    /// <inheritdoc/>
    public bool IsBlocked(string countryCode)
        => _countryRepo.IsBlocked(Normalize(countryCode));

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Normalize(string code) => code.Trim().ToUpperInvariant();

    private static BlockedCountryResponse MapToBlockedCountryResponse(BlockedCountry c) => new()
    {
        CountryCode = c.CountryCode,
        CountryName = c.CountryName,
        BlockedAt = c.BlockedAt
    };

    private static TemporalBlockResponse MapToTemporalBlockResponse(TemporalBlock b) => new()
    {
        CountryCode = b.CountryCode,
        CountryName = b.CountryName,
        BlockedAt = b.BlockedAt,
        ExpiresAt = b.ExpiresAt,
        DurationMinutes = b.DurationMinutes
    };

    private static Task<(bool, string?, T?)> Ok<T>(T data)
        => Task.FromResult<(bool, string?, T?)>((true, null, data));

    private static Task<(bool, string?, T?)> Fail<T>(string error)
        => Task.FromResult<(bool, string?, T?)>((false, error, default));
}
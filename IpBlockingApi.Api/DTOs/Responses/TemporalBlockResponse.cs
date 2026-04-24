namespace IpBlockingApi.DTOs.Responses;

/// <summary>
/// Response model for a time-limited (temporal) country block entry.
/// </summary>
public class TemporalBlockResponse
{
    /// <summary>ISO 3166-1 alpha-2 country code.</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Full country name.</summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the temporal block was created.</summary>
    public DateTime BlockedAt { get; set; }

    /// <summary>UTC timestamp when the temporal block will automatically expire.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Requested block duration in minutes.</summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Time remaining until the block expires. Returns <see cref="TimeSpan.Zero"/>
    /// if already expired.
    /// </summary>
    public TimeSpan RemainingTime =>
        ExpiresAt > DateTime.UtcNow ? ExpiresAt - DateTime.UtcNow : TimeSpan.Zero;
}
namespace IpBlockingApi.Models;

/// <summary>
/// Represents a country that has been blocked for a finite duration.
/// Once <see cref="ExpiresAt"/> is reached, the block is no longer active.
/// </summary>
public class TemporalBlock
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country code. Always stored in uppercase.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Full display name of the country.
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp recording when this temporal block was created.
    /// </summary>
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp after which this block is considered expired and inactive.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Requested block duration in minutes (1–1440).
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Returns <c>true</c> when the current UTC time has passed <see cref="ExpiresAt"/>.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
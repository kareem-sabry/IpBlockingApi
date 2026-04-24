namespace IpBlockingApi.Models;

/// <summary>
/// Represents a country that has been permanently added to the blocked list.
/// </summary>
public class BlockedCountry
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country code (e.g., "US", "EG", "GB").
    /// Always stored in uppercase.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Full display name of the country (e.g., "United States", "Egypt").
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp recording when this country was added to the blocked list.
    /// </summary>
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
}
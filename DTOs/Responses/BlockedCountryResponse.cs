namespace IpBlockingApi.DTOs.Responses;

/// <summary>
/// Response model representing a single permanently blocked country entry.
/// </summary>
public class BlockedCountryResponse
{
    /// <summary>ISO 3166-1 alpha-2 country code.</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Full country name.</summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the country was blocked.</summary>
    public DateTime BlockedAt { get; set; }
}
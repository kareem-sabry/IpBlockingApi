namespace IpBlockingApi.DTOs.Responses;

/// <summary>
/// Response model for the IP block-check endpoint, indicating
/// whether the caller's country is currently blocked.
/// </summary>
public class BlockCheckResponse
{
    /// <summary>The caller's resolved IP address.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>Country code resolved from the caller's IP.</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Country name resolved from the caller's IP.</summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the caller's country is currently in the blocked list
    /// (permanently or temporarily).
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>UTC timestamp of when the check was performed.</summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
namespace IpBlockingApi.DTOs.Responses;

/// <summary>
/// Response model for a single entry in the blocked-attempts log.
/// </summary>
public class BlockedAttemptLogResponse
{
    /// <summary>IP address of the caller who triggered the check.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>UTC timestamp of the attempt.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Country code resolved from the caller's IP.</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Whether the caller's country was blocked at the time of the attempt.</summary>
    public bool IsBlocked { get; set; }

    /// <summary>User-Agent header sent with the request.</summary>
    public string UserAgent { get; set; } = string.Empty;
}
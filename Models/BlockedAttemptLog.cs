namespace IpBlockingApi.Models;

/// <summary>
/// Represents a single recorded access attempt that was evaluated against
/// the blocked countries list, regardless of whether it was blocked or not.
/// </summary>
public class BlockedAttemptLog
{
    /// <summary>
    /// The caller's resolved IP address at the time of the attempt.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of when the attempt occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Country code resolved from the caller's IP via the geolocation service.
    /// </summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the resolved country was in the blocked list
    /// at the time of the attempt.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// The value of the User-Agent header sent with the incoming request.
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
}
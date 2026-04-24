using System.ComponentModel.DataAnnotations;

namespace IpBlockingApi.Settings;

/// <summary>
/// Strongly-typed configuration model bound to the <c>GeoLocation</c> section
/// of <c>appsettings.json</c>.
/// </summary>
public sealed class GeoLocationSettings
{
    /// <summary>Geolocation provider identifier (e.g., "ipapi").</summary>
    public string Provider { get; set; } = "ipapi";

    /// <summary>
    /// Optional API key. Leave empty to use the ipapi.co free tier
    /// (1 000 requests / day without authentication).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL of the geolocation REST API.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = "https://ipapi.co";
}
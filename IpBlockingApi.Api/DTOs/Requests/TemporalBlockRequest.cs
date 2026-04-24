using System.ComponentModel.DataAnnotations;

namespace IpBlockingApi.DTOs.Requests;

/// <summary>
/// Request body for temporarily blocking a country for a specified duration.
/// </summary>
public class TemporalBlockRequest
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country code. Must be exactly 2 uppercase letters (e.g., "US", "EG").
    /// </summary>
    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters only.")]
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the block in minutes. Accepted range: 1 to 1440 (24 hours).
    /// </summary>
    [Required(ErrorMessage = "Duration in minutes is required.")]
    [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes.")]
    public int DurationMinutes { get; set; }
}
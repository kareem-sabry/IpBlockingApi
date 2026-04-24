using System.ComponentModel.DataAnnotations;

namespace IpBlockingApi.DTOs.Requests;

/// <summary>
/// Request body for permanently blocking a country by its ISO country code.
/// </summary>
public class BlockCountryRequest
{
    /// <summary>
    /// ISO 3166-1 alpha-2 country code. Must be exactly 2 uppercase letters (e.g., "US", "EG").
    /// </summary>
    [Required(ErrorMessage = "Country code is required.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Country code must be exactly 2 characters.")]
    [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters only.")]
    public string CountryCode { get; set; } = string.Empty;
}
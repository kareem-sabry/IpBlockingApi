namespace IpBlockingApi.DTOs.Responses;

/// <summary>
/// Response model containing geolocation details resolved from an IP address.
/// </summary>
public class IpLookupResponse
{
    /// <summary>The queried IP address.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>ISO 3166-1 alpha-2 country code resolved from the IP.</summary>
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Full country name resolved from the IP.</summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>Internet Service Provider name.</summary>
    public string Isp { get; set; } = string.Empty;

    /// <summary>City associated with the IP address.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Region or state associated with the IP address.</summary>
    public string Region { get; set; } = string.Empty;
}
using System.Net;

namespace IpBlockingApi.Common;

/// <summary>
/// Static helpers for validating user-supplied input before it reaches services.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Returns <c>true</c> if <paramref name="ip"/> is a syntactically valid
    /// IPv4 or IPv6 address.
    /// </summary>
    public static bool IsValidIpAddress(string? ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;
        return IPAddress.TryParse(ip.Trim(), out _);
    }

    /// <summary>
    /// Returns <c>true</c> if <paramref name="code"/> matches the pattern
    /// <c>^[A-Z]{2}$</c> (exactly 2 uppercase ASCII letters).
    /// </summary>
    public static bool IsValidCountryCodeFormat(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        var t = code.Trim();
        return t.Length == 2 && t.All(char.IsAsciiLetterUpper);
    }
}
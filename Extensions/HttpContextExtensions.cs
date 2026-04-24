using System.Net;

namespace IpBlockingApi.Extensions;

/// <summary>
/// Extension methods on <see cref="HttpContext"/> for common request-level operations.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Resolves the caller's real IP address.
    /// <para>
    /// Checks the <c>X-Forwarded-For</c> header first (comma-separated list;
    /// the leftmost entry is the original client). Falls back to
    /// <see cref="ConnectionInfo.RemoteIpAddress"/> when the header is absent
    /// or contains no valid IP.
    /// </para>
    /// </summary>
    public static string GetCallerIp(this HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var candidate = forwardedFor.Split(',')[0].Trim();
            if (IPAddress.TryParse(candidate, out _))
                return candidate;
        }

        // MapToIPv4 converts ::ffff:x.x.x.x (IPv6-mapped IPv4) back to plain IPv4.
        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}
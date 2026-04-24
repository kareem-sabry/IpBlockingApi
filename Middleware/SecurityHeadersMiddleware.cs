namespace IpBlockingApi.Middleware;

/// <summary>
/// Middleware that appends security-related HTTP response headers to every outgoing response.
/// These headers mitigate common browser-based attack vectors such as MIME-sniffing,
/// clickjacking, and information leakage.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Register headers before the response body starts streaming.
        context.Response.OnStarting(() =>
        {
            var h = context.Response.Headers;

            // Prevent MIME-type sniffing.
            h["X-Content-Type-Options"] = "nosniff";

            // Deny framing to prevent clickjacking.
            h["X-Frame-Options"] = "DENY";

            // Basic XSS filter (legacy browsers).
            h["X-XSS-Protection"] = "1; mode=block";

            // Do not send the Referer header cross-origin.
            h["Referrer-Policy"] = "no-referrer";

            // Disable caching for API responses.
            h["Cache-Control"] = "no-store";

            // Restrict browser feature access.
            h["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
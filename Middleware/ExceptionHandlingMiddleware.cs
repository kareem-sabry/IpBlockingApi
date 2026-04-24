using System.Net;
using System.Text.Json;
using IpBlockingApi.Common;

namespace IpBlockingApi.Middleware;

/// <summary>
/// Global exception-handling middleware. Catches any unhandled exception thrown
/// by the inner pipeline, logs it, and returns a standardized JSON error response.
/// Stack traces are never exposed outside of the Development environment.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate                      _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment                     _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate                       next,
        ILogger<ExceptionHandlingMiddleware>  logger,
        IHostEnvironment                      env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception — {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted) return;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

        var message = _env.IsDevelopment()
            ? $"Unhandled exception: {ex.Message}"
            : "An unexpected error occurred. Please try again later.";

        var body = ApiResponse<object>.Fail(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }
}
namespace IpBlockingApi.Common;

/// <summary>
/// Standardized envelope for all API responses.
/// Ensures every endpoint returns a consistent JSON shape.
/// </summary>
/// <typeparam name="T">The type of the response payload.</typeparam>
public class ApiResponse<T>
{
    /// <summary>Indicates whether the operation completed successfully.</summary>
    public bool Success { get; private set; }

    /// <summary>Human-readable message describing the result or error.</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>The response payload. Null on failure.</summary>
    public T? Data { get; private set; }

    private ApiResponse()
    {
    }

    /// <summary>
    /// Creates a successful response containing <paramref name="data"/>.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.")
        => new() { Success = true, Message = message, Data = data };

    /// <summary>
    /// Creates a failed response with an error <paramref name="message"/> and no data.
    /// </summary>
    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}
namespace IpBlockingApi.DTOs.Responses;

/// <summary>
/// Generic wrapper for paginated list responses.
/// </summary>
/// <typeparam name="T">The type of items in the paginated result.</typeparam>
public class PagedResponse<T>
{
    /// <summary>The items on the current page.</summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>The current page number (1-based).</summary>
    public int Page { get; set; }

    /// <summary>The maximum number of items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>The total number of items across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages, derived from <see cref="TotalCount"/>
    /// and <see cref="PageSize"/>.
    /// </summary>
    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;
}
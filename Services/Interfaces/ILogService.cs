using IpBlockingApi.DTOs.Responses;

namespace IpBlockingApi.Services.Interfaces;

/// <summary>
/// Business logic for retrieving and paginating the blocked-attempt log.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Returns a paginated list of blocked-attempt log entries,
    /// ordered by timestamp descending (most recent first).
    /// <para>
    /// <paramref name="page"/> is clamped to a minimum of 1.
    /// <paramref name="pageSize"/> is clamped to the range [1, 100].
    /// </para>
    /// </summary>
    PagedResponse<BlockedAttemptLogResponse> GetBlockedAttempts(int page, int pageSize);
}
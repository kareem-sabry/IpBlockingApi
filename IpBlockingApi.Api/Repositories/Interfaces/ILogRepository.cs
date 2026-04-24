using IpBlockingApi.Models;

namespace IpBlockingApi.Repositories.Interfaces;

/// <summary>
/// Contract for storing and retrieving blocked-attempt log entries.
/// </summary>
public interface ILogRepository
{
    /// <summary>Appends a new log entry to the in-memory store.</summary>
    void AddLog(BlockedAttemptLog log);

    /// <summary>
    /// Returns all log entries ordered by <see cref="BlockedAttemptLog.Timestamp"/>
    /// descending (most recent first).
    /// </summary>
    IEnumerable<BlockedAttemptLog> GetAllLogs();
}
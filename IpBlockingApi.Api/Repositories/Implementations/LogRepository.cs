using System.Collections.Concurrent;
using IpBlockingApi.Models;
using IpBlockingApi.Repositories.Interfaces;

namespace IpBlockingApi.Repositories.Implementations;

/// <summary>
/// Thread-safe in-memory log repository backed by <see cref="ConcurrentBag{T}"/>.
/// Entries are returned ordered by timestamp descending.
/// </summary>
public sealed class LogRepository : ILogRepository
{
    private readonly ConcurrentBag<BlockedAttemptLog> _logs = new();

    /// <inheritdoc/>
    public void AddLog(BlockedAttemptLog log) => _logs.Add(log);

    /// <inheritdoc/>
    public IEnumerable<BlockedAttemptLog> GetAllLogs()
        => _logs.OrderByDescending(l => l.Timestamp).ToList();
}
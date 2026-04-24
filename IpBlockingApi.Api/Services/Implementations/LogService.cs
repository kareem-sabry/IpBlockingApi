using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Repositories.Interfaces;
using IpBlockingApi.Services.Interfaces;

namespace IpBlockingApi.Services.Implementations;

/// <inheritdoc cref="ILogService"/>
public sealed class LogService : ILogService
{
    private readonly ILogRepository _logRepo;

    public LogService(ILogRepository logRepo)
    {
        _logRepo = logRepo;
    }

    /// <inheritdoc/>
    public PagedResponse<BlockedAttemptLogResponse> GetBlockedAttempts(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var all = _logRepo.GetAllLogs().ToList(); // already ordered desc by timestamp

        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new BlockedAttemptLogResponse
            {
                IpAddress = l.IpAddress,
                Timestamp = l.Timestamp,
                CountryCode = l.CountryCode,
                IsBlocked = l.IsBlocked,
                UserAgent = l.UserAgent
            })
            .ToList();

        return new PagedResponse<BlockedAttemptLogResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        };
    }
}
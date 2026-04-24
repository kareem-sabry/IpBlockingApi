using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockingApi.Controllers;

/// <summary>
/// Exposes the in-memory log of blocked-access attempt entries.
/// </summary>
[ApiController]
[Route("api/logs")]
[Produces("application/json")]
public sealed class LogsController : ControllerBase
{
    private readonly ILogRepository _logRepo;
    private readonly ILogger<LogsController> _logger;

    public LogsController(ILogRepository logRepo, ILogger<LogsController> logger)
    {
        _logRepo = logRepo;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of blocked-access attempt log entries
    /// ordered by timestamp descending (most recent first).
    /// </summary>
    /// <param name="page">Page number (1-based, default 1).</param>
    /// <param name="pageSize">Items per page (default 10, max 100).</param>
    [HttpGet("blocked-attempts")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<BlockedAttemptLogResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetBlockedAttempts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var all = _logRepo.GetAllLogs().ToList();

        var paged = all
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

        var response = new PagedResponse<BlockedAttemptLogResponse>
        {
            Items = paged,
            Page = page,
            PageSize = pageSize,
            TotalCount = all.Count
        };

        return Ok(ApiResponse<PagedResponse<BlockedAttemptLogResponse>>.Ok(response));
    }
}
using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Services.Interfaces;
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
    private readonly ILogService _logService;
    private readonly ILogger<LogsController> _logger;

    public LogsController(ILogService logService, ILogger<LogsController> logger)
    {
        _logService = logService;
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
        var result = _logService.GetBlockedAttempts(page, pageSize);
        return Ok(ApiResponse<PagedResponse<BlockedAttemptLogResponse>>.Ok(result));
    }
}
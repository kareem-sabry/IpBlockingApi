using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Extensions;
using IpBlockingApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockingApi.Controllers;

/// <summary>
/// Endpoints for IP address geolocation lookups and blocked-country checks.
/// </summary>
[ApiController]
[Route("api/ip")]
[Produces("application/json")]
public sealed class IpController : ControllerBase
{
    private readonly IIpService _ipService;
    private readonly ILogger<IpController> _logger;

    public IpController(IIpService ipService, ILogger<IpController> logger)
    {
        _ipService = ipService;
        _logger = logger;
    }

    /// <summary>
    /// Looks up geolocation details for an IP address.
    /// When <paramref name="ipAddress"/> is omitted, the caller's IP is used automatically.
    /// </summary>
    /// <param name="ipAddress">Target IPv4 or IPv6 address (optional).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(ApiResponse<IpLookupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IpLookupResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<IpLookupResponse>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> Lookup(
        [FromQuery] string? ipAddress, CancellationToken ct)
    {
        var targetIp = string.IsNullOrWhiteSpace(ipAddress)
            ? HttpContext.GetCallerIp()
            : ipAddress.Trim();

        if (!ValidationHelper.IsValidIpAddress(targetIp))
            return BadRequest(
                ApiResponse<IpLookupResponse>.Fail($"'{targetIp}' is not a valid IP address."));

        var result = await _ipService.LookupIpAsync(targetIp, ct);

        if (result is null)
            return StatusCode(StatusCodes.Status502BadGateway,
                ApiResponse<IpLookupResponse>.Fail(
                    "Geolocation service is unavailable or returned an error."));

        return Ok(ApiResponse<IpLookupResponse>.Ok(result));
    }

    /// <summary>
    /// Checks whether the caller's country is in the blocked list.
    /// The caller's IP is resolved automatically from the HTTP connection.
    /// Every check is written to the attempt log regardless of the outcome.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("check-block")]
    [ProducesResponseType(typeof(ApiResponse<BlockCheckResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlockCheckResponse>), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CheckBlock(CancellationToken ct)
    {
        var callerIp = HttpContext.GetCallerIp();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _ipService.CheckBlockAsync(callerIp, userAgent, ct);

        if (result is null)
            return StatusCode(StatusCodes.Status502BadGateway,
                ApiResponse<BlockCheckResponse>.Fail(
                    "Could not resolve geolocation for your IP address."));

        return Ok(ApiResponse<BlockCheckResponse>.Ok(result));
    }
}
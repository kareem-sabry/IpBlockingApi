using IpBlockingApi.Common;
using IpBlockingApi.DTOs.Requests;
using IpBlockingApi.DTOs.Responses;
using IpBlockingApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IpBlockingApi.Controllers;

/// <summary>
/// Manages the blocked countries list — both permanent and temporal blocks.
/// </summary>
[ApiController]
[Route("api/countries")]
[Produces("application/json")]
public sealed class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(
        ICountryService countryService,
        ILogger<CountriesController> logger)
    {
        _countryService = countryService;
        _logger = logger;
    }

    /// <summary>
    /// Permanently blocks a country by ISO 3166-1 alpha-2 country code.
    /// </summary>
    /// <param name="request">Body containing the country code (e.g., "US").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created blocked-country record.</returns>
    [HttpPost("block")]
    [ProducesResponseType(typeof(ApiResponse<BlockedCountryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlockedCountryResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BlockCountry(
        [FromBody] BlockCountryRequest request, CancellationToken ct)
    {
        var (success, error, data) = await _countryService.BlockCountryAsync(request, ct);

        if (!success)
            return Conflict(ApiResponse<BlockedCountryResponse>.Fail(error!));

        return Ok(ApiResponse<BlockedCountryResponse>.Ok(data!, "Country blocked successfully."));
    }

    /// <summary>
    /// Removes a country from the permanent block list.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 code of the country to unblock.</param>
    [HttpDelete("block/{countryCode}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public IActionResult UnblockCountry([FromRoute] string countryCode)
    {
        var (success, error) = _countryService.UnblockCountry(countryCode);

        if (!success)
            return NotFound(ApiResponse<object>.Fail(error!));

        var code = countryCode.Trim().ToUpperInvariant();
        return Ok(ApiResponse<object>.Ok(new { }, $"Country '{code}' has been unblocked."));
    }

    /// <summary>
    /// Returns a paginated, searchable list of all permanently blocked countries.
    /// </summary>
    /// <param name="page">Page number (1-based, default 1).</param>
    /// <param name="pageSize">Items per page (default 10, max 100).</param>
    /// <param name="search">Optional filter applied to country code and name.</param>
    [HttpGet("blocked")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<BlockedCountryResponse>>), StatusCodes.Status200OK)]
    public IActionResult GetBlockedCountries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = _countryService.GetBlockedCountries(page, pageSize, search);
        return Ok(ApiResponse<PagedResponse<BlockedCountryResponse>>.Ok(result));
    }

    /// <summary>
    /// Temporarily blocks a country for a specified duration (1–1440 minutes).
    /// The block is removed automatically when it expires.
    /// </summary>
    /// <param name="request">Country code and duration in minutes.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpPost("temporal-block")]
    [ProducesResponseType(typeof(ApiResponse<TemporalBlockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TemporalBlockResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<TemporalBlockResponse>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TemporalBlock(
        [FromBody] TemporalBlockRequest request, CancellationToken ct)
    {
        var (success, error, data) = await _countryService.TemporalBlockAsync(request, ct);

        if (!success)
        {
            // Duplicate temporal block → 409 Conflict; invalid code → 400 Bad Request.
            return error!.Contains("already temporarily blocked")
                ? Conflict(ApiResponse<TemporalBlockResponse>.Fail(error))
                : BadRequest(ApiResponse<TemporalBlockResponse>.Fail(error));
        }

        return Ok(ApiResponse<TemporalBlockResponse>.Ok(data!, "Country temporarily blocked."));
    }
}
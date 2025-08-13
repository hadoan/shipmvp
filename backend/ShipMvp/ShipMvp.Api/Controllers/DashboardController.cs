using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ShipMvp.Application.Analytics;
using ShipMvp.Domain.Analytics.Models;
using ShipMvp.Domain.Shared.Constants;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace ShipMvp.Api.Controllers;

/// <summary>
/// Dashboard analytics endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    /// <summary>
    /// Initializes a new instance of the DashboardController
    /// </summary>
    /// <param name="dashboardService">Dashboard service</param>
    /// <param name="logger">Logger instance</param>
    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get complete dashboard data including summary stats, top sources, and time series
    /// </summary>
    /// <param name="startDate">Start date for data range (optional, defaults to 7 days ago)</param>
    /// <param name="endDate">End date for data range (optional, defaults to today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete dashboard data</returns>
    [HttpGet]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Get dashboard data",
        Description = "Retrieve complete dashboard analytics including summary statistics, top traffic sources, and time series data"
    )]
    [SwaggerResponse(200, "Dashboard data retrieved successfully", typeof(DashboardData))]
    [SwaggerResponse(400, "Invalid date range")]
    [SwaggerResponse(403, "Access denied")]
    public async Task<ActionResult<DashboardData>> GetDashboardDataAsync(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "Start date cannot be after end date" });
            }

            _logger.LogDebug("Getting dashboard data for date range: {StartDate} to {EndDate}",
                startDate, endDate);

            var data = await _dashboardService.GetDashboardDataAsync(startDate, endDate, cancellationToken);

            _logger.LogInformation("Successfully retrieved dashboard data");
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard data" });
        }
    }

    /// <summary>
    /// Get top traffic sources
    /// </summary>
    /// <param name="startDate">Start date for data range (optional, defaults to 7 days ago)</param>
    /// <param name="endDate">End date for data range (optional, defaults to today)</param>
    /// <param name="limit">Maximum number of sources to return (1-50, defaults to 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of top traffic sources</returns>
    [HttpGet("sources")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Get top traffic sources",
        Description = "Retrieve the top traffic sources with visitor counts and sales data"
    )]
    [SwaggerResponse(200, "Traffic sources retrieved successfully", typeof(List<TrafficSource>))]
    [SwaggerResponse(400, "Invalid parameters")]
    [SwaggerResponse(403, "Access denied")]
    public async Task<ActionResult<List<TrafficSource>>> GetTopSourcesAsync(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery][Range(1, 50)] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "Start date cannot be after end date" });
            }

            _logger.LogDebug("Getting top {Limit} traffic sources for date range: {StartDate} to {EndDate}",
                limit, startDate, endDate);

            var sources = await _dashboardService.GetTopSourcesAsync(startDate, endDate, limit, cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} traffic sources", sources.Count);
            return Ok(sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top traffic sources");
            return StatusCode(500, new { message = "An error occurred while retrieving traffic sources" });
        }
    }

    /// <summary>
    /// Get time series data for charts
    /// </summary>
    /// <param name="startDate">Start date for data range (optional, defaults to 7 days ago)</param>
    /// <param name="endDate">End date for data range (optional, defaults to today)</param>
    /// <param name="granularity">Data granularity: daily, weekly, or monthly (defaults to daily)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Time series data points</returns>
    [HttpGet("timeseries")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Get time series data",
        Description = "Retrieve time series data for revenue, profit, page views, and signups charts"
    )]
    [SwaggerResponse(200, "Time series data retrieved successfully", typeof(List<TimeSeriesDataPoint>))]
    [SwaggerResponse(400, "Invalid parameters")]
    [SwaggerResponse(403, "Access denied")]
    public async Task<ActionResult<List<TimeSeriesDataPoint>>> GetTimeSeriesDataAsync(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string granularity = "daily",
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "Start date cannot be after end date" });
            }

            var validGranularities = new[] { "daily", "weekly", "monthly" };
            if (!validGranularities.Contains(granularity.ToLowerInvariant()))
            {
                return BadRequest(new { message = "Granularity must be one of: daily, weekly, monthly" });
            }

            _logger.LogDebug("Getting time series data with {Granularity} granularity for date range: {StartDate} to {EndDate}",
                granularity, startDate, endDate);

            var data = await _dashboardService.GetTimeSeriesDataAsync(startDate, endDate, granularity, cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} time series data points", data.Count);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time series data");
            return StatusCode(500, new { message = "An error occurred while retrieving time series data" });
        }
    }

    /// <summary>
    /// Get dashboard summary statistics only
    /// </summary>
    /// <param name="startDate">Start date for data range (optional, defaults to 7 days ago)</param>
    /// <param name="endDate">End date for data range (optional, defaults to today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard summary statistics</returns>
    [HttpGet("summary")]
    [Authorize(Policy = Policies.RequireAdminRole)]
    [SwaggerOperation(
        Summary = "Get dashboard summary",
        Description = "Retrieve summary statistics including page views, revenue, paying users, and signups with percentage changes"
    )]
    [SwaggerResponse(200, "Summary statistics retrieved successfully", typeof(DashboardSummary))]
    [SwaggerResponse(400, "Invalid date range")]
    [SwaggerResponse(403, "Access denied")]
    public async Task<ActionResult<DashboardSummary>> GetSummaryAsync(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                return BadRequest(new { message = "Start date cannot be after end date" });
            }

            _logger.LogDebug("Getting dashboard summary for date range: {StartDate} to {EndDate}",
                startDate, endDate);

            var data = await _dashboardService.GetDashboardDataAsync(startDate, endDate, cancellationToken);

            _logger.LogInformation("Successfully retrieved dashboard summary");
            return Ok(data.Summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard summary" });
        }
    }
}

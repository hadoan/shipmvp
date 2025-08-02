using Microsoft.Extensions.Logging;
using ShipMvp.Application.Analytics;
using ShipMvp.Domain.Analytics;
using ShipMvp.Domain.Analytics.Models;

namespace ShipMvp.Application.Analytics;

/// <summary>
/// Dashboard service implementation
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IAnalyticsService analyticsService,
        ILogger<DashboardService> logger)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<DashboardData> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var dateRange = CreateDateRange(startDate, endDate);

            _logger.LogDebug("Getting dashboard data for range: {StartDate} to {EndDate}",
                dateRange.StartDate, dateRange.EndDate);

            var result = await _analyticsService.GetDashboardDataAsync(dateRange, cancellationToken);

            _logger.LogInformation("Successfully retrieved dashboard data");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<TrafficSource>> GetTopSourcesAsync(DateTime? startDate = null, DateTime? endDate = null, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var dateRange = CreateDateRange(startDate, endDate);

            _logger.LogDebug("Getting top {Limit} traffic sources for range: {StartDate} to {EndDate}",
                limit, dateRange.StartDate, dateRange.EndDate);

            var result = await _analyticsService.GetTopTrafficSourcesAsync(dateRange, limit, cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} traffic sources", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top traffic sources");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<TimeSeriesDataPoint>> GetTimeSeriesDataAsync(DateTime? startDate = null, DateTime? endDate = null, string granularity = "daily", CancellationToken cancellationToken = default)
    {
        try
        {
            var dateRange = CreateDateRange(startDate, endDate);

            _logger.LogDebug("Getting time series data with {Granularity} granularity for range: {StartDate} to {EndDate}",
                granularity, dateRange.StartDate, dateRange.EndDate);

            var result = await _analyticsService.GetTimeSeriesDataAsync(dateRange, granularity, cancellationToken);

            _logger.LogInformation("Successfully retrieved {Count} time series data points", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time series data");
            throw;
        }
    }

    private static DateTimeRange CreateDateRange(DateTime? startDate, DateTime? endDate)
    {
        var end = endDate ?? DateTime.UtcNow.Date;
        var start = startDate ?? end.AddDays(-7); // Default to last 7 days

        return new DateTimeRange
        {
            StartDate = start,
            EndDate = end
        };
    }
}

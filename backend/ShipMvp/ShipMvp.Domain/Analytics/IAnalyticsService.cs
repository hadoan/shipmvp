using ShipMvp.Domain.Analytics.Models;

namespace ShipMvp.Domain.Analytics;

/// <summary>
/// Service for retrieving Google Analytics data
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Get dashboard data for the specified date range
    /// </summary>
    /// <param name="dateRange">Date range to retrieve data for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete dashboard data</returns>
    Task<DashboardData> GetDashboardDataAsync(DateTimeRange dateRange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top traffic sources
    /// </summary>
    /// <param name="dateRange">Date range to retrieve data for</param>
    /// <param name="limit">Maximum number of sources to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of top traffic sources</returns>
    Task<List<TrafficSource>> GetTopTrafficSourcesAsync(DateTimeRange dateRange, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get time series data for charts
    /// </summary>
    /// <param name="dateRange">Date range to retrieve data for</param>
    /// <param name="granularity">Data granularity (daily, weekly, monthly)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Time series data points</returns>
    Task<List<TimeSeriesDataPoint>> GetTimeSeriesDataAsync(DateTimeRange dateRange, string granularity = "daily", CancellationToken cancellationToken = default);

    /// <summary>
    /// Get summary statistics
    /// </summary>
    /// <param name="dateRange">Current period date range</param>
    /// <param name="previousDateRange">Previous period for comparison</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard summary with percentage changes</returns>
    Task<DashboardSummary> GetSummaryStatisticsAsync(DateTimeRange dateRange, DateTimeRange previousDateRange, CancellationToken cancellationToken = default);
}

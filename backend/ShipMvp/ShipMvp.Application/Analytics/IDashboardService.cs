using ShipMvp.Domain.Analytics;
using ShipMvp.Domain.Analytics.Models;
using ShipMvp.Core;
using ShipMvp.Core.Abstractions;

namespace ShipMvp.Application.Analytics;

/// <summary>
/// Application service for dashboard analytics
/// </summary>
public interface IDashboardService : IScopedService
{
    /// <summary>
    /// Get complete dashboard data
    /// </summary>
    /// <param name="startDate">Start date for data range</param>
    /// <param name="endDate">End date for data range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard data</returns>
    Task<DashboardData> GetDashboardDataAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top traffic sources
    /// </summary>
    /// <param name="startDate">Start date for data range</param>
    /// <param name="endDate">End date for data range</param>
    /// <param name="limit">Maximum number of sources to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top traffic sources</returns>
    Task<List<TrafficSource>> GetTopSourcesAsync(DateTime? startDate = null, DateTime? endDate = null, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get time series data for charts
    /// </summary>
    /// <param name="startDate">Start date for data range</param>
    /// <param name="endDate">End date for data range</param>
    /// <param name="granularity">Data granularity (daily, weekly, monthly)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Time series data</returns>
    Task<List<TimeSeriesDataPoint>> GetTimeSeriesDataAsync(DateTime? startDate = null, DateTime? endDate = null, string granularity = "daily", CancellationToken cancellationToken = default);
}

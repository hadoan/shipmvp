using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using ShipMvp.Domain.Analytics;
using ShipMvp.Domain.Analytics.Models;
using ShipMvp.Domain.Subscriptions;
using ShipMvp.Domain.Identity;

namespace ShipMvp.Application.Infrastructure.Analytics.Services;

/// <summary>
/// Mock analytics service implementation for development and testing
/// </summary>
public class MockAnalyticsService : IAnalyticsService
{
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<MockAnalyticsService> _logger;

    public MockAnalyticsService(
        IUserSubscriptionRepository subscriptionRepository,
        IUserRepository userRepository,
        IMemoryCache cache,
        ILogger<MockAnalyticsService> logger)
    {
        _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<DashboardData> GetDashboardDataAsync(DateTimeRange dateRange, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting mock dashboard data for date range: {StartDate} to {EndDate}",
                dateRange.StartDate, dateRange.EndDate);

            var cacheKey = $"mock_dashboard_data_{dateRange.StartDate:yyyyMMdd}_{dateRange.EndDate:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out DashboardData? cachedData) && cachedData != null)
            {
                _logger.LogDebug("Returning cached mock dashboard data");
                return cachedData;
            }

            // Calculate previous period for comparison
            var daysDiff = (dateRange.EndDate - dateRange.StartDate).Days;
            var previousDateRange = new DateTimeRange
            {
                StartDate = dateRange.StartDate.AddDays(-daysDiff - 1),
                EndDate = dateRange.StartDate.AddDays(-1)
            };

            // Start all tasks concurrently
            var summaryTask = GetSummaryStatisticsAsync(dateRange, previousDateRange, cancellationToken);
            var topSourcesTask = GetTopTrafficSourcesAsync(dateRange, 10, cancellationToken);
            var timeSeriesTask = GetTimeSeriesDataAsync(dateRange, "daily", cancellationToken);

            // Wait for all tasks to complete
            await Task.WhenAll(summaryTask, topSourcesTask, timeSeriesTask);

            // Deconstruct results into a tuple
            var (summary, topSources, timeSeriesData) = (
                await summaryTask,
                await topSourcesTask,
                await timeSeriesTask
            );

            // Compose the final dashboard data
            var dashboardData = new DashboardData
            {
                Summary = summary,
                TopSources = topSources,
                TimeSeriesData = timeSeriesData,
                DateRange = dateRange
            };

            // Cache the result for 15 minutes
            _cache.Set(cacheKey, dashboardData, TimeSpan.FromMinutes(15));

            _logger.LogInformation("Successfully retrieved mock dashboard data");
            return dashboardData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving mock dashboard data");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<TrafficSource>> GetTopTrafficSourcesAsync(DateTimeRange dateRange, int limit = 10, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken); // Simulate async operation

        _logger.LogDebug("Getting top {Limit} mock traffic sources", limit);

        var sources = new List<TrafficSource>
        {
            new() { Source = "Direct / None", Visitors = 3349, Sales = 0, ConversionRate = 2.1m },
            new() { Source = "Google", Visitors = 2688, Sales = 0, ConversionRate = 1.8m },
            new() { Source = "Reddit", Visitors = 732, Sales = 0, ConversionRate = 0.9m },
            new() { Source = "docs.opensaas.sh", Visitors = 438, Sales = 0, ConversionRate = 3.2m },
            new() { Source = "GitHub", Visitors = 360, Sales = 0, ConversionRate = 1.5m },
            new() { Source = "Twitter", Visitors = 274, Sales = 0, ConversionRate = 0.7m },
            new() { Source = "hackerstartup", Visitors = 115, Sales = 0, ConversionRate = 1.2m }
        };

        // Enhance with real sales data from database
        await EnhanceWithSalesDataAsync(sources, dateRange, cancellationToken);

        return sources.Take(limit).ToList();
    }

    /// <inheritdoc />
    public async Task<List<TimeSeriesDataPoint>> GetTimeSeriesDataAsync(DateTimeRange dateRange, string granularity = "daily", CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting mock time series data with {Granularity} granularity", granularity);

        var dataPoints = new List<TimeSeriesDataPoint>();

        // Get real revenue and signup data from database
        var revenueData = await GetRevenueDataAsync(dateRange, cancellationToken);
        var signupData = await GetSignupDataAsync(dateRange, cancellationToken);

        var currentDate = dateRange.StartDate;
        var random = new Random(42); // Use seed for consistent mock data

        while (currentDate <= dateRange.EndDate)
        {
            // Find corresponding revenue and signup data
            var dayRevenue = revenueData.ContainsKey(currentDate) ? revenueData[currentDate] : 0;
            var daySignups = signupData.ContainsKey(currentDate) ? signupData[currentDate] : 0;
            var dayProfit = dayRevenue * 0.7m; // Assuming 70% profit margin

            // Generate mock page views based on the day
            var basePageViews = 400;
            var variation = random.Next(-100, 200);
            var pageViews = Math.Max(100, basePageViews + variation);

            dataPoints.Add(new TimeSeriesDataPoint
            {
                Date = currentDate,
                PageViews = pageViews,
                Revenue = dayRevenue,
                Profit = dayProfit,
                Signups = daySignups
            });

            currentDate = currentDate.AddDays(1);
        }

        return dataPoints.OrderBy(d => d.Date).ToList();
    }

    /// <inheritdoc />
    public async Task<DashboardSummary> GetSummaryStatisticsAsync(DateTimeRange dateRange, DateTimeRange previousDateRange, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting mock summary statistics");

        // Get database statistics
        var currentDbStats = await GetDatabaseStatsAsync(dateRange, cancellationToken);
        var previousDbStats = await GetDatabaseStatsAsync(previousDateRange, cancellationToken);

        return new DashboardSummary
        {
            TotalPageViews = 23526, // Mock data
            PageViewsChange = 29m,
            TotalRevenue = currentDbStats.Revenue,
            RevenueChange = CalculatePercentageChange(currentDbStats.Revenue, previousDbStats.Revenue),
            TotalPayingUsers = currentDbStats.PayingUsers,
            PayingUsersChange = CalculatePercentageChange(currentDbStats.PayingUsers, previousDbStats.PayingUsers),
            TotalSignups = currentDbStats.Signups,
            SignupsChange = CalculatePercentageChange(currentDbStats.Signups, previousDbStats.Signups)
        };
    }

    #region Private Methods

    private async Task<(decimal Revenue, long PayingUsers, long Signups)> GetDatabaseStatsAsync(DateTimeRange dateRange, CancellationToken cancellationToken)
    {
        // Get active subscriptions and their revenue
        var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);
        var revenue = subscriptions
            .Where(s => s.CurrentPeriodStart >= dateRange.StartDate && s.CurrentPeriodStart <= dateRange.EndDate)
            .Sum(s => s.Plan?.Price?.Amount ?? 0);

        var payingUsers = subscriptions
            .Where(s => s.CurrentPeriodStart >= dateRange.StartDate && s.CurrentPeriodStart <= dateRange.EndDate)
            .Select(s => s.UserId)
            .Distinct()
            .Count();

        // Get signups in the date range
        var allUsers = await _userRepository.GetAllAsync(cancellationToken);
        var signups = allUsers
            .Where(u => u.CreatedAt >= dateRange.StartDate && u.CreatedAt <= dateRange.EndDate)
            .Count();

        return (revenue, payingUsers, signups);
    }

    private async Task<Dictionary<DateTime, decimal>> GetRevenueDataAsync(DateTimeRange dateRange, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);

        return subscriptions
            .Where(s => s.CurrentPeriodStart >= dateRange.StartDate && s.CurrentPeriodStart <= dateRange.EndDate)
            .GroupBy(s => s.CurrentPeriodStart.Date)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(s => s.Plan?.Price?.Amount ?? 0)
            );
    }

    private async Task<Dictionary<DateTime, long>> GetSignupDataAsync(DateTimeRange dateRange, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .Where(u => u.CreatedAt >= dateRange.StartDate && u.CreatedAt <= dateRange.EndDate)
            .GroupBy(u => u.CreatedAt.Date)
            .ToDictionary(
                g => g.Key,
                g => (long)g.Count()
            );
    }

    private async Task EnhanceWithSalesDataAsync(List<TrafficSource> sources, DateTimeRange dateRange, CancellationToken cancellationToken)
    {
        // This is a simplified implementation
        // In a real scenario, you'd track conversion sources in your database
        var totalRevenue = (await GetDatabaseStatsAsync(dateRange, cancellationToken)).Revenue;
        var totalVisitors = sources.Sum(s => s.Visitors);

        if (totalVisitors > 0)
        {
            foreach (var source in sources)
            {
                // Distribute revenue proportionally to visitor count (simplified approach)
                source.Sales = totalRevenue * source.Visitors / totalVisitors;
            }
        }
    }

    private static decimal CalculatePercentageChange(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((current - previous) / previous * 100, 1);
    }

    private static decimal CalculatePercentageChange(long current, long previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round((decimal)(current - previous) / previous * 100, 1);
    }

    #endregion
}

namespace ShipMvp.Domain.Analytics.Models;

/// <summary>
/// Dashboard summary statistics
/// </summary>
public class DashboardSummary
{
    /// <summary>
    /// Total page views
    /// </summary>
    public long TotalPageViews { get; set; }

    /// <summary>
    /// Page views percentage change
    /// </summary>
    public decimal PageViewsChange { get; set; }

    /// <summary>
    /// Total revenue
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Revenue percentage change
    /// </summary>
    public decimal RevenueChange { get; set; }

    /// <summary>
    /// Total paying users count
    /// </summary>
    public long TotalPayingUsers { get; set; }

    /// <summary>
    /// Paying users percentage change
    /// </summary>
    public decimal PayingUsersChange { get; set; }

    /// <summary>
    /// Total signups count
    /// </summary>
    public long TotalSignups { get; set; }

    /// <summary>
    /// Signups percentage change
    /// </summary>
    public decimal SignupsChange { get; set; }
}

/// <summary>
/// Traffic source information
/// </summary>
public class TrafficSource
{
    /// <summary>
    /// Source name (e.g., Google, Direct, Reddit)
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// Number of visitors from this source
    /// </summary>
    public long Visitors { get; set; }

    /// <summary>
    /// Sales generated from this source
    /// </summary>
    public decimal Sales { get; set; }

    /// <summary>
    /// Conversion rate for this source
    /// </summary>
    public decimal ConversionRate { get; set; }
}

/// <summary>
/// Time series data point for charts
/// </summary>
public class TimeSeriesDataPoint
{
    /// <summary>
    /// Date for this data point
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Revenue value
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Profit value
    /// </summary>
    public decimal Profit { get; set; }

    /// <summary>
    /// Page views count
    /// </summary>
    public long PageViews { get; set; }

    /// <summary>
    /// Signups count
    /// </summary>
    public long Signups { get; set; }
}

/// <summary>
/// Complete dashboard data
/// </summary>
public class DashboardData
{
    /// <summary>
    /// Summary statistics
    /// </summary>
    public required DashboardSummary Summary { get; set; }

    /// <summary>
    /// Top traffic sources
    /// </summary>
    public List<TrafficSource> TopSources { get; set; } = new();

    /// <summary>
    /// Time series data for charts
    /// </summary>
    public List<TimeSeriesDataPoint> TimeSeriesData { get; set; } = new();

    /// <summary>
    /// Date range for the data
    /// </summary>
    public DateTimeRange DateRange { get; set; } = new();
}

/// <summary>
/// Date range specification
/// </summary>
public class DateTimeRange
{
    /// <summary>
    /// Start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Google Analytics configuration
/// </summary>
public class GoogleAnalyticsRequest
{
    /// <summary>
    /// Property ID for Google Analytics 4
    /// </summary>
    public required string PropertyId { get; set; }

    /// <summary>
    /// Date range for the request
    /// </summary>
    public required DateTimeRange DateRange { get; set; }

    /// <summary>
    /// Metrics to retrieve
    /// </summary>
    public List<string> Metrics { get; set; } = new();

    /// <summary>
    /// Dimensions to group by
    /// </summary>
    public List<string> Dimensions { get; set; } = new();
}

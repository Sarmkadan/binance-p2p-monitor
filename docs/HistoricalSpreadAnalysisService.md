# HistoricalSpreadAnalysisService

Computes statistical metrics over historical spread data and raises anomaly alerts.

## Public API

### IHistoricalSpreadAnalysisService

#### AnalyzeHistoricalSpreadAsync

```csharp
Task<SpreadStatisticsReport?> AnalyzeHistoricalSpreadAsync(
    string asset, 
    string fiat, 
    int hours = 24, 
    CancellationToken ct = default)
```

Builds a full statistical report for the given asset/fiat pair over the specified time window.

**Parameters:**
- `asset`: The asset symbol (e.g., "BTC", "ETH")
- `fiat`: The fiat currency (e.g., "USD", "EUR")
- `hours`: Number of hours of historical data to analyze (default: 24)
- `ct`: Cancellation token for asynchronous operation

**Returns:**
- `SpreadStatisticsReport` containing statistical metrics, or `null` if no historical data is available

**Behavior:**
- Retrieves historical spread data from `_historyRepository`
- Calculates mean, median, standard deviation, variance, min/max spread
- Computes 5th and 95th percentiles
- Gets current spread from `_spreadAnalysisService`
- Calculates Z-score and trend slope using linear regression
- Flags as anomalous if |Z-score| >= 2.0
- Logs analysis results at Information level
- Logs warning if no historical data found
- Logs error and rethrows exception on failure

#### DetectStatisticalAlertsAsync

```csharp
Task<IEnumerable<SpreadStatisticsReport>> DetectStatisticalAlertsAsync(
    IEnumerable<(string Asset, string Fiat)> pairs,
    decimal zScoreThreshold = 2.5m,
    CancellationToken ct = default)
```

Scans the supplied pairs, flags those whose current spread exceeds the Z-score threshold, and publishes a `SpreadAlertTriggeredEvent` for each anomaly detected.

**Parameters:**
- `pairs`: Collection of asset/fiat pairs to analyze
- `zScoreThreshold`: Threshold for detecting anomalies (default: 2.5)
- `ct`: Cancellation token for asynchronous operation

**Returns:**
- Collection of `SpreadStatisticsReport` objects representing detected anomalies

**Behavior:**
- Analyzes each pair concurrently using `AnalyzeHistoricalSpreadAsync`
- Filters results where |Z-score| >= threshold
- Publishes `SpreadAlertTriggeredEvent` for each anomaly via `_eventBus`
- Logs warning with count of detected anomalies
- Logs error and rethrows exception on failure

#### GetSpreadPercentileAsync

```csharp
Task<decimal> GetSpreadPercentileAsync(
    string asset, 
    string fiat, 
    decimal percentile, 
    int hours = 24, 
    CancellationToken ct = default)
```

Returns the spread value at the requested percentile (0–100) from recent history.

**Parameters:**
- `asset`: The asset symbol
- `fiat`: The fiat currency
- `percentile`: Percentile value between 0 and 100
- `hours`: Number of hours of historical data to consider (default: 24)
- `ct`: Cancellation token for asynchronous operation

**Returns:**
- Decimal value representing the spread at the specified percentile

**Exceptions:**
- `ArgumentOutOfRangeException` if percentile is not between 0 and 100

**Behavior:**
- Retrieves historical spread data
- Returns 0 if no historical data available
- Calculates percentile using linear interpolation between nearest ranks
- Logs error and rethrows exception on failure

#### GetRollingWindowAveragesAsync

```csharp
Task<IEnumerable<(DateTime WindowEnd, decimal AverageSpread)>> GetRollingWindowAveragesAsync(
    string asset, 
    string fiat, 
    int windowSizeMinutes = 15, 
    int hours = 24, 
    CancellationToken ct = default)
```

Returns a time-ordered sequence of rolling-window average spreads.

**Parameters:**
- `asset`: The asset symbol
- `fiat`: The fiat currency
- `windowSizeMinutes`: Size of rolling window in minutes (default: 15)
- `hours`: Number of hours of historical data to analyze (default: 24)
- `ct`: Cancellation token for asynchronous operation

**Returns:**
- Sequence of tuples containing window end time and average spread for that window

**Behavior:**
- Retrieves historical spread data ordered by time
- Returns empty sequence if no historical data available
- Computes rolling averages using sliding window approach
- Only includes windows with at least one data point
- Logs error and rethrows exception on failure

## Implementation Details

### HistoricalSpreadAnalysisService

**Constructor Dependencies:**
- `IHistoryRepository` - For retrieving historical price/spread data
- `ISpreadAnalysisService` - For getting current spread analysis
- `IEventBus` - For publishing alert events
- `AppSettings` - For configuration values
- `ILogger<HistoricalSpreadAnalysisService>` - For logging

**Private Helper Methods:**
- `CalculateStandardDeviation` - Computes standard deviation of values
- `CalculatePercentile` - Calculates percentile using linear interpolation
- `CalculateTrendSlope` - Uses ordinary least-squares linear regression to calculate trend in spread-percentage-points per minute

**Error Handling:**
- All public methods wrap operations in try/catch blocks
- Errors are logged at Error level with context
- Exceptions are rethrown after logging
- Null parameter validation in constructor throws `ArgumentNullException`

**Logging:**
- Information level: Successful analysis results
- Warning level: No data found, anomalies detected
- Error level: Exception details with context

## Data Structures

### SpreadStatisticsReport

The service returns `SpreadStatisticsReport` objects containing:
- `Asset`: Asset symbol
- `Fiat`: Fiat currency
- `TimeWindowHours`: Hours of data analyzed
- `SampleCount`: Number of data points
- `Mean`: Average spread percentage
- `Median`: 50th percentile spread
- `StandardDeviation`: Spread standard deviation
- `Variance`: Spread variance
- `MinSpread`: Minimum spread observed
- `MaxSpread`: Maximum spread observed
- `Percentile5`: 5th percentile spread
- `Percentile95`: 95th percentile spread
- `CurrentSpread`: Most recent spread value
- `ZScore`: How many standard deviations current spread is from mean
- `TrendSlope`: Spread change per minute (positive = increasing spread)
- `AnalyzedAt`: Timestamp when analysis was performed
- `IsAnomalous`: Whether current spread exceeds 2.0 Z-score threshold

### SpreadAlertTriggeredEvent

Published when anomalies are detected:
- `Asset`: Asset symbol
- `Fiat`: Fiat currency
- `SpreadPercentage`: Current spread percentage
- `Threshold`: Default spread threshold from settings

## Usage Notes

- The service requires at least 2 data points for meaningful standard deviation and trend calculations
- Percentile calculation uses linear interpolation between nearest ranks
- Trend slope is calculated in spread-percentage-points per minute
- Anomaly detection in `AnalyzeHistoricalSpreadAsync` uses hardcoded threshold of 2.0
- Anomaly detection in `DetectStatisticalAlertsAsync` uses configurable threshold (default 2.5)
- All time calculations use UTC
- Methods are safe for concurrent execution
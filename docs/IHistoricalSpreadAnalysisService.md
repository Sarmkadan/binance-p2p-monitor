# IHistoricalSpreadAnalysisService

The `IHistoricalSpreadAnalysisService` interface defines the contract for performing statistical analysis on historical Binance P2P spread data. It provides asynchronous methods to calculate spread statistics, detect anomalies based on configurable thresholds, determine percentile rankings, and compute rolling window averages. This service is intended for use in monitoring systems that require deep insights into market liquidity trends and pricing deviations over time.

## API

### `HistoricalSpreadAnalysisService`
Represents the concrete implementation of the interface. While the interface defines the contract, this constructor or class entry point initializes the service with necessary data repositories and configuration options required to perform historical queries.

### `AnalyzeHistoricalSpreadAsync`
```csharp
public async Task<SpreadStatisticsReport?> AnalyzeHistoricalSpreadAsync(...)
```
Computes a comprehensive statistical report for a specific asset or trading pair over a defined historical period.
*   **Purpose**: Generates aggregate metrics such as mean, median, standard deviation, min, and max spreads.
*   **Parameters**: Accepts criteria defining the target asset, time range, and data granularity (specific parameters depend on the implementation but generally include `string asset`, `DateTime start`, `DateTime end`).
*   **Return Value**: Returns a `SpreadStatisticsReport` object containing the calculated metrics. Returns `null` if insufficient data exists for the specified range.
*   **Exceptions**: Throws `ArgumentException` if the start date is after the end date or if the asset identifier is invalid. Throws `DataRetrievalException` if the underlying data source is unavailable.

### `DetectStatisticalAlertsAsync`
```csharp
public async Task<IEnumerable<SpreadStatisticsReport>> DetectStatisticalAlertsAsync(...)
```
Scans historical data to identify periods where spread behavior deviated significantly from the norm based on statistical thresholds.
*   **Purpose**: Identifies potential market anomalies, liquidity crunches, or arbitrage opportunities by filtering data points that exceed standard deviation bounds or percentile limits.
*   **Parameters**: Requires configuration for sensitivity (e.g., number of standard deviations) and the time window to scan.
*   **Return Value**: Returns an enumerable collection of `SpreadStatisticsReport` objects, each representing a detected alert window. Returns an empty collection if no alerts are found.
*   **Exceptions**: Throws `InvalidOperationException` if the statistical configuration parameters are out of logical bounds (e.g., negative threshold).

### `GetSpreadPercentileAsync`
```csharp
public async Task<decimal> GetSpreadPercentileAsync(...)
```
Calculates the specific percentile rank of a given spread value within the context of historical data for an asset.
*   **Purpose**: Determines how a current or specific spread compares to the historical distribution (e.g., determining if a spread is in the 95th percentile).
*   **Parameters**: Takes the target spread value, the asset identifier, and the historical lookback period.
*   **Return Value**: Returns a `decimal` between 0 and 100 representing the percentile rank.
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if the provided lookback period contains no data points.

### `GetRollingWindowAveragesAsync`
```csharp
public async Task<IEnumerable<(DateTime WindowEnd, decimal AverageSpread)>> GetRollingWindowAveragesAsync(...)
```
Computes the moving average of spreads over a sliding time window across a historical timeline.
*   **Purpose**: Smooths out short-term fluctuations to highlight longer-term trends in spread volatility.
*   **Parameters**: Requires the window size (e.g., 1 hour, 1 day) and the total time range to analyze.
*   **Return Value**: Returns an enumerable of tuples containing the `WindowEnd` timestamp and the calculated `AverageSpread` for that window.
*   **Exceptions**: Throws `ArgumentException` if the window size is larger than the requested time range or if the window size is non-positive.

## Usage

### Example 1: Generating a Daily Statistical Report
The following example demonstrates how to retrieve a statistical summary for BTC over the last 24 hours to assess general market conditions.

```csharp
public async Task AssessMarketConditionsAsync(IHistoricalSpreadAnalysisService analysisService)
{
    var endTime = DateTime.UtcNow;
    var startTime = endTime.AddHours(-24);

    // Analyze the spread for BTC over the last 24 hours
    var report = await analysisService.AnalyzeHistoricalSpreadAsync(
        asset: "BTC",
        startTime: startTime,
        endTime: endTime
    );

    if (report != null)
    {
        Console.WriteLine($"BTC Spread Analysis (24h):");
        Console.WriteLine($"Average: {report.MeanSpread:P4}");
        Console.WriteLine($"Volatility (StdDev): {report.StandardDeviation:P4}");
        Console.WriteLine($"Max Observed: {report.MaxSpread:P4}");
    }
    else
    {
        Console.WriteLine("Insufficient data available for analysis.");
    }
}
```

### Example 2: Detecting Anomalies via Rolling Averages
This example identifies periods where the rolling average spread exceeded a specific threshold, indicating potential liquidity issues.

```csharp
public async Task IdentifyLiquidityCrunchesAsync(IHistoricalSpreadAnalysisService analysisService)
{
    var endTime = DateTime.UtcNow;
    var startTime = endTime.AddDays(-7);
    var windowSize = TimeSpan.FromHours(1);

    // Get 1-hour rolling averages for the past week
    var rollingAverages = await analysisService.GetRollingWindowAveragesAsync(
        asset: "ETH",
        startTime: startTime,
        endTime: endTime,
        windowSize: windowSize
    );

    var alerts = new List<string>();
    foreach (var (windowEnd, averageSpread) in rollingAverages)
    {
        // Flag if average spread exceeds 2%
        if (averageSpread > 0.02m)
        {
            alerts.Add($"Alert at {windowEnd:yyyy-MM-dd HH:mm}: Spread avg {averageSpread:P2}");
        }
    }

    if (alerts.Any())
    {
        foreach (var alert in alerts)
        {
            Console.WriteLine(alert);
        }
    }
}
```

## Notes

*   **Data Availability**: Methods returning nullable results (`AnalyzeHistoricalSpreadAsync`) or empty collections (`DetectStatisticalAlertsAsync`, `GetRollingWindowAveragesAsync`) do not throw exceptions when no data is found; they simply return the null/empty state. Callers must handle these cases explicitly.
*   **Time Zone Handling**: All `DateTime` parameters and return values operate strictly in UTC. Passing local time without conversion may result in incorrect window calculations or data mismatches.
*   **Thread Safety**: The interface methods are designed to be stateless regarding the input parameters. Implementations should be thread-safe for concurrent read operations, allowing multiple analysis tasks to run in parallel against the same service instance without locking conflicts, provided the underlying data store supports concurrent reads.
*   **Performance Considerations**: `GetRollingWindowAveragesAsync` and `DetectStatisticalAlertsAsync` may involve scanning large datasets. For wide time ranges, consider implementing pagination or restricting the analysis window to prevent timeout issues in high-latency environments.
*   **Precision**: Return types for spread values are `decimal` to maintain financial precision. Avoid casting to `double` before performing further financial calculations to prevent rounding errors.

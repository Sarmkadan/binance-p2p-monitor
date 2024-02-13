# SpreadStatisticsReport

The `SpreadStatisticsReport` type encapsulates statistical analysis of the spread between Binance P2P buy and sell prices for a given asset‑fiat pair over a configurable time window. It provides descriptive statistics (mean, median, variance, etc.), current spread metrics, and anomaly detection flags useful for monitoring market conditions and triggering alerts.

## API

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Asset` | Symbol of the cryptocurrency asset (e.g., "BTC", "ETH"). | none | `string` | None |
| `Fiat` | Fiat currency used for the spread calculation (e.g., "USD", "EUR"). | none | `string` | None |
| `TimeWindowHours` | Length of the historical window, in hours, over which the statistics were computed. | none | `int` | None |
| `SampleCount` | Number of spread observations included in the sample. | none | `long` | None |
| `Mean` | Arithmetic mean of the spread values in the sample. | none | `decimal` | None |
| `Median` | Median (50th percentile) spread value. | none | `decimal` | None |
| `StandardDeviation` | Standard deviation of the spread sample. | none | `decimal` | None |
| `Variance` | Variance of the spread sample (square of `StandardDeviation`). | none | `decimal` | None |
| `MinSpread` | Minimum spread observed in the sample. | none | `decimal` | None |
| `MaxSpread` | Maximum spread observed in the sample. | none | `decimal` | None |
| `Percentile5` | 5th percentile of the spread distribution. | none | `decimal` | None |
| `Percentile95` | 95th percentile of the spread distribution. | none | `decimal` | None |
| `CurrentSpread` | Most recent spread value at the time of analysis. | none | `decimal` | None |
| `ZScore` | Number of standard deviations the `CurrentSpread` deviates from the `Mean`. | none | `decimal` | None |
| `TrendSlope` | Linear regression slope of spread over time within the window (units: spread per hour). | none | `decimal` | None |
| `AnalyzedAt` | UTC timestamp indicating when the report was generated. | none | `DateTime` | None |
| `IsAnomalous` | Flag indicating whether the `CurrentSpread` is considered an anomaly based on statistical thresholds (e.g., |ZScore| > 3). | none | `bool` | None |
| `GetTrendLabel()` | Returns a human‑readable description of the spread trend (e.g., "Rising sharply", "Stable", "Falling"). | none | `string` | None |
| `IsCritical` | True when the spread exceeds a pre‑defined critical threshold (configuration‑dependent). | none | `bool` | None |
| `IsAboveAverage` | True when `CurrentSpread` is greater than the `Mean`. | none | `bool` | None |

## Usage

```csharp
using BinanceP2pMonitor.Models;

// Assume a report has been produced by the monitoring service.
SpreadStatisticsReport report = await spreadAnalyzer.GetReportAsync("BTC", "USD", 24);

// Basic inspection of the current market condition.
Console.WriteLine($"Asset: {report.Asset}/{report.Fiat}");
Console.WriteLine($"Current spread: {report.CurrentSpread:F4}");
Console.WriteLine($"Z‑score: {report.ZScore:F2}");
Console.WriteLine($"Trend: {report.GetTrendLabel()}");

// Alert logic based on anomaly detection.
if (report.IsAnomalous)
{
    Logger.Warning($"Anomalous spread detected for {report.Asset}/{report.Fiat} (Z={report.ZScore:F2}).");
}
if (report.IsCritical)
{
    AlertService.SendCriticalAlert($"Critical spread for {report.Asset}/{report.Fiat}: {report.CurrentSpread:F4}");
}
```

```csharp
using System;
using System.Collections.Concurrent;
using BinanceP2pMonitor.Models;

// Example of aggregating multiple reports into a thread‑safe collection.
ConcurrentBag<SpreadStatisticsReport> dailyReports = new();

// Simulated background worker that updates reports every hour.
void Worker()
{
    while (!cancellationToken.IsCancellationRequested)
    {
        var report = spreadAnalyzer.GetReportAsync("ETH", "EUR", 6).Result;
        dailyReports.Add(report);
        Thread.Sleep(TimeSpan.FromHours(1));
    }
}

// Later, compute aggregate statistics (e.g., average mean spread).
decimal averageMean = dailyReports.Average(r => r.Mean);
Console.WriteLine($"Average mean spread over the day: {averageMean:F4}");
```

## Notes

- All members are read‑only after the report instance is constructed; modifying them directly would break the statistical invariants.
- The class does not contain any synchronization primitives; instances are safe for concurrent read access by multiple threads, but concurrent writes (e.g., deserializing into the same instance from different threads) must be externally synchronized.
- `GetTrendLabel()` relies on the internal `TrendSlope` value; extreme values (e.g., `decimal.MinValue` or `decimal.MaxValue`) may produce unexpected labels if the underlying slope calculation overflows—such values should be considered invalid and the report discarded.
- `IsAnomalous` and `IsCritical` are derived from configurable thresholds; if the underlying configuration changes after the report is created, the flags will not update automatically.
- `AnalyzedAt` is expressed in UTC; consumers should convert to local time zones as needed for display.
- The type assumes that `SampleCount` > 0; a zero sample count would render `Mean`, `Median`, `StandardDeviation`, `Variance`, `Percentile5`, and `Percentile95` meaningless. Consumers should validate `SampleCount` before relying on those statistics.

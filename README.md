# Binance P2P Monitor

A CLI tool for monitoring Binance P2P prices, tracking spread anomalies, and sending Telegram alerts.

![Build](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/binance-p2p-monitor)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

## Features

- Real-time price monitoring via WebSocket with auto-reconnect
- Configurable price-change and spread alerts per user
- Telegram and webhook notifications
- SQLite-backed price history with retention cleanup
- CSV/JSON/table export
- Backtesting support

## Requirements

- .NET 10 SDK
- SQLite (embedded)
- Telegram bot token (optional)

## Setup

```bash
git clone https://github.com/sarmkadan/binance-p2p-monitor
cd binance-p2p-monitor
dotnet restore
```

...

## PerformanceMetricsExtensions

The `PerformanceMetricsExtensions` class provides extension methods for analyzing and reporting performance metrics of operations, including success rates, average durations, failure counts, and operation statistics.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

var metrics = new PerformanceMetrics();
// Add some operations to metrics...

// Get basic metrics
double successRate = PerformanceMetricsExtensions.GetSuccessRate(metrics);
double averageDuration = PerformanceMetricsExtensions.GetAverageDurationMs(metrics);
int failureCount = PerformanceMetricsExtensions.GetFailureCount(metrics);
int totalCount = PerformanceMetricsExtensions.GetTotalCount(metrics);

// Get detailed metrics
double totalDuration = PerformanceMetricsExtensions.GetTotalDurationMs(metrics);
bool hasExecuted = PerformanceMetricsExtensions.HasExecuted(metrics);
DateTime? lastExecution = PerformanceMetricsExtensions.GetLastExecutionTime(metrics);
PerformanceMetrics.OperationMetrics? mostRecent = PerformanceMetricsExtensions.GetMostRecentOperation(metrics);
string? worstOperation = PerformanceMetricsExtensions.GetOperationWithHighestFailureRate(metrics);
double averageSuccessRate = PerformanceMetricsExtensions.GetAverageSuccessRate(metrics);
int operationCount = PerformanceMetricsExtensions.GetOperationCount(metrics);
int failedOperations = PerformanceMetricsExtensions.GetFailedOperationCount(metrics);

Console.WriteLine($"Success Rate: {successRate:P}");
Console.WriteLine($"Average Duration: {averageDuration}ms");
Console.WriteLine($"Total Operations: {totalCount}");
Console.WriteLine($"Last Execution: {lastExecution}");
Console.WriteLine($"Worst Performing: {worstOperation}");
```

...

## ApiResponseExtensions

The `ApiResponseExtensions` class provides utility methods for working with `ApiResponse` objects, enabling fluent validation, error handling, and summary generation. It supports both generic and non-generic response types.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

ApiResponse<int> response = new ApiResponse<int>();
if (!response.IsSuccessful)
{
    response = response.AddError("Failed to process request.");
}
response = response.WithData(42);
Console.WriteLine(response.Summary());
```

...

## License

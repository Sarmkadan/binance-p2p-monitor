# Binance P2p Monitor

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

## CommandFactoryExtensions

The `CommandFactoryExtensions` class provides utility methods for managing command registration and creation in the CLI. It enables checking command availability, retrieving registered command names, and creating commands with validation.

### Usage

```csharp
using BinanceP2pMonitor.CLI;

var factory = new CommandFactory();
bool isAvailable = CommandFactoryExtensions.IsCommandAvailable(factory, "list");
HashSet<string> availableCommands = CommandFactoryExtensions.GetAvailableCommandsSet(factory);
int commandCount = CommandFactoryExtensions.GetCommandCount(factory);

Console.WriteLine($"Commands available: {isAvailable}");
Console.WriteLine($"Registered commands: [{string.Join(", ", availableCommands)}]");
Console.WriteLine($"Total commands: {commandCount}");

if (CommandFactoryExtensions.TryCreateCommand(factory, "list", out var command))
{
    Console.WriteLine("Created 'list' command successfully.");
}
else
{
    Console.WriteLine($"First available command: {CommandFactoryExtensions.FindFirstAvailableCommand(factory)}");
}
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

## ExportCommandExtensions

The `ExportCommandExtensions` class provides utility methods for validating and generating export commands.

### Usage

```csharp
using BinanceP2pMonitor.Commands;

var outputPath = ExportCommandExtensions.GetDefaultOutputPath();
var format = ExportCommandExtensions.GetFormat("csv");
var daysToExport = ExportCommandExtensions.GetDaysToExport(7);
var assetFilter = ExportCommandExtensions.GetAssetFilter("BTC");
var fiatFilter = ExportCommandExtensions.GetFiatFilter("USD");
var assetFiatPairs = ExportCommandExtensions.ValidateAssetFiatPair(new List<string> { "BTC-USD", "ETH-USD" });
var validOutputPaths = ExportCommandExtensions.ValidateOutputPath(new List<string> { "/path/to/output" });

Console.WriteLine($"Default Output Path: {outputPath}");
Console.WriteLine($"Format: {format}");
Console.WriteLine($"Days to Export: {daysToExport}");
Console.WriteLine($"Asset Filter: {assetFilter}");
Console.WriteLine($"Fiat Filter: {fiatFilter}");
Console.WriteLine($"Asset-Fiat Pairs: [{string.Join(", ", assetFiatPairs)}]");
Console.WriteLine($"Valid Output Paths: [{string.Join(", ", validOutputPaths)}]");
```

...

## SpreadAnalysisBenchmarks

The `SpreadAnalysisBenchmarks` class provides a set of benchmark methods for analyzing and computing statistics on spread data.

### Usage

```csharp
using BinanceP2pMonitor.Benchmarks;

var benchmark = new SpreadAnalysisBenchmarks();
benchmark.Setup();

var directSpread = benchmark.AnalyzeSpread_Direct();
Console.WriteLine($"Direct Spread Analysis: {directSpread}");

var (mean, stdDev) = benchmark.ComputeStatistics_Loop();
Console.WriteLine($"Loop Statistics: Mean={mean}, StdDev={stdDev}");

var anomalies = benchmark.FindAnomalies_ZScore();
Console.WriteLine($"Z-Score Anomalies: {anomalies}");

var anomaliesPool = benchmark.FindAnomalies_ZScore_ArrayPool();
Console.WriteLine($"Z-Score Anomalies (Array Pool): {anomaliesPool}");
```

...

## PriceMonitoringServiceExtensions

The `PriceMonitoringServiceExtensions` class provides utility methods for analyzing and retrieving current prices, best buy/sell prices, and price statistics. It allows users to monitor prices in real-time and make informed decisions.

### Usage

```csharp
using BinanceP2pMonitor.Services;

var priceMonitoringService = new PriceMonitoringServiceExtensions("BTC", "USD", 24);
var currentPrice = await priceMonitoringService.GetCurrentPriceAsync();
Console.WriteLine($"Current Price: {currentPrice}");

var filteredPrices = await priceMonitoringService.GetFilteredCurrentPricesAsync();
Console.WriteLine($"Filtered Current Prices: [{string.Join(", ", filteredPrices)}]");

var (bestBuyPrice, bestSellPrice, priceCount) = await priceMonitoringService.GetBestPricesAsync();
Console.WriteLine($"Best Buy Price: {bestBuyPrice}, Best Sell Price: {bestSellPrice}, Price Count: {priceCount}");

var priceStatistics = await priceMonitoringService.GetPriceStatisticsAsync();
if (priceStatistics != null)
{
    Console.WriteLine($"Average Buy Price: {priceStatistics.AverageBuyPrice}, Average Sell Price: {priceStatistics.AverageSellPrice}");
    Console.WriteLine($"Min Buy Price: {priceStatistics.MinBuyPrice}, Max Buy Price: {priceStatistics.MaxBuyPrice}");
    Console.WriteLine($"Min Sell Price: {priceStatistics.MinSellPrice}, Max Sell Price: {priceStatistics.MaxSellPrice}");
    Console.WriteLine($"Buy Price Volatility: {priceStatistics.BuyPriceVolatilityPercent}%, Sell Price Volatility: {priceStatistics.SellPriceVolatilityPercent}%");
    Console.WriteLine($"Price Count: {priceStatistics.PriceCount}, Last Updated: {priceStatistics.LastUpdated}");
}

var wouldTriggerAlert = await priceMonitoringService.WouldTriggerAlertAsync();
Console.WriteLine($"Would Trigger Alert: {wouldTriggerAlert}");
```

...

## DatabaseContextExtensions

`DatabaseContextExtensions` adds a collection of helper methods that make it easier to execute raw SQL against a `DatabaseContext`. The extensions cover non‑query commands, result‑set queries, scalar retrieval, typed single‑row queries, and transactional execution, all while handling parameters in a concise way.

### Usage

```csharp
using BinanceP2pMonitor.Data;

// Assume DatabaseContext implements IDisposable and has a parameter‑less constructor.
using var db = new DatabaseContext();

// Execute a non‑query command.
int affectedRows = db.ExecuteCommand(
    "DELETE FROM PriceHistory WHERE Timestamp < @maxAge",
    new { maxAge = DateTime.UtcNow.AddDays(-30) });

// Run a query that returns multiple rows as dictionaries.
IEnumerable<Dictionary<string, object>> rows = db.ExecuteQuery(
    "SELECT Asset, Fiat, Price FROM Prices WHERE Asset = @asset",
    new { asset = "BTC" });

// Retrieve a scalar integer.
int totalCount = db.QueryInt(
    "SELECT COUNT(*) FROM Prices");

// Retrieve a decimal value.
decimal avgPrice = db.QueryDecimal(
    "SELECT AVG(Price) FROM Prices WHERE Asset = @asset",
    new { asset = "BTC" });

// Retrieve a boolean flag.
bool isActive = db.QueryBool(
    "SELECT IsActive FROM Users WHERE Id = @id",
    new { id = 1 });

// Retrieve a nullable DateTime.
DateTime? lastUpdate = db.QueryDateTime(
    "SELECT MAX(UpdatedAt) FROM Prices");

// Retrieve a single strongly‑typed object.
var firstPrice = db.QuerySingle<PriceHistory>(
    "SELECT * FROM Prices ORDER BY Timestamp DESC LIMIT 1");

// Generic scalar retrieval.
long maxId = db.ExecuteScalar<long>(
    "SELECT MAX(Id) FROM Prices");

// Execute multiple commands inside a transaction.
db.ExecuteInTransaction(() =>
{
    db.ExecuteCommand(
        "INSERT INTO Prices (Asset, Fiat, Price) VALUES (@asset, @fiat, @price)",
        new { asset = "ETH", fiat = "USD", price = 1800.50m });

    db.ExecuteCommand(
        "UPDATE Statistics SET LastRun = @now",
        new { now = DateTime.UtcNow });
});
```

The example demonstrates the most common extension methods; you can mix and match them according to the shape of the data you need to work with.

...

## AppSettingsExtensions

The `AppSettingsExtensions` class provides utility methods for accessing and converting application settings values. It includes methods for checking notification configuration, retrieving monitoring intervals, alert thresholds, and lists of monitored assets and currencies.

### Usage

```csharp
using BinanceP2pMonitor.Configuration;

var settings = new AppSettings
{
    EnableTelegramNotifications = true,
    TelegramBotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    TelegramAdminChatId = "123456789",
    EnableWebhookNotifications = true,
    WebhookUrl = "https://example.com/webhook",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 5,
    HistoryRetentionDays = 30,
    DatabaseCommandTimeoutSeconds = 30,
    SpreadAnalysisHistoryHours = 24,
    DailySummaryHourUtc = 9,
    MaxAlertsPerUser = 3,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.8m,
    MonitoredAssets = new List<string> { "BTC", "ETH", "USDT" },
    MonitoredFiats = new List<string> { "USD", "EUR", "RUB" }
};

// Check notification configuration
bool isTelegramReady = settings.IsTelegramConfigured();
bool isWebhookReady = settings.IsWebhookConfigured();

// Get monitoring intervals
int monitoringIntervalMs = settings.GetMonitoringIntervalMs();
int alertCooldownSeconds = settings.GetAlertCooldownSeconds();
int historyRetentionHours = settings.GetHistoryRetentionHours();
int databaseTimeoutMs = settings.GetDatabaseCommandTimeoutMs();
int spreadAnalysisPeriodHours = settings.GetSpreadAnalysisHistoryPeriod();

// Check daily summary settings
bool isDailySummaryEnabled = settings.IsDailySummaryEnabled();
int dailySummaryLocalHour = settings.GetDailySummaryLocalHour();

// Get alert thresholds
int maxAlertsPerCooldown = settings.GetMaxAlertsPerCooldown();
decimal priceChangeThresholdPercent = settings.GetPriceChangeThresholdPercent();
decimal spreadThresholdPercent = settings.GetSpreadThresholdPercent();

// Get monitored lists
List<string> monitoredAssets = settings.GetMonitoredAssets();
List<string> monitoredFiats = settings.GetMonitoredFiats();

Console.WriteLine($"Telegram configured: {isTelegramReady}");
Console.WriteLine($"Webhook configured: {isWebhookReady}");
Console.WriteLine($"Monitoring interval: {monitoringIntervalMs}ms");
Console.WriteLine($"Alert cooldown: {alertCooldownSeconds}s");
Console.WriteLine($"History retention: {historyRetentionHours}h");
Console.WriteLine($"Daily summary enabled: {isDailySummaryEnabled} (hour: {dailySummaryLocalHour})");
Console.WriteLine($"Price change threshold: {priceChangeThresholdPercent}%");
Console.WriteLine($"Spread threshold: {spreadThresholdPercent}%");
Console.WriteLine($"Monitored assets: [{string.Join(", ", monitoredAssets)}]");
Console.WriteLine($"Monitored fiats: [{string.Join(", ", monitoredFiats)}]");
```

## License

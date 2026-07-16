# Binance P2P Monitor

A .NET application for monitoring Binance P2P (peer-to-peer) prices and spreads.

## Spread

The `Spread` type represents spread analysis data between buy and sell prices. It stores various metrics such as current spread percentage, average spread percentage, minimum and maximum spread percentages, sample count, and standard deviation. Here is an example of creating and using a `Spread` instance:

```csharp
using BinanceP2pMonitor.Models;

var spread = new Spread
{
  Asset = "USDT-BTC",
  Fiat = "USDT",
  CurrentSpreadPercent = 0.23m,
  AverageSpreadPercent = 0.15m,
  MinSpreadPercent = 0.05m,
  MaxSpreadPercent = 0.30m,
  SampleCount = 100,
  LastUpdatedAt = DateTime.UtcNow,
  CreatedAt = DateTime.UtcNow,
  StandardDeviation = 0.01m,
  PercentileRank = 0.5m
};

Console.WriteLine($"Current spread: {spread.CurrentSpreadPercent:F4}%");
Console.WriteLine($"Average spread: {spread.AverageSpreadPercent:F4}%");
Console.WriteLine($"Is high spread: {spread.IsHighSpread(1.5m)}");
Console.WriteLine($"Is low spread: {spread.IsLowSpread(0.3m)}");
Console.WriteLine($"Variance from average: {spread.GetVarianceFromAverage():F4}%");
Console.WriteLine($"Is normal: {spread.IsNormal()}");
Console.WriteLine($"Is valid: {spread.IsValid()}");
Console.WriteLine($"Risk level: {spread.GetRiskLevel()}");
```

## SpreadStatisticsReport

`SpreadStatisticsReport` is an immutable data object that summarizes statistical analysis of spread percentages over a configurable time window. It contains raw metrics (mean, median, variance, etc.) as well as helper methods to interpret the current spread in context.

```csharp
using System;
using BinanceP2pMonitor.Models;

var report = new SpreadStatisticsReport
{
  Asset = "BTC",
  Fiat = "USDT",
  TimeWindowHours = 24,
  SampleCount = 500,
  Mean = 0.12m,
  Median = 0.10m,
  StandardDeviation = 0.03m,
  Variance = 0.0009m,
  MinSpread = 0.05m,
  MaxSpread = 0.20m,
  Percentile5 = 0.06m,
  Percentile95 = 0.18m,
  CurrentSpread = 0.15m,
  ZScore = 1.0m,
  TrendSlope = 0.02m,
  AnalyzedAt = DateTime.UtcNow,
  IsAnomalous = false
};

Console.WriteLine($"Trend: {report.GetTrendLabel()}");
Console.WriteLine($"Critical? {report.IsCritical()}");
Console.WriteLine($"Above average? {report.IsAboveAverage()}");
```

## Price


The `Price` type represents real-time price data for a specific trading pair on Binance P2P. It stores buy and sell prices along with their percentage changes, timestamps, and metadata. The type provides methods for spread calculation, price validation, and JSON serialization.

```csharp
using BinanceP2pMonitor.Models;

var price = new Price
{
  Id = 1,
  Asset = "USDT-BTC",
  Fiat = "USDT",
  BuyPrice = 50000.50m,
  SellPrice = 50010.25m,
  BuyChangePercent = 0.25m,
  SellChangePercent = 0.30m,
  Timestamp = DateTime.UtcNow,
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow,
  Metadata = "{\"source\":\"binance_p2p\",\"version\":\"1.0\"}"
};

Console.WriteLine($"Pair: {price.Asset}-{price.Fiat}");
Console.WriteLine($"Buy: {price.BuyPrice:C}");
Console.WriteLine($"Sell: {price.SellPrice:C}");
Console.WriteLine($"Buy change: {price.BuyChangePercent:F2}%");
Console.WriteLine($"Sell change: {price.SellChangePercent:F2}%");
Console.WriteLine($"Spread: {price.CalculateSpread():F4}%");
Console.WriteLine($"Valid? {price.IsValid()}");
Console.WriteLine($"Different from reference? {price.IsDifferentFrom(50000.00m)}");
Console.WriteLine($"JSON: {price.ToJson()}");

// Parse from JSON
var parsedPrice = Price.FromJson(price.ToJson());
Console.WriteLine($"Parsed valid? {parsedPrice?.IsValid()}");
```

## Market


The `Market` type represents a trading pair on Binance P2P with comprehensive market data and monitoring capabilities. It tracks buy/sell prices, trading volume, offer counts, activity status, and provides methods for spread calculation and price validation.

```csharp
using BinanceP2pMonitor.Models;

var market = new Market
{
  Id = 1,
  Asset = "USDT-BTC",
  Fiat = "USDT",
  IsActive = true,
  IsMonitored = true,
  Description = "Bitcoin to Tether trading pair",
  LastBuyPrice = 50000.50m,
  LastSellPrice = 50010.25m,
  TotalOffers = 42,
  DailyVolume = 1500000,
  CreatedAt = DateTime.UtcNow.AddDays(-30),
  UpdatedAt = DateTime.UtcNow,
  LastPriceUpdateAt = DateTime.UtcNow.AddMinutes(-5),
  MonitoringPriority = 1
};

Console.WriteLine($"Pair ID: {market.GetPairId()}");
Console.WriteLine($"Current spread: {market.CalculateSpread():F4}%");
Console.WriteLine($"Price stale? {market.IsPriceStale(TimeSpan.FromMinutes(10))}");
Console.WriteLine($"Valid market? {market.IsValid()}");
Console.WriteLine($"Activity level: {market.GetActivityLevel()}");

// Update prices when new data arrives
market.UpdatePrices(50050.75m, 50060.50m);
```

## PriceAlert


The `PriceAlert` type represents a price alert configuration that monitors specific trading pairs for price threshold breaches. It tracks alert conditions, threshold values, trigger history, and provides methods to check if conditions are met and record alert events.

```csharp
using BinanceP2pMonitor.Models;

var alert = new PriceAlert
{
  Asset = "USDT-BTC",
  Fiat = "USDT",
  AlertType = AlertType.PriceAbove,
  Threshold = 51000.00m,
  Condition = AlertCondition.GreaterThan,
  IsEnabled = true,
  UserId = 123,
  Notes = "Notify when BTC price exceeds $51,000",
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

Console.WriteLine($"Alert: {alert.GetDescription()}");
Console.WriteLine($"Should trigger? {alert.ShouldTrigger(51500.00m)}");
Console.WriteLine($"Is valid? {alert.IsValid()}");

// Check if current price meets alert condition
if (alert.ShouldTrigger(51500.00m))
{
  Console.WriteLine("ALERT: Price threshold breached!");
  alert.RecordTrigger();
}

// Toggle alert status
alert.Toggle();
Console.WriteLine($"Alert enabled: {alert.IsEnabled}");
```

## AlertService

The `AlertService` manages price alert creation, retrieval, updates, and deletion. It handles alert triggering logic, notification delivery, and alert status management. This service integrates with repositories and notification channels to provide a complete alert management system for monitoring Binance P2P price thresholds.

```csharp
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
  .AddLogging()
  .AddSingleton<IAlertRepository, AlertRepository>()
  .AddSingleton<IUserRepository, UserRepository>()
  .AddSingleton<INotificationService, NotificationService>()
  .AddSingleton<IEventBus, EventBus>()
  .AddSingleton<AlertService>()
  .BuildServiceProvider();

var alertService = services.GetRequiredService<AlertService>();

// Create a new price alert
var newAlert = new PriceAlert
{
  Asset = "BTC",
  Fiat = "USDT",
  AlertType = AlertType.PriceAbove,
  Threshold = 51000.00m,
  Condition = AlertCondition.GreaterThan,
  IsEnabled = true,
  UserId = 123,
  Notes = "Notify when BTC price exceeds $51,000",
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

var alertId = await alertService.CreateAlertAsync(newAlert);
Console.WriteLine($"Alert created with ID: {alertId}");

// Get all alerts for a user
var userAlerts = await alertService.GetUserAlertsAsync(123);
Console.WriteLine($"User has {userAlerts.Count()} active alerts");

// Update an existing alert
newAlert.Threshold = 51500.00m;
var updateResult = await alertService.UpdateAlertAsync(newAlert);
Console.WriteLine($"Alert updated successfully: {updateResult}");

// Check if any alerts should trigger based on current prices
var triggeredAlerts = await alertService.CheckTriggersAsync();
Console.WriteLine($"Found {triggeredAlerts.Count()} alerts that need to trigger");

// Send notifications for triggered alerts
foreach (var alert in triggeredAlerts)
{
  await alertService.SendNotificationAsync(alert);
}

// Delete an alert
var deleteResult = await alertService.DeleteAlertAsync(alertId);
Console.WriteLine($"Alert deleted successfully: {deleteResult}");

// Get active alert count
var activeCount = await alertService.GetActiveAlertCountAsync();
Console.WriteLine($"Total active alerts: {activeCount}");

// Test if an alert would trigger without saving
var testAlert = new PriceAlert
{
  Asset = "ETH",
  Fiat = "USDT",
  AlertType = AlertType.PriceBelow,
  Threshold = 3000.00m,
  Condition = AlertCondition.LessThan,
  IsEnabled = true
};

var wouldTrigger = await alertService.TestAlertAsync(testAlert, 2950.00m);
Console.WriteLine($"Test alert would trigger: {wouldTrigger}");
```

## CommandFactory

The `CommandFactory` class provides centralized registration and creation of CLI commands within the Binance P2P Monitor application. It maintains a registry of available commands and allows dynamic command creation based on registered types. This enables extensible CLI functionality where new commands can be added without modifying the core application flow.

```csharp
using BinanceP2pMonitor.CLI;
using BinanceP2pMonitor.Commands;

// Create the command factory
var commandFactory = new CommandFactory();

// Register available commands
commandFactory.RegisterCommand<MonitorCommand>("monitor", "Start monitoring Binance P2P prices");
commandFactory.RegisterCommand<AlertCommand>("alert", "Manage price alerts");
commandFactory.RegisterCommand<BacktestCommand>("backtest", "Run price monitoring backtests");

// Check if a command is registered
bool hasMonitor = commandFactory.IsCommandRegistered("monitor");
Console.WriteLine($"Monitor command registered: {hasMonitor}");

// Create a command instance by name
var monitorCommand = commandFactory.CreateCommand("monitor");
if (monitorCommand != null)
{
    Console.WriteLine($"Created command: {monitorCommand.GetType().Name}");
    Console.WriteLine($"Command description: {commandFactory.GetAvailableCommands().FirstOrDefault(c => c == "monitor")?.Description}");
}

// Get all available commands
var availableCommands = commandFactory.GetAvailableCommands();
Console.WriteLine("Available commands:");
foreach (var cmd in availableCommands)
{
    Console.WriteLine($"- {cmd.Name}: {cmd.Description}");
}
```

## PriceMonitoringService

The `PriceMonitoringService` is the core service for monitoring Binance P2P prices in real-time. It provides comprehensive price monitoring functionality including retrieving current prices, updating prices, calculating averages, detecting significant changes, analyzing spreads, and managing WebSocket-based price monitoring. This service integrates with repositories, alert services, spread analysis, and WebSocket connections to provide a complete monitoring solution.

```csharp
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection (simplified example)
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IPriceRepository, PriceRepository>()
    .AddSingleton<IPriceHistoryService, PriceHistoryService>()
    .AddSingleton<IAlertService, AlertService>()
    .AddSingleton<ISpreadAnalysisService, SpreadAnalysisService>()
    .AddSingleton<IEventBus, EventBus>()
    .AddSingleton<IWebSocketService, WebSocketService>()
    .AddSingleton<AppSettings>(new AppSettings { EnableWebSocket = true, MonitoredAssets = new[] { "BTC", "ETH" }, MonitoredFiats = new[] { "USDT", "USDC" } })
    .AddSingleton<PriceMonitoringService>()
    .BuildServiceProvider();

var monitoringService = services.GetRequiredService<PriceMonitoringService>();

// Get current price for a trading pair
var currentPrice = await monitoringService.GetCurrentPriceAsync("BTC", "USDT");
Console.WriteLine($"Current BTC/USDT price: Buy={currentPrice?.BuyPrice:C}, Sell={currentPrice?.SellPrice:C}");

// Get all current prices
var allPrices = await monitoringService.GetAllCurrentPricesAsync();
Console.WriteLine($"Total active prices: {allPrices.Count()}");

// Update a price
var newPrice = new Price
{
    Asset = "BTC",
    Fiat = "USDT",
    BuyPrice = 50000.50m,
    SellPrice = 50010.25m,
    Timestamp = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
var updated = await monitoringService.UpdatePriceAsync(newPrice);
Console.WriteLine($"Price updated successfully: {updated}");

// Get average price over last 24 hours
var avgPrice = await monitoringService.GetAveragePriceAsync("BTC", "USDT", 24);
Console.WriteLine($"24h average BTC/USDT price: {avgPrice:C}");

// Get prices with significant change (e.g., > 2%)
var significantChanges = await monitoringService.GetPricesWithSignificantChangeAsync(2.0m);
Console.WriteLine($"Prices with >2% change: {significantChanges.Count()}");

// Analyze spread for a trading pair
var spreadAnalysis = await monitoringService.GetSpreadAnalysisAsync("BTC", "USDT");
if (spreadAnalysis != null)
{
    Console.WriteLine($"Current spread: {spreadAnalysis.CurrentSpreadPercent:F4}%");
    Console.WriteLine($"Spread risk level: {spreadAnalysis.GetRiskLevel()}");
}

// Start monitoring via WebSocket
await monitoringService.StartMonitoringAsync(CancellationToken.None);
Console.WriteLine("Price monitoring started...");

// Stop monitoring
await monitoringService.StopMonitoringAsync();
Console.WriteLine("Price monitoring stopped.");
```

## PriceHistoryService

The `PriceHistoryService` manages historical price data recording, retrieval, and analysis for Binance P2P trading pairs. It provides functionality to record prices, retrieve historical data, calculate price trends and statistics, and perform cleanup operations. This service is essential for tracking price movements over time and generating insights for trading decisions.

```csharp
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
.AddLogging()
.AddSingleton<IHistoryRepository, HistoryRepository>()
.AddSingleton<AppSettings>(new AppSettings { HistoryRetentionDays = 30 })
.AddSingleton<IPriceHistoryService, PriceHistoryService>()
.BuildServiceProvider();

var priceHistoryService = services.GetRequiredService<IPriceHistoryService>() as PriceHistoryService;

// Record a new price
var price = new Price
{
  Id = 1,
  Asset = "BTC",
  Fiat = "USDT",
  BuyPrice = 50000.50m,
  SellPrice = 50010.25m,
  BuyChangePercent = 0.25m,
  SellChangePercent = 0.30m,
  Timestamp = DateTime.UtcNow,
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

var recordId = await priceHistoryService.RecordPriceAsync(price);
Console.WriteLine($"Price recorded with ID: {recordId}");

// Get price history for a trading pair
var history = await priceHistoryService.GetHistoryAsync("BTC", "USDT", 24);
Console.WriteLine($"Retrieved {history.Count()} historical records");

// Calculate price trend over last 24 hours
var trend = await priceHistoryService.GetPriceTrendAsync("BTC", "USDT", 24);
Console.WriteLine($"24h price trend: {trend:F4}%");

// Get price statistics (high, low, average)
var (high, low, average) = await priceHistoryService.GetPriceStatsAsync("BTC", "USDT", 24);
Console.WriteLine($"Price stats - High: {high:C}, Low: {low:C}, Avg: {average:C}");

// Get detailed analysis with multiple metrics
var analysis = await priceHistoryService.GetDetailedAnalysisAsync("BTC", "USDT", 24);
Console.WriteLine($"Analysis for BTC/USDT:");
Console.WriteLine($"  High: {analysis["HighPrice"]}");
Console.WriteLine($"  Low: {analysis["LowPrice"]}");
Console.WriteLine($"  Average: {analysis["AveragePrice"]}");
Console.WriteLine($"  Trend: {analysis["Trend"]}%");
Console.WriteLine($"  Direction: {analysis["TrendDirection"]}");
Console.WriteLine($"  Record count: {analysis["RecordCount"]}");

// Clean up old history records (older than 30 days)
var cleanupResult = await priceHistoryService.CleanupOldHistoryAsync(30);
Console.WriteLine($"Cleanup completed: {cleanupResult}");

// Get total history count
var totalCount = await priceHistoryService.GetHistoryCountAsync();
Console.WriteLine($"Total history records: {totalCount}");
```

## IWebSocketService

The `IWebSocketService` interface defines a contract for WebSocket-based real-time price monitoring. It provides methods to connect/disconnect from WebSocket servers and subscribe/unsubscribe to specific trading pairs (asset/fiat combinations). Implementations receive price updates through the `OnPriceUpdate` event, which delivers `PriceUpdateEventArgs` containing buy/sell prices and timestamps.

```csharp
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IWebSocketService, WebSocketService>()
    .BuildServiceProvider();

var webSocketService = services.GetRequiredService<IWebSocketService>();

// Subscribe to price updates for a trading pair
await webSocketService.SubscribeToPairAsync("BTC", "USDT");

// Handle price update events
webSocketService.OnPriceUpdate += (sender, args) =>
{
    Console.WriteLine($"Price update for {args.Asset}-{args.Fiat}:");
    Console.WriteLine($"  Buy: {args.BuyPrice:C}, Sell: {args.SellPrice:C}");
    Console.WriteLine($"  Update time: {args.UpdateTime:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"  Spread: {(args.SellPrice - args.BuyPrice) / args.BuyPrice * 100:F4}%");
};

// Connect to WebSocket
await webSocketService.ConnectAsync();

// Disconnect when done
await webSocketService.DisconnectAsync();
```

## WebSocketService

The `WebSocketService` class provides a concrete implementation of `IWebSocketService` for connecting to Binance's WebSocket API and receiving real-time price updates. It handles connection management, automatic reconnection with exponential backoff, subscription management, and periodic keepalive pings to prevent server-side timeouts. The service parses incoming ticker messages and raises `OnPriceUpdate` events with the latest buy/sell prices.

```csharp
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IWebSocketService, WebSocketService>()
    .BuildServiceProvider();

var webSocketService = services.GetRequiredService<IWebSocketService>() as WebSocketService;

// Connect to Binance WebSocket endpoint
await webSocketService.ConnectAsync();

// Subscribe to multiple trading pairs
await webSocketService.SubscribeToPairAsync("BTC", "USDT");
await webSocketService.SubscribeToPairAsync("ETH", "USDT");
await webSocketService.SubscribeToPairAsync("BNB", "USDT");

// Handle price update events
webSocketService.OnPriceUpdate += (sender, args) =>
{
    Console.WriteLine($"Real-time price update:");
    Console.WriteLine($"  Pair: {args.Asset}-{args.Fiat}");
    Console.WriteLine($"  Buy: {args.BuyPrice:C}, Sell: {args.SellPrice:C}");
    Console.WriteLine($"  Spread: {(args.SellPrice - args.BuyPrice) / args.BuyPrice * 100:F4}%");
    Console.WriteLine($"  Timestamp: {args.UpdateTime:yyyy-MM-dd HH:mm:ss}");
};

// Disconnect when done (automatically disposes resources)
await webSocketService.DisconnectAsync();
```

## SpreadAnalysisService

The `SpreadAnalysisService` provides comprehensive spread analysis functionality for identifying trading opportunities and market anomalies. It calculates spreads between buy and sell prices, tracks historical spread patterns, identifies arbitrage opportunities across different fiat currencies, and detects anomalous spreads using statistical methods like Z-score analysis. This service is essential for arbitrage trading strategies and monitoring market inefficiencies.

```csharp
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IPriceRepository, PriceRepository>()
    .AddSingleton<IPriceHistoryService, PriceHistoryService>()
    .AddSingleton<AppSettings>(new AppSettings {
        SpreadAnalysisHistoryHours = 24,
        DefaultSpreadThreshold = 1.5m
    })
    .AddSingleton<ISpreadAnalysisService, SpreadAnalysisService>()
    .BuildServiceProvider();

var spreadAnalysisService = services.GetRequiredService<ISpreadAnalysisService>() as SpreadAnalysisService;

// Get spread analysis for a trading pair
var spreadAnalysis = await spreadAnalysisService.GetSpreadAnalysisAsync("BTC", "USDT");
if (spreadAnalysis != null)
{
    Console.WriteLine($"Spread analysis for BTC/USDT:");
    Console.WriteLine($" Current spread: {spreadAnalysis.CurrentSpreadPercent:F4}%");
    Console.WriteLine($" Average spread: {spreadAnalysis.AverageSpreadPercent:F4}%");
    Console.WriteLine($" Min spread: {spreadAnalysis.MinSpreadPercent:F4}%");
    Console.WriteLine($" Max spread: {spreadAnalysis.MaxSpreadPercent:F4}%");
    Console.WriteLine($" Sample count: {spreadAnalysis.SampleCount}");
    Console.WriteLine($" Standard deviation: {spreadAnalysis.StandardDeviation:F4}%");
    Console.WriteLine($" Risk level: {spreadAnalysis.GetRiskLevel()}");
}

// Get top spread opportunities (spreads above threshold)
var topOpportunities = await spreadAnalysisService.GetTopSpreadOpportunitiesAsync(limit: 5);
Console.WriteLine($"Top {topOpportunities.Count()} spread opportunities:");
foreach (var spread in topOpportunities)
{
    Console.WriteLine($" {spread.Asset}/{spread.Fiat}: {spread.CurrentSpreadPercent:F4}%");
}

// Analyze spread between two prices
var spreadPercent = await spreadAnalysisService.AnalyzeSpreadAsync(50000.50m, 50010.25m);
Console.WriteLine($"Spread between prices: {spreadPercent:F4}%");

// Update spread analysis
var updated = await spreadAnalysisService.UpdateSpreadAsync(spreadAnalysis);
Console.WriteLine($"Spread updated: {updated}");

// Get all spreads
var allSpreads = await spreadAnalysisService.GetAllSpreadsAsync();
Console.WriteLine($"Total spreads tracked: {allSpreads.Count}");

// Calculate cross-currency spread (e.g., BTC in USD vs EUR)
var crossSpread = await spreadAnalysisService.GetCrossCurrencySpreadAsync(
    asset: "BTC",
    baseFiat: "USD",
    quoteFiat: "EUR",
    conversionRate: 0.92m
);
if (crossSpread != null)
{
    Console.WriteLine($"Cross-currency spread for BTC:");
    Console.WriteLine($" Base: {crossSpread.BaseFiat}, Quote: {crossSpread.QuoteFiat}");
    Console.WriteLine($" Spread: {crossSpread.SpreadPercent:F4}%");
    Console.WriteLine($" Buy price (base): {crossSpread.BuyPriceInBaseFiat:C}");
    Console.WriteLine($" Sell price (converted): {crossSpread.SellPriceInBaseFiat:C}");
}

// Find anomalous spreads using Z-score analysis
var anomalies = await spreadAnalysisService.FindAnomalousSpreadAsync(zScoreThreshold: 2.0m);
Console.WriteLine($"Found {anomalies.Count()} anomalous spreads:");
foreach (var (asset, fiat, spread) in anomalies)
{
    Console.WriteLine($" {asset}/{fiat}: {spread:F4}%");
}
```

## IHistoricalSpreadAnalysisService

The `IHistoricalSpreadAnalysisService` interface provides statistical analysis of historical spread data across configurable time windows. It enables detection of anomalous spreads, percentile-based spread analysis, and rolling-window averages for monitoring price arbitrage opportunities and market anomalies.

```csharp
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IHistoryRepository, HistoryRepository>()
    .AddSingleton<ISpreadAnalysisService, SpreadAnalysisService>()
    .AddSingleton<IEventBus, EventBus>()
    .AddSingleton<AppSettings>(new AppSettings { DefaultSpreadThreshold = 1.5m })
    .AddSingleton<IHistoricalSpreadAnalysisService, HistoricalSpreadAnalysisService>()
    .BuildServiceProvider();

var analysisService = services.GetRequiredService<IHistoricalSpreadAnalysisService>();

// Analyze historical spread for a trading pair over 24 hours
var report = await analysisService.AnalyzeHistoricalSpreadAsync("BTC", "USDT", hours: 24);
if (report != null)
{
    Console.WriteLine($"Spread analysis for BTC/USDT:");
    Console.WriteLine($"  Mean: {report.Mean:F4}%");
    Console.WriteLine($"  Median: {report.Median:F4}%");
    Console.WriteLine($"  Standard deviation: {report.StandardDeviation:F4}%");
    Console.WriteLine($"  Current spread: {report.CurrentSpread:F4}%");
    Console.WriteLine($"  Z-score: {report.ZScore:F2}");
    Console.WriteLine($"  Anomalous: {report.IsAnomalous}");
    Console.WriteLine($"  Trend slope: {report.TrendSlope:F6}%/min");
}

// Detect statistical alerts across multiple trading pairs
var pairs = new[] { ("BTC", "USDT"), ("ETH", "USDT"), ("BNB", "USDT") };
var anomalies = await analysisService.DetectStatisticalAlertsAsync(pairs, zScoreThreshold: 2.5m);
Console.WriteLine($"Detected {anomalies.Count()} anomalous spreads");

// Get spread percentile (e.g., 95th percentile)
var percentile95 = await analysisService.GetSpreadPercentileAsync("BTC", "USDT", percentile: 95, hours: 24);
Console.WriteLine($"95th percentile spread: {percentile95:F4}%");

// Get rolling window averages (15-minute windows over 24 hours)
var windowAverages = await analysisService.GetRollingWindowAveragesAsync("BTC", "USDT", windowSizeMinutes: 15, hours: 24);
Console.WriteLine($"Rolling window averages: {windowAverages.Count()} data points");
foreach (var (windowEnd, avgSpread) in windowAverages.Take(5))
{
    Console.WriteLine($"  {windowEnd:yyyy-MM-dd HH:mm:ss}: {avgSpread:F4}%");
}
```

## CommandContext

`CommandContext` carries all information required to execute a CLI command: the command name, raw arguments, parsed options and flags, a service provider for dependency resolution, and a cancellation token for graceful shutdown. It also offers helper methods to query options/flags and retrieve services from the injected `IServiceProvider`.

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using BinanceP2pMonitor.CLI;
using Microsoft.Extensions.DependencyInjection;

// Build a simple service provider (registering a string for demo purposes)
var services = new ServiceCollection()
    .AddSingleton<string>("demo-service")
    .BuildServiceProvider();

// Create a command context
var context = new CommandContext
{
    CommandName = "monitor",
    Arguments = new[] { "BTC", "USDT" },
    Options = new Dictionary<string, string> { ["interval"] = "5m" },
    Flags = new Dictionary<string, string> { ["verbose"] = "" },
    ServiceProvider = services,
    CancellationToken = CancellationToken.None
};

// Use helper methods
if (context.HasOption("interval"))
{
    var interval = context.GetOption("interval", "1m");
    Console.WriteLine($"Running with interval: {interval}");
}

if (context.HasFlag("verbose"))
{
    Console.WriteLine("Verbose output enabled.");
}

// Resolve a registered service
var demo = context.GetRequiredService<string>();
Console.WriteLine($"Resolved service value: {demo}");
```

## NumericExtensions

The `NumericExtensions` class provides extension methods for performing common numeric operations on decimal values. These methods include rounding, percentage calculations, range checks, and formatting utilities that are frequently used throughout the Binance P2P Monitor application for price calculations, spread analysis, and data validation.

```csharp
using BinanceP2pMonitor.Utilities;

// Round a price to 2 decimal places for display
var roundedPrice = 50000.5678m.RoundTo(2);
Console.WriteLine($"Rounded price: {roundedPrice:C}"); // $50,000.57

// Check if a price is within 2% of a target price
var isWithinRange = 50100.50m.IsWithinPercentage(50000.00m, 2.0m);
Console.WriteLine($"Within 2% range: {isWithinRange}"); // True

// Calculate percentage change between old and new prices
var priceChange = 51000.75m.CalculatePercentageChange(50000.00m);
Console.WriteLine($"Price change: {priceChange:F2}%"); // 2.00%

// Clamp a value between minimum and maximum bounds
var clampedValue = 150.75m.Clamp(100.00m, 200.00m);
Console.WriteLine($"Clamped value: {clampedValue:C}"); // $150.75

// Check if a price change is positive or negative
var isPositiveChange = 2.5m.IsPositive();
var isNegativeChange = (-1.2m).IsNegative();
Console.WriteLine($"Positive change: {isPositiveChange}, Negative change: {isNegativeChange}"); // True, True

// Check if a value is within a specific range
var isInRange = 150.50m.IsBetween(100.00m, 200.00m);
Console.WriteLine($"In range: {isInRange}"); // True

// Calculate absolute percentage difference between two prices
var priceDifference = 50000.50m.AbsolutePercentageDifference(50100.75m);
Console.WriteLine($"Price difference: {priceDifference:F2}%"); // 0.20%

// Format a price as currency string
var formattedPrice = 50000.50m.ToCurrencyString("₿");
Console.WriteLine($"Formatted price: {formattedPrice}"); // ₿50,000.50

// Format a price with specific precision
var precisePrice = 50000.56789m.FormatPrecision(4);
Console.WriteLine($"Precise price: {precisePrice}"); // 50000.5679
```

## DatabaseContext

The `DatabaseContext` class serves as the primary data access layer for the Binance P2P Monitor application. It provides methods for executing SQL commands, queries, and managing SQLite database connections. The context handles database initialization, connection management, and provides various execution methods for interacting with the application's SQLite database.

```csharp
using BinanceP2pMonitor.Data;
using Microsoft.Data.Sqlite;

// Create a new DatabaseContext instance
var databaseContext = new DatabaseContext();

// Initialize the database (creates tables if they don't exist)
databaseContext.Initialize();

// Get the underlying SQLite connection
using var connection = databaseContext.GetConnection();
Console.WriteLine($"Database connection state: {connection.State}");

// Execute a raw SQL command (e.g., CREATE TABLE, INSERT, UPDATE, DELETE)
var createTableResult = databaseContext.ExecuteCommand(
    @"CREATE TABLE IF NOT EXISTS TestPrices (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Asset TEXT NOT NULL,
        Fiat TEXT NOT NULL,
        BuyPrice DECIMAL(18, 2) NOT NULL,
        SellPrice DECIMAL(18, 2) NOT NULL,
        Timestamp DATETIME NOT NULL,
        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
    )"
);
Console.WriteLine($"Table created: {createTableResult > 0}");

// Execute a SELECT query and read results
using var reader = databaseContext.ExecuteReader(
    @"SELECT Id, Asset, Fiat, BuyPrice, SellPrice, Timestamp 
      FROM TestPrices 
     WHERE Asset = @asset AND Fiat = @fiat",
    new SqliteParameter("@asset", "BTC"),
    new SqliteParameter("@fiat", "USDT")
);

while (reader.Read())
{
    var id = reader.GetInt32(0);
    var asset = reader.GetString(1);
    var fiat = reader.GetString(2);
    var buyPrice = reader.GetDecimal(3);
    var sellPrice = reader.GetDecimal(4);
    var timestamp = reader.GetDateTime(5);
    
    Console.WriteLine($"Price record #{id}: {asset}/{fiat} = Buy:{buyPrice:C}, Sell:{sellPrice:C} at {timestamp}");
}

// Execute a scalar query to get a single value
var count = databaseContext.ExecuteScalar(
    @"SELECT COUNT(*) FROM TestPrices WHERE Asset = @asset",
    new SqliteParameter("@asset", "BTC")
);
Console.WriteLine($"Total BTC price records: {count}");

// Execute an INSERT command with parameters
var insertResult = databaseContext.ExecuteCommand(
    @"INSERT INTO TestPrices (Asset, Fiat, BuyPrice, SellPrice, Timestamp)
      VALUES (@asset, @fiat, @buyPrice, @sellPrice, @timestamp)",
    new SqliteParameter("@asset", "ETH"),
    new SqliteParameter("@fiat", "USDT"),
    new SqliteParameter("@buyPrice", 3500.75m),
    new SqliteParameter("@sellPrice", 3502.50m),
    new SqliteParameter("@timestamp", DateTime.UtcNow)
);
Console.WriteLine($"Rows inserted: {insertResult}");

// The DatabaseContext implements IDisposable for proper resource cleanup
databaseContext.Dispose();
```

## AppSettings

The `AppSettings` class defines the application configuration for the Binance P2P Monitor. It contains all essential settings including database connection strings, API credentials, monitoring intervals, alert thresholds, and feature toggles. This configuration is typically loaded from the `AppSettings` section of `appsettings.json` and used throughout the application for centralized configuration management.

```csharp
using BinanceP2pMonitor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Create configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Setup dependency injection
var services = new ServiceCollection()
    .Configure<AppSettings>(configuration.GetSection("AppSettings"))
    .AddOptions<AppSettings>()
    .ValidateDataAnnotations()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateOnStart()
    .Services
    .BuildServiceProvider();

// Resolve the settings
var appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;

// Use the configuration
Console.WriteLine($"Database: {appSettings.DatabaseConnectionString}");
Console.WriteLine($"Binance API enabled: {appSettings.BinanceApiKey != null}");
Console.WriteLine($"Monitoring interval: {appSettings.MonitoringIntervalSeconds} seconds");
Console.WriteLine($"Alert cooldown: {appSettings.AlertCooldownMinutes} minutes");
Console.WriteLine($"Spread threshold: {appSettings.DefaultSpreadThreshold}%");
Console.WriteLine($"WebSocket enabled: {appSettings.EnableWebSocket}");
Console.WriteLine($"Telegram notifications: {appSettings.EnableTelegramNotifications}");
Console.WriteLine($"Auto cleanup enabled: {appSettings.EnableAutoCleanup}");
Console.WriteLine($"Daily summary hour (UTC): {appSettings.DailySummaryHourUtc}");

// Example AppSettings configuration
var exampleSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    BinanceApiKey = "your-binance-api-key",
    BinanceApiSecret = "your-binance-api-secret",
    TelegramBotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    TelegramAdminChatId = "-1001234567890",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30,
    MaxHistoryRecords = 100000,
    DatabaseCommandTimeoutSeconds = 30,
    SpreadAnalysisHistoryHours = 24,
    EnableWebSocket = true,
    EnableTelegramNotifications = true,
    EnableAutoCleanup = true,
    DailySummaryHourUtc = 14,
    WebhookUrl = "https://your-webhook-endpoint.com/api/alerts",
    EnableWebhookNotifications = false
};
```

## BinanceP2PMonitorOptions

```csharp
using BinanceP2pMonitor.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Create configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// Setup dependency injection
var services = new ServiceCollection()
    .Configure<BinanceP2PMonitorOptions>(configuration.GetSection("AppSettings"))
    .AddOptions<BinanceP2PMonitorOptions>()
    .ValidateDataAnnotations()
    .Bind(configuration.GetSection("AppSettings"))
    .ValidateOnStart()
    .Services
    .BuildServiceProvider();

// Resolve the options
var options = services.GetRequiredService<IOptions<BinanceP2PMonitorOptions>>().Value;

// Use the configuration
Console.WriteLine($"Database: {options.DatabaseConnectionString}");
Console.WriteLine($"Monitoring interval: {options.MonitoringIntervalSeconds} seconds");
Console.WriteLine($"Spread threshold: {options.DefaultSpreadThreshold}%");
Console.WriteLine($"WebSocket enabled: {options.EnableWebSocket}");
Console.WriteLine($"Telegram notifications: {options.EnableTelegramNotifications}");
Console.WriteLine($"Log level: {options.LogLevel}");
Console.WriteLine($"Monitored assets: {string.Join(", ", options.MonitoredAssets)}");
Console.WriteLine($"Monitored fiats: {string.Join(", ", options.MonitoredFiats)}");

// Example configuration values
var exampleOptions = new BinanceP2PMonitorOptions
{
    DatabaseConnectionString = "Data Source=binance-p2p.db",
    BinanceApiKey = "your-api-key-here",
    BinanceApiSecret = "your-api-secret-here",
    TelegramBotToken = "123456:ABC-DEF1234ghIkl-zyx57W2v1u123ew11",
    TelegramAdminChatId = "-1001234567890",
    MonitoringIntervalSeconds = 60,
    AlertCooldownMinutes = 10,
    MaxAlertsPerUser = 50,
    DefaultPriceChangeThreshold = 3.0,
    DefaultSpreadThreshold = 2.0,
    HistoryRetentionDays = 90,
    MaxHistoryRecords = 500000,
    DatabaseCommandTimeoutSeconds = 60,
    EnableWebSocket = true,
    EnableTelegramNotifications = true,
    EnableAutoCleanup = true,
    DailySummaryHourUtc = 12,
    LogLevel = "Debug",
    LogPath = "/var/log/binance-p2p",
    MonitoredAssets = new[] { "BTC", "ETH", "BNB", "SOL", "ADA" },
    MonitoredFiats = new[] { "USD", "EUR", "GBP", "USDT", "BUSD" }
};
```

## AlertRepository

The `AlertRepository` class provides data access methods for storing, retrieving, updating, and deleting price alert records in the Binance P2P Monitor application. It serves as the primary interface for interacting with price alert data in the database, offering methods to fetch alerts by ID, user ID, asset/fiat combinations, and active status. The repository supports comprehensive alert management including enabling/disabling alerts, tracking trigger history, and managing user-specific alert configurations.

```csharp
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IAlertRepository, AlertRepository>()
    .BuildServiceProvider();

var alertRepository = services.GetRequiredService<IAlertRepository>() as AlertRepository;

// Add a new price alert
var newAlert = new PriceAlert
{
    Asset = "BTC",
    Fiat = "USDT",
    AlertType = AlertType.PriceAbove,
    Threshold = 51000.00m,
    Condition = AlertCondition.GreaterThan,
    IsEnabled = true,
    UserId = 123,
    Notes = "Notify when BTC price exceeds $51,000",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

var alertId = await alertRepository.AddAsync(newAlert);
Console.WriteLine($"Alert created with ID: {alertId}");

// Get an alert by ID
var retrievedAlert = await alertRepository.GetByIdAsync(alertId);
if (retrievedAlert != null)
{
    Console.WriteLine($"Retrieved alert: {retrievedAlert.GetDescription()}");
    Console.WriteLine($"Threshold: {retrievedAlert.Threshold:C}");
    Console.WriteLine($"Enabled: {retrievedAlert.IsEnabled}");
}

// Get all enabled alerts (alerts that are active and not disabled)
var enabledAlerts = await alertRepository.GetEnabledAlertsAsync();
Console.WriteLine($"Total enabled alerts: {enabledAlerts.Count()}");

// Get all alerts for a specific user
var userAlerts = await alertRepository.GetUserAlertsAsync(123);
Console.WriteLine($"User has {userAlerts.Count()} active alerts");

// Get alerts for a specific asset/fiat combination
var btcAlerts = await alertRepository.GetAlertsByAssetAndFiatAsync("BTC", "USDT");
Console.WriteLine($"BTC/USDT alerts: {btcAlerts.Count()}");

// Update an existing alert
if (retrievedAlert != null)
{
    retrievedAlert.Threshold = 51500.00m;
    retrievedAlert.UpdatedAt = DateTime.UtcNow;
    
    var updateResult = await alertRepository.UpdateAsync(retrievedAlert);
    Console.WriteLine($"Alert updated successfully: {updateResult}");
}

// Delete an alert
var deleteResult = await alertRepository.DeleteAsync(alertId);
Console.WriteLine($"Alert deleted successfully: {deleteResult}");

// Delete all alerts for a specific user
var userDeleteResult = await alertRepository.DeleteUserAlertsAsync(123);
Console.WriteLine($"User alerts deleted: {userDeleteResult}");

// Get the count of active alerts for a user
var alertCount = await alertRepository.GetUserAlertCountAsync(123);
Console.WriteLine($"User has {alertCount} active alerts");
```

## PriceRepository

The `PriceRepository` class provides data access methods for storing, retrieving, updating, and deleting price records in the Binance P2P Monitor application. It serves as the primary interface for interacting with price data in the database, offering methods to fetch prices by ID, asset/fiat combinations, active status, and time-based queries. The repository also includes methods for calculating average prices and detecting price changes over time periods.

```csharp
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<IPriceRepository, PriceRepository>()
    .BuildServiceProvider();

var priceRepository = services.GetRequiredService<IPriceRepository>() as PriceRepository;

// Add a new price record
var newPrice = new Price
{
    Asset = "BTC",
    Fiat = "USDT",
    BuyPrice = 50000.50m,
    SellPrice = 50010.25m,
    BuyChangePercent = 0.25m,
    SellChangePercent = 0.30m,
    Timestamp = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    Metadata = "{\"source\":\"binance_p2p\",\"version\":\"1.0\"}"
};

var priceId = await priceRepository.AddAsync(newPrice);
Console.WriteLine($"Price added with ID: {priceId}");

// Get a price by ID
var retrievedPrice = await priceRepository.GetByIdAsync(priceId);
if (retrievedPrice != null)
{
    Console.WriteLine($"Retrieved price: {retrievedPrice.Asset}/{retrievedPrice.Fiat}");
    Console.WriteLine($"Buy: {retrievedPrice.BuyPrice:C}, Sell: {retrievedPrice.SellPrice:C}");
}

// Get the latest price for a specific asset/fiat combination
var latestPrice = await priceRepository.GetLatestByAssetAndFiatAsync("BTC", "USDT");
Console.WriteLine($"Latest BTC/USDT price: {latestPrice?.BuyPrice:C}");

// Get all active prices (prices that haven't been deleted)
var allActivePrices = await priceRepository.GetAllActiveAsync();
Console.WriteLine($"Total active prices: {allActivePrices.Count()}");

// Get prices by asset (e.g., all BTC prices)
var btcPrices = await priceRepository.GetByAssetAsync("BTC");
Console.WriteLine($"BTC price records: {btcPrices.Count()}");

// Get prices by fiat (e.g., all USDT prices)
var usdtPrices = await priceRepository.GetByFiatAsync("USDT");
Console.WriteLine($"USDT price records: {usdtPrices.Count()}");

// Get prices that changed since a specific time
var pricesChangedSince = await priceRepository.GetPricesChangedSinceAsync(DateTime.UtcNow.AddHours(-1));
Console.WriteLine($"Prices changed in last hour: {pricesChangedSince.Count()}");

// Update an existing price
if (retrievedPrice != null)
{
    retrievedPrice.BuyPrice = 50100.75m;
    retrievedPrice.SellPrice = 50110.50m;
    retrievedPrice.UpdatedAt = DateTime.UtcNow;
    
    var updateResult = await priceRepository.UpdateAsync(retrievedPrice);
    Console.WriteLine($"Price updated successfully: {updateResult}");
}

// Calculate average price for a trading pair
var averagePrice = await priceRepository.GetAveragePriceAsync("BTC", "USDT");
Console.WriteLine($"Average BTC/USDT price: {averagePrice:C}");

// Delete a price record (soft delete)
var deleteResult = await priceRepository.DeleteAsync(priceId);
Console.WriteLine($"Price deleted successfully: {deleteResult}");

// Get prices with significant changes (> 2% change in buy or sell price)
var significantChanges = await priceRepository.GetPricesChangedSinceAsync(
    DateTime.UtcNow.AddHours(-24),
    minChangePercent: 2.0m
);
Console.WriteLine($"Prices with >2% change in last 24h: {significantChanges.Count()}");
```

## HistoryRepository

The `HistoryRepository` class provides data access methods for storing, retrieving, updating, and deleting historical price data in the Binance P2P Monitor application. It serves as the primary interface for interacting with historical price records, offering methods to fetch price history by ID, asset/fiat combinations, date ranges, and recent history queries. The repository also includes methods for calculating price statistics (highest/lowest prices), counting total history records, and managing data retention through automatic cleanup of old records.

```csharp
using BinanceP2pMonitor.Repositories;

// ... rest of HistoryRepository content ...
```

## MemoryCache

The `MemoryCache` class provides an in-memory caching mechanism for storing and retrieving frequently accessed data in the Binance P2P Monitor application. It supports asynchronous operations for getting, setting, and removing cache entries, and provides methods to check for existence, clear the cache, and get or create values with automatic cache population. The cache automatically manages expiration based on absolute time-to-live (TTL) and provides a simple interface for managing application-wide caching needs.

```csharp
using BinanceP2pMonitor.Caching;
using System;

// Create a new MemoryCache instance
var cache = new MemoryCache(TimeSpan.FromMinutes(5));

// Get a value from cache (returns null if not found)
var cachedValue = await cache.GetAsync<string>("config_key");
Console.WriteLine($"Cached value: {cachedValue ?? "null"}");

// Set a value in cache with 5-minute expiration
await cache.SetAsync("config_key", "configuration_value");
Console.WriteLine("Value cached successfully");

// Check if a key exists in cache
var exists = await cache.ExistsAsync("config_key");
Console.WriteLine($"Key exists: {exists}");

// Get or create a value with automatic cache population
var value = await cache.GetOrCreateAsync("expensive_operation_key", async () => {
    // This expensive operation will only run if the key doesn't exist
    await Task.Delay(100); // Simulate expensive operation
    return "expensive_result";
});
Console.WriteLine($"Value from cache or created: {value}");

// Remove a value from cache
await cache.RemoveAsync("config_key");
Console.WriteLine("Value removed from cache");

// Check if key still exists
var removedExists = await cache.ExistsAsync("config_key");
Console.WriteLine($"Key exists after removal: {removedExists}");

// Clear the entire cache
await cache.ClearAsync();
Console.WriteLine("Cache cleared");

// Access the underlying cache value directly (if needed)
var directValue = cache.Value;
Console.WriteLine($"Underlying cache object: {directValue}");

// Access expiration information
if (cache.ExpiresAt.HasValue)
{
    Console.WriteLine($"Cache expires at: {cache.ExpiresAt.Value}");
}

// Dispose the cache when done (implements IDisposable)
cache.Dispose();
Console.WriteLine("Cache disposed");
```

## TradeOfferRepository

The `TradeOfferRepository` class provides data access methods for managing trade offer data from Binance P2P. It serves as the primary interface for interacting with trade offers in the database, offering methods to retrieve, add, update, and delete trade offers. The repository includes functionality to fetch offers by ID, Binance offer ID, asset/fiat combinations, trade type, and to retrieve the best available offers based on price and trader rating. It also provides aggregate methods for counting total offers and calculating average prices.

```csharp
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<ITradeOfferRepository, TradeOfferRepository>()
    .BuildServiceProvider();

var tradeOfferRepository = services.GetRequiredService<ITradeOfferRepository>() as TradeOfferRepository;

// Add a new trade offer
var newOffer = new TradeOffer
{
    OfferIdFromBinance = "123456789",
    Asset = "USDT",
    Fiat = "BTC",
    TradeType = Constants.TradeType.SELL,
    Price = 50000.50m,
    MinAmount = 0.001m,
    MaxAmount = 1.0m,
    TraderRating = 4.8m,
    CompletedTrades = 42,
    PaymentMethods = "Tether, Bank Transfer",
    IsActive = true,
    Timestamp = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

var offerId = await tradeOfferRepository.AddAsync(newOffer);
Console.WriteLine($"Trade offer added with ID: {offerId}");

// Get a trade offer by ID
var retrievedOffer = await tradeOfferRepository.GetByIdAsync(offerId);
if (retrievedOffer != null)
{
    Console.WriteLine($"Retrieved offer: {retrievedOffer.Asset}/{retrievedOffer.Fiat}");
    Console.WriteLine($"Price: {retrievedOffer.Price:C}");
    Console.WriteLine($"Trader rating: {retrievedOffer.TraderRating}");
}

// Get trade offers by asset and fiat
var btcOffers = await tradeOfferRepository.GetByAssetAndFiatAsync("USDT", "BTC");
Console.WriteLine($"USDT/BTC offers: {btcOffers.Count()}");

// Get all active trade offers
var activeOffers = await tradeOfferRepository.GetAllActiveAsync();
Console.WriteLine($"Total active offers: {activeOffers.Count()}");

// Get best offers (top 10 by price)
var bestOffers = await tradeOfferRepository.GetBestOffersAsync("USDT", "BTC", limit: 10);
Console.WriteLine($"Best offers: {bestOffers.Count()}");

// Get offers by trade type
var sellOffers = await tradeOfferRepository.GetByTradeTypeAsync((int)Constants.TradeType.SELL);
Console.WriteLine($"Sell offers: {sellOffers.Count()}");

// Get total offers count for a trading pair
var totalCount = await tradeOfferRepository.GetTotalOffersCountAsync("USDT", "BTC");
Console.WriteLine($"Total offers for USDT/BTC: {totalCount}");

// Calculate average price for a trading pair
var averagePrice = await tradeOfferRepository.GetAveragePriceAsync("USDT", "BTC");
Console.WriteLine($"Average price: {averagePrice:C}");

// Update an existing offer
if (retrievedOffer != null)
{
    retrievedOffer.Price = 50100.75m;
    retrievedOffer.UpdatedAt = DateTime.UtcNow;
    
    var updateResult = await tradeOfferRepository.UpdateAsync(retrievedOffer);
    Console.WriteLine($"Offer updated successfully: {updateResult}");
}

// Delete an offer
var deleteResult = await tradeOfferRepository.DeleteAsync(offerId);
Console.WriteLine($"Offer deleted successfully: {deleteResult}");
```

## HistoryRepository
using BinanceP2pMonitor.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
.AddLogging()
.AddSingleton<IHistoryRepository, HistoryRepository>()
.BuildServiceProvider();

var historyRepository = services.GetRequiredService<IHistoryRepository>() as HistoryRepository;

// Add a new historical price record
var newHistoryRecord = new PriceHistory
{
  Asset = "BTC",
  Fiat = "USDT",
  BuyPrice = 50000.50m,
  SellPrice = 50010.25m,
  BuyChangePercent = 0.25m,
  SellChangePercent = 0.30m,
  Timestamp = DateTime.UtcNow,
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

var historyId = await historyRepository.AddAsync(newHistoryRecord);
Console.WriteLine($"History record added with ID: {historyId}");

// Get a history record by ID
var retrievedHistory = await historyRepository.GetByIdAsync(historyId);
if (retrievedHistory != null)
{
  Console.WriteLine($"Retrieved history: {retrievedHistory.Asset}/{retrievedHistory.Fiat}");
  Console.WriteLine($"Buy: {retrievedHistory.BuyPrice:C}, Sell: {retrievedHistory.SellPrice:C}");
  Console.WriteLine($"Timestamp: {retrievedHistory.Timestamp:yyyy-MM-dd HH:mm:ss}");
}

// Get price history for a specific asset/fiat combination
var btcHistory = await historyRepository.GetHistoryByAssetAndFiatAsync("BTC", "USDT");
Console.WriteLine($"BTC/USDT history records: {btcHistory.Count()}");

// Get recent history records (e.g., last 100 records)
var recentHistory = await historyRepository.GetRecentHistoryAsync(limit: 100);
Console.WriteLine($"Recent history records: {recentHistory.Count()}");

// Get history records within a specific date range
var dateRangeHistory = await historyRepository.GetHistoryByDateRangeAsync(
  DateTime.UtcNow.AddDays(-7),
  DateTime.UtcNow
);
Console.WriteLine($"History records from last 7 days: {dateRangeHistory.Count()}");

// Get the highest price ever recorded for a trading pair
var highestPrice = await historyRepository.GetHighestPriceAsync("BTC", "USDT");
Console.WriteLine($"Highest BTC/USDT price: {highestPrice:C}");

// Get the lowest price ever recorded for a trading pair
var lowestPrice = await historyRepository.GetLowestPriceAsync("BTC", "USDT");
Console.WriteLine($"Lowest BTC/USDT price: {lowestPrice:C}");

// Get the total count of all history records
var totalCount = await historyRepository.GetTotalHistoryCountAsync();
Console.WriteLine($"Total history records: {totalCount}");

// Delete old records (older than 30 days)
var cleanupResult = await historyRepository.DeleteOldRecordsAsync(30);
Console.WriteLine($"Old records deleted: {cleanupResult}");

// Update an existing history record
if (retrievedHistory != null)
{
  retrievedHistory.BuyPrice = 50100.75m;
  retrievedHistory.SellPrice = 50110.50m;
  retrievedHistory.UpdatedAt = DateTime.UtcNow;

  var updateResult = await historyRepository.UpdateAsync(retrievedHistory);
  Console.WriteLine($"History record updated successfully: {updateResult}");
}

// Delete a specific history record
var deleteResult = await historyRepository.DeleteAsync(historyId);
Console.WriteLine($"History record deleted successfully: {deleteResult}");
```

## BacktestOptions

// ... rest of content ...

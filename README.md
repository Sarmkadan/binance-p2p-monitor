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

## StringExtensions

The `StringExtensions` class provides utility extension methods for common string operations including text formatting, parsing, and manipulation. These methods are frequently used throughout the Binance P2P Monitor application for data processing, API response handling, and user input validation.

```csharp
using BinanceP2pMonitor.Utilities;

// Truncate a long string to fit display constraints
var longText = "This is a very long error message that needs to be truncated for display purposes";
var truncated = longText.Truncate(30);
Console.WriteLine(truncated); // "This is a very long error..."

// Split camelCase identifiers for better readability
var camelCaseText = "priceAlertConfiguration";
var splitText = camelCaseText.SplitCamelCase();
Console.WriteLine(splitText); // "Price Alert Configuration"

// Convert between naming conventions
var pascalCase = "PriceAlertThreshold";
var snakeCase = pascalCase.ToSnakeCase();
Console.WriteLine(snakeCase); // "price_alert_threshold"

var backToPascal = snakeCase.ToPascalCase();
Console.WriteLine(backToPascal); // "PriceAlertThreshold"

// Check if a string contains any of multiple search terms
var searchText = "BTC USDT price monitoring alert";
var containsBtc = searchText.ContainsAny(StringComparison.OrdinalIgnoreCase, "btc", "eth", "bnb");
Console.WriteLine(containsBtc); // True

// Check if string is numeric (digits only)
var numericString = "1234567890";
var isNumeric = numericString.IsNumeric();
Console.WriteLine(isNumeric); // True

// Safely parse strings to numeric types
var decimalString = "50000.50";
var parsedDecimal = decimalString.ToDecimalOrNull();
Console.WriteLine(parsedDecimal); // 50000.50

var intString = "42";
var parsedInt = intString.ToIntOrNull();
Console.WriteLine(parsedInt); // 42

// Mask sensitive information for logging
var apiKey = "sk_live_1234567890abcdef";
var maskedKey = apiKey.Mask(showChars: 8);
Console.WriteLine(maskedKey); // "sk_live_************"
```

## DateTimeExtensions

The `DateTimeExtensions` class provides utility extension methods for common DateTime operations including Unix timestamp conversion, time formatting, and date/time rounding. These methods are frequently used throughout the Binance P2P Monitor application for API timestamp handling, price history analysis, and scheduling operations.

```csharp
using BinanceP2pMonitor.Utilities;

// Convert current time to Unix timestamp in milliseconds
var timestampMs = DateTime.UtcNow.ToUnixTimestampMs();
Console.WriteLine($"Current Unix timestamp (ms): {timestampMs}");

// Convert Unix timestamp back to DateTime
var dateTime = DateTimeExtensions.FromUnixTimestampMs(timestampMs);
Console.WriteLine($"Converted back: {dateTime:yyyy-MM-dd HH:mm:ss}");

// Get human-readable time difference
var priceUpdateTime = DateTime.UtcNow.AddMinutes(-5);
var timeAgo = priceUpdateTime.GetTimeAgoString();
Console.WriteLine($"Price updated {timeAgo}");

// Round to nearest 15-minute interval for scheduled monitoring
var monitoringTime = DateTime.UtcNow.RoundTo(TimeSpan.FromMinutes(15));
Console.WriteLine($"Next monitoring window: {monitoringTime:yyyy-MM-dd HH:mm:ss}");

// Get start/end of day for daily price aggregation
var startOfDay = DateTime.UtcNow.StartOfDay();
var endOfDay = DateTime.UtcNow.EndOfDay();
Console.WriteLine($"Price aggregation window: {startOfDay:yyyy-MM-dd HH:mm:ss} to {endOfDay:yyyy-MM-dd HH:mm:ss}");

// Get start of week for weekly reports
var startOfWeek = DateTime.UtcNow.StartOfWeek();
Console.WriteLine($"Week starts: {startOfWeek:yyyy-MM-dd}");

// Get start/end of month for monthly statistics
var startOfMonth = DateTime.UtcNow.StartOfMonth();
var endOfMonth = DateTime.UtcNow.EndOfMonth();
Console.WriteLine($"Month: {startOfMonth:yyyy-MM} ({startOfMonth:yyyy-MM-dd} to {endOfMonth:yyyy-MM-dd})");
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

## SpreadAnalysisServiceTests

The `SpreadAnalysisServiceTests` class contains unit tests for the `SpreadAnalysisService` class, verifying spread analysis functionality including spread calculations, cross-currency spread analysis, and spread update operations. These tests ensure that the spread analysis service correctly handles various scenarios such as valid prices, zero prices, missing data, and invalid spreads.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using FluentAssertions;
using Xunit;

// Create test instance with mocks
var priceRepositoryMock = new Mock<IPriceRepository>();
var historyServiceMock = new Mock<IPriceHistoryService>();
var settings = new AppSettings { DefaultSpreadThreshold = 1.0m, SpreadAnalysisHistoryHours = 24 };
var loggerMock = new Mock<ILogger<SpreadAnalysisService>>();
var service = new SpreadAnalysisService(priceRepositoryMock.Object, historyServiceMock.Object, settings, loggerMock.Object);

// Test 1: AnalyzeSpreadAsync with valid prices
var spreadPercent = await service.AnalyzeSpreadAsync(100m, 102m);
spreadPercent.Should().Be(2.0m); // (102-100)/100 * 100 = 2%

// Test 2: AnalyzeSpreadAsync with zero buy price (should throw exception)
Func<Task> act = () => service.AnalyzeSpreadAsync(0m, 102m).AsTask();
await act.Should().ThrowAsync<InvalidPriceException>();

// Test 3: UpdateSpreadAsync with valid spread
var spread = new Spread
{
    Asset = "BTC",
    Fiat = "USD",
    CurrentSpreadPercent = 0.5m,
    AverageSpreadPercent = 0.5m,
    MinSpreadPercent = 0.4m,
    MaxSpreadPercent = 0.6m,
    SampleCount = 1,
    LastUpdatedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow
};
var updateResult = await service.UpdateSpreadAsync(spread);
updateResult.Should().BeTrue();

// Test 4: UpdateSpreadAsync with invalid spread (should throw exception)
var invalidSpread = new Spread { Asset = "", Fiat = "USD" };
Func<Task> act2 = () => service.UpdateSpreadAsync(invalidSpread).AsTask();
await act2.Should().ThrowAsync<InvalidPriceException>();

// Test 5: GetCrossCurrencySpreadAsync with valid data
priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync("BTC", "USD"))
    .ReturnsAsync(new Price { Asset = "BTC", Fiat = "USD", BuyPrice = 100m });
priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync("BTC", "EUR"))
    .ReturnsAsync(new Price { Asset = "BTC", Fiat = "EUR", SellPrice = 120m });

var crossSpread = await service.GetCrossCurrencySpreadAsync("BTC", "USD", "EUR", 0.9m);
crossSpread.Should().NotBeNull();
crossSpread!.SpreadPercent.Should().Be(8.0m); // (120*0.9 - 100)/100 * 100 = 8%

// Test 6: GetCrossCurrencySpreadAsync with missing data (should return null)
priceRepositoryMock.Setup(r => r.GetLatestByAssetAndFiatAsync("BTC", "USD"))
    .ReturnsAsync((Price?)null);
var nullSpread = await service.GetCrossCurrencySpreadAsync("BTC", "USD", "EUR", 0.9m);
nullSpread.Should().BeNull();
```

## AlertServiceTests

The `AlertServiceTests` class contains unit tests for the `AlertService` class, verifying alert management functionality including alert creation, updates, deletion, and retrieval. These tests ensure that the alert service correctly handles valid and invalid alert configurations, respects maximum alert limits per user, and properly integrates with the alert repository for data operations.

```csharp
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

// Create test dependencies
var mockAlertRepository = Substitute.For<IAlertRepository>();
var appSettings = new AppSettings { MaxAlertsPerUser = 5 };
var mockLogger = Substitute.For<ILogger<AlertService>>();
var mockTelegram = Substitute.For<ITelegramNotificationClient>();
var mockWebhook = Substitute.For<IWebhookNotificationClient>();

var alertService = new AlertService(mockAlertRepository, appSettings, mockLogger, mockTelegram, mockWebhook);

// Test 1: CreateAlertAsync with valid alert and max alerts not reached
var validAlert = new PriceAlert
{
    UserId = 1,
    Asset = "USDT",
    Fiat = "UAH",
    AlertType = AlertType.PriceChange,
    Condition = AlertCondition.GreaterThan,
    Threshold = 1.0m,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

mockAlertRepository.GetUserAlertCountAsync(1).Returns(2);
mockAlertRepository.AddAsync(Arg.Any<PriceAlert>()).Returns(1);

var alertId = await alertService.CreateAlertAsync(validAlert);
alertId.Should().Be(1);

// Test 2: CreateAlertAsync with invalid alert (should throw exception)
var invalidAlert = new PriceAlert { UserId = 1 };

Func<Task> invalidAction = async () => await alertService.CreateAlertAsync(invalidAlert);
await invalidAction.Should().ThrowAsync<InvalidAlertException>();

// Test 3: CreateAlertAsync when max alerts reached (should throw exception)
var maxAlertsAlert = new PriceAlert
{
    UserId = 2,
    Asset = "BTC",
    Fiat = "USDT",
    AlertType = AlertType.PriceAbove,
    Condition = AlertCondition.GreaterThan,
    Threshold = 50000.00m,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

mockAlertRepository.GetUserAlertCountAsync(2).Returns(5); // MaxAlertsPerUser = 5

Func<Task> maxAlertsAction = async () => await alertService.CreateAlertAsync(maxAlertsAlert);
await maxAlertsAction.Should().ThrowAsync<InvalidAlertException>()
    .WithMessage("Maximum number of alerts (5) reached");

// Test 4: UpdateAlertAsync with valid alert that exists
var existingAlert = new PriceAlert
{
    Id = 1,
    UserId = 1,
    Asset = "USDT",
    Fiat = "UAH",
    AlertType = AlertType.PriceChange,
    Condition = AlertCondition.GreaterThan,
    Threshold = 2.0m,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

mockAlertRepository.UpdateAsync(Arg.Any<PriceAlert>()).Returns(true);

var updateResult = await alertService.UpdateAlertAsync(existingAlert);
updateResult.Should().BeTrue();

// Test 5: UpdateAlertAsync with alert that doesn't exist
var nonExistentAlert = new PriceAlert
{
    Id = 99,
    UserId = 1,
    Asset = "BTC",
    Fiat = "USDT",
    AlertType = AlertType.PriceAbove,
    Threshold = 50000.00m,
    IsEnabled = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

mockAlertRepository.UpdateAsync(Arg.Any<PriceAlert>()).Returns(false);

var falseResult = await alertService.UpdateAlertAsync(nonExistentAlert);
falseResult.Should().BeFalse();

// Test 6: DeleteAlertAsync with existing alert
mockAlertRepository.DeleteAsync(1).Returns(true);

var deleteResult = await alertService.DeleteAlertAsync(1);
deleteResult.Should().BeTrue();

// Test 7: GetUserAlertsAsync when user has alerts
var userAlerts = new List<PriceAlert>
{
    new PriceAlert { Id = 1, UserId = 1, Asset = "USDT", Fiat = "UAH", AlertType = AlertType.PriceChange },
    new PriceAlert { Id = 2, UserId = 1, Asset = "BTC", Fiat = "USDT", AlertType = AlertType.PriceAbove }
};

mockAlertRepository.GetUserAlertsAsync(1).Returns(userAlerts);

var retrievedAlerts = await alertService.GetUserAlertsAsync(1);
retrievedAlerts.Should().HaveCount(2);
```

## AlertRepository

The `AlertRepository` class provides data access methods for storing, retrieving, updating, and deleting price alert records in the Binance P2P Monitor application. It serves as the primary interface for interacting with price alert data in the database, offering methods to fetch alerts by ID, user ID, asset/fiat combinations, and active status. The repository supports comprehensive alert management including enabling/disabling alerts, tracking trigger history, and managing user-specific alert configurations.

```csharp
using BinanceP2pMonitor.Repositories;

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

## PriceAlertTests

The `PriceAlertTests` class contains unit tests for the `PriceAlert` class, verifying price alert functionality including alert triggering logic, state management, and validation. These tests ensure that alerts correctly evaluate conditions, manage trigger counts, handle cooldown periods, and validate required fields.

```csharp
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Constants;

// Create a price alert for monitoring when BTC price change exceeds 5%
var alert = new PriceAlert
{
    Asset = "BTC",
    Fiat = "USD",
    AlertType = AlertType.PriceChange,
    Condition = AlertCondition.GreaterThan,
    Threshold = 5.0m,
    IsEnabled = true,
    UserId = 123,
    Notes = "Notify when BTC price change exceeds 5%",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

// Test if the alert should trigger with a current price change of 6%
bool shouldTrigger = alert.ShouldTrigger(currentChange: 6.0m);
Console.WriteLine($"Alert should trigger: {shouldTrigger}"); // True

// Record the alert trigger (increments trigger count and sets timestamp)
if (shouldTrigger)
{
    alert.RecordTrigger();
    Console.WriteLine($"Trigger count: {alert.TriggerCount}"); // 1
    Console.WriteLine($"Last triggered: {alert.LastTriggeredAt}");
}

// Toggle the alert status (enable/disable)
alert.Toggle();
Console.WriteLine($"Alert enabled: {alert.IsEnabled}"); // False

// Get alert description for display
Console.WriteLine($"Alert description: {alert.GetDescription()}");
// "Alert on BTC/USD: Price change > 5.00%"

// Check if alert is valid
Console.WriteLine($"Alert is valid: {alert.IsValid()}"); // True
```

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Models;
using FluentAssertions;
using Xunit;

// Create test database setup
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();
var context = new DatabaseContext(connection);
var alertRepository = new AlertRepository(context);

// Initialize database schema
context.ExecuteCommand(@"CREATE TABLE PriceAlerts (...)");

// Test 1: AddAsync should add alert and return ID
var newAlert = new PriceAlert
{
UserId = 1,
Asset = "USDT",
Fiat = "UAH",
AlertType = AlertType.PriceChange,
Threshold = 1.0m,
Condition = AlertCondition.GreaterThan,
IsEnabled = true,
CreatedAt = DateTime.UtcNow,
UpdatedAt = DateTime.UtcNow,
TriggerCount = 0,
Notes = "Test Alert"
};

var alertId = await alertRepository.AddAsync(newAlert);
alertId.Should().BeGreaterThan(0);

// Test 2: GetByIdAsync should return alert when exists
var retrievedAlert = await alertRepository.GetByIdAsync(alertId);
retrievedAlert.Should().NotBeNull();
retrievedAlert!.Asset.Should().Be("USDT");

// Test 3: GetByIdAsync should return null when alert doesn't exist
var nullAlert = await alertRepository.GetByIdAsync(999);
nullAlert.Should().BeNull();

// Test 4: UpdateAsync should update alert and return true
retrievedAlert.Threshold = 2.0m;
retrievedAlert.Notes = "Updated Test Alert";
var updateResult = await alertRepository.UpdateAsync(retrievedAlert);
updateResult.Should().BeTrue();

// Test 5: UpdateAsync should return false when alert doesn't exist
var fakeAlert = new PriceAlert { Id = 999 };
var falseResult = await alertRepository.UpdateAsync(fakeAlert);
falseResult.Should().BeFalse();

// Test 6: DeleteAsync should delete alert and return true
var deleteResult = await alertRepository.DeleteAsync(alertId);
deleteResult.Should().BeTrue();

// Test 7: DeleteAsync should return false when alert doesn't exist
var falseDelete = await alertRepository.DeleteAsync(999);
falseDelete.Should().BeFalse();

// Test 8: GetUserAlertsAsync should return alerts for user
var userId = 1;
await alertRepository.AddAsync(new PriceAlert { UserId = userId });
await alertRepository.AddAsync(new PriceAlert { UserId = userId });
await alertRepository.AddAsync(new PriceAlert { UserId = 2 }); // Different user

var userAlerts = await alertRepository.GetUserAlertsAsync(userId);
userAlerts.Should().HaveCount(2);
userAlerts.Should().AllSatisfy(a => a.UserId.Should().Be(userId));

// Cleanup
disposable.Dispose();
```

## AlertRepository
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

## PriceHistoryServiceTests

The `PriceHistoryServiceTests` class contains unit tests for the `PriceHistoryService` class, verifying historical price data analysis, statistics calculation, and cleanup operations. These tests ensure that the price history service correctly handles price trend calculations, price statistics (high/low/average), empty history scenarios, and proper delegation to repository methods. The test suite uses mock repositories to isolate service functionality from database concerns.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;
using Moq;

// Create test dependencies
var repoMock = new Mock<IHistoryRepository>();
var settings = new AppSettings { DatabaseConnectionString = "Data Source=:memory:" };
var logger = Mock.Of<ILogger<PriceHistoryService>>();

// Create the service with mocked dependencies
var priceHistoryService = new PriceHistoryService(repoMock.Object, settings, logger);

// Test 1: GetPriceTrendAsync with rising prices
var earlier = new PriceHistory
{
    Asset = "BTC",
    Fiat = "USD",
    BuyPrice = 40000m,
    SellPrice = 40100m,
    RecordedAt = DateTime.UtcNow.AddHours(-2),
    CreatedAt = DateTime.UtcNow
};
var later = new PriceHistory
{
    Asset = "BTC",
    Fiat = "USD",
    BuyPrice = 44000m,
    SellPrice = 44100m,
    RecordedAt = DateTime.UtcNow.AddHours(-1),
    CreatedAt = DateTime.UtcNow
};

repoMock.Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
    .ReturnsAsync(new[] { earlier, later });

var trend = await priceHistoryService.GetPriceTrendAsync("BTC", "USD", 24);
Console.WriteLine($"Price trend: {trend:F4}%"); // Positive trend

// Test 2: GetPriceStatsAsync with multiple records
var records = new[]
{
    new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 40000m, SellPrice = 41000m, RecordedAt = DateTime.UtcNow.AddHours(-3), CreatedAt = DateTime.UtcNow },
    new PriceHistory { Asset = "BTC", Fiat = "USD", BuyPrice = 50000m, SellPrice = 51000m, RecordedAt = DateTime.UtcNow.AddHours(-1), CreatedAt = DateTime.UtcNow }
};

repoMock.Setup(r => r.GetHistoryByAssetAndFiatAsync("BTC", "USD", 24))
    .ReturnsAsync(records);

var (high, low, average) = await priceHistoryService.GetPriceStatsAsync("BTC", "USD", 24);
Console.WriteLine($"High: {high:C}, Low: {low:C}, Average: {average:C}"); // High: $51,000.00, Low: $40,000.00, Average: $45,500.00

// Test 3: GetHistoryCountAsync - delegates to repository
repoMock.Setup(r => r.GetTotalHistoryCountAsync())
    .ReturnsAsync(100L);

var totalCount = await priceHistoryService.GetHistoryCountAsync();
Console.WriteLine($"Total history records: {totalCount}"); // 100

// Test 4: CleanupOldHistoryAsync - delegates to repository
repoMock.Setup(r => r.DeleteOldRecordsAsync(30))
    .ReturnsAsync(true);

var cleanupResult = await priceHistoryService.CleanupOldHistoryAsync(daysOld: 30);
Console.WriteLine($"Cleanup successful: {cleanupResult}"); // true

// Test 5: Constructor validation
try
{
    var invalidService = new PriceHistoryService(null!, settings, logger);
}
catch (ArgumentNullException ex)
{
    Console.WriteLine($"Constructor validation: {ex.ParamName}"); // historyRepository
}
```

## PriceMonitoringServiceTests

The `PriceMonitoringServiceTests` class contains unit tests for the `PriceMonitoringService` class, verifying price monitoring functionality including retrieving current prices, updating prices, calculating averages, detecting significant changes, and handling invalid price scenarios. These tests ensure that the price monitoring service correctly integrates with repositories, history services, alert services, spread analysis services, and WebSocket services while maintaining proper error handling and validation.

```csharp
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

// Setup dependency injection with mocked services
var mockPriceRepository = Substitute.For<IPriceRepository>();
var mockPriceHistoryService = Substitute.For<IPriceHistoryService>();
var mockAlertService = Substitute.For<IAlertService>();
var mockSpreadAnalysisService = Substitute.For<ISpreadAnalysisService>();
var mockEventBus = Substitute.For<IEventBus>();
var mockWebSocketService = Substitute.For<IWebSocketService>();
var appSettings = new AppSettings { DatabaseConnectionString = "DataSource=:memory:", EnableWebSocket = false };
var mockLogger = Substitute.For<ILogger<PriceMonitoringService>>();

var priceMonitoringService = new PriceMonitoringService(
    mockPriceRepository,
    mockPriceHistoryService,
    mockAlertService,
    mockSpreadAnalysisService,
    mockEventBus,
    mockWebSocketService,
    appSettings,
    mockLogger
);

// Test 1: GetCurrentPriceAsync with existing price
var existingPrice = new Price { Asset = "BTC", Fiat = "USDT", BuyPrice = 50000.50m, SellPrice = 50010.25m };
mockPriceRepository.GetLatestByAssetAndFiatAsync("BTC", "USDT").Returns(existingPrice);

var currentPrice = await priceMonitoringService.GetCurrentPriceAsync("BTC", "USDT");
currentPrice.Should().NotBeNull();
currentPrice!.BuyPrice.Should().Be(50000.50m);

// Test 2: GetCurrentPriceAsync with non-existent price
mockPriceRepository.GetLatestByAssetAndFiatAsync("ETH", "USDT").Returns((Price?)null);
var nullPrice = await priceMonitoringService.GetCurrentPriceAsync("ETH", "USDT");
nullPrice.Should().BeNull();

// Test 3: UpdatePriceAsync with valid price
var newPrice = new Price
{
    Asset = "BTC",
    Fiat = "USDT",
    BuyPrice = 50100.75m,
    SellPrice = 50110.50m,
    Timestamp = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
mockPriceRepository.AddAsync(Arg.Any<Price>()).Returns(1);
mockAlertService.CheckTriggersAsync(Arg.Any<Price>()).Returns(new List<PriceAlert>());

var updateResult = await priceMonitoringService.UpdatePriceAsync(newPrice);
updateResult.Should().BeTrue();
await mockPriceRepository.Received(1).AddAsync(Arg.Any<Price>());
await mockPriceHistoryService.Received(1).RecordPriceAsync(Arg.Any<Price>());
await mockAlertService.Received(1).CheckTriggersAsync(Arg.Any<Price>());

// Test 4: UpdatePriceAsync with invalid price (should throw exception)
var invalidPrice = new Price { Asset = "BTC", BuyPrice = -1.0m };
Func<Task> invalidAction = async () => await priceMonitoringService.UpdatePriceAsync(invalidPrice);
await invalidAction.Should().ThrowAsync<ArgumentException>();

// Test 5: GetAveragePriceAsync
mockPriceRepository.GetAveragePriceAsync("BTC", "USDT", 24).Returns(50050.25m);
var averagePrice = await priceMonitoringService.GetAveragePriceAsync("BTC", "USDT", 24);
averagePrice.Should().Be(50050.25m);

// Test 6: GetPricesWithSignificantChangeAsync
var prices = new List<Price>
{
    new() { Asset = "BTC", Fiat = "USDT", BuyPrice = 50000m, BuyChangePercent = 5.0m },
    new() { Asset = "ETH", Fiat = "USDT", BuyPrice = 3500m, BuyChangePercent = 1.0m }
};
mockPriceRepository.GetAllActiveAsync().Returns(prices);

var significantChanges = await priceMonitoringService.GetPricesWithSignificantChangeAsync(3.0m);
significantChanges.Should().ContainSingle(p => p.Asset == "BTC");
```

## PriceRepositoryTests

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

// Create in-memory database for testing
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

var context = new DatabaseContext(connection);
var priceRepository = new PriceRepository(context);

// Initialize database schema
context.ExecuteCommand(@"
CREATE TABLE Prices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Asset TEXT NOT NULL,
    Fiat TEXT NOT NULL,
    BuyPrice REAL NOT NULL,
    SellPrice REAL NOT NULL,
    BuyChangePercent REAL NOT NULL,
    SellChangePercent REAL NOT NULL,
    Timestamp TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    Metadata TEXT
);");

// Test 1: AddAsync should add price and return ID
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
priceId.Should().BeGreaterThan(0);

// Test 2: GetByIdAsync should return price when exists
var retrievedPrice = await priceRepository.GetByIdAsync(priceId);
retrievedPrice.Should().NotBeNull();
retrievedPrice!.Asset.Should().Be("BTC");

// Test 3: GetByIdAsync should return null when price doesn't exist
var nullPrice = await priceRepository.GetByIdAsync(999);
nullPrice.Should().BeNull();

// Test 4: GetLatestByAssetAndFiatAsync should return latest price
var latestPrice = await priceRepository.GetLatestByAssetAndFiatAsync("BTC", "USDT");
latestPrice.Should().NotBeNull();

// Test 5: UpdateAsync should update price and return true
if (retrievedPrice != null)
{
    retrievedPrice.BuyPrice = 50100.75m;
    retrievedPrice.SellPrice = 50110.50m;
    retrievedPrice.UpdatedAt = DateTime.UtcNow;
    
    var updateResult = await priceRepository.UpdateAsync(retrievedPrice);
    updateResult.Should().BeTrue();
}

// Test 6: DeleteAsync should delete price and return true
var deleteResult = await priceRepository.DeleteAsync(priceId);
deleteResult.Should().BeTrue();

// Test 7: GetAveragePriceAsync should return average price
var avgPrice = await priceRepository.GetAveragePriceAsync("BTC", "USDT", 24);
avgPrice.Should().BeGreaterThan(0);

// Cleanup
connection.Close();
connection.Dispose();
```

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

## PriceCalculator

The `PriceCalculator` utility class provides static methods for performing common price calculations, spread analysis, and price formatting operations. It includes methods for calculating percentage changes, spreads, mid-prices, moving averages, standard deviations, and various threshold checks. The class also provides utilities for rounding prices and formatting them for display.

```csharp
using BinanceP2pMonitor.Utilities;

// Calculate percentage change between two prices
var changePercent = PriceCalculator.CalculatePercentageChange(51000.75m, 50000.00m);
Console.WriteLine($"Price change: {changePercent:F2}%"); // 2.00%

// Calculate spread between buy and sell prices
var spreadPercent = PriceCalculator.CalculateSpread(50000.50m, 50010.25m);
Console.WriteLine($"Spread: {spreadPercent:F4}%"); // 0.0205%

// Calculate mid-price (average of buy and sell)
var midPrice = PriceCalculator.CalculateMidPrice(50000.50m, 50010.25m);
Console.WriteLine($"Mid price: {midPrice:C}"); // $50,005.38

// Check if price is above threshold
var isAbove = PriceCalculator.IsAboveThreshold(51000.00m, 50000.00m, 2.0m);
Console.WriteLine($"Above 2% threshold: {isAbove}"); // True

// Check if price is below threshold
var isBelow = PriceCalculator.IsBelowThreshold(49000.00m, 50000.00m, 2.0m);
Console.WriteLine($"Below 2% threshold: {isBelow}"); // True

// Round a price to 2 decimal places
var roundedPrice = PriceCalculator.RoundPrice(50000.5678m, 2);
Console.WriteLine($"Rounded price: {roundedPrice:C}"); // $50,000.57

// Format a price with currency symbol
var formattedPrice = PriceCalculator.FormatPrice(50000.50m, "$");
Console.WriteLine($"Formatted price: {formattedPrice}"); // $50,000.50

// Calculate moving average over a collection of prices
var prices = new decimal[] { 50000.00m, 50100.50m, 50200.75m, 50300.25m, 50400.00m };
var movingAvg = PriceCalculator.CalculateMovingAverage(prices, windowSize: 3);
Console.WriteLine($"3-period moving average: {movingAvg:C}"); // $50,200.50

// Calculate standard deviation of prices
var stdDev = PriceCalculator.CalculateStandardDeviation(prices);
Console.WriteLine($"Standard deviation: {stdDev:F4}"); // 163.3012
```

## PriceCalculatorTests

The `PriceCalculatorTests` class contains unit tests for the `PriceCalculator` utility class, verifying price calculation functionality including percentage changes, spreads, mid-prices, moving averages, standard deviations, and price formatting operations. These tests ensure that all price calculation methods handle edge cases correctly such as zero prices, identical prices, and various threshold scenarios.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

// Test percentage change calculations
[Fact]
public void CalculatePercentageChange_PriceIncreases_ReturnsPositivePercentage()
{
    var result = PriceCalculator.CalculatePercentageChange(51000.75m, 50000.00m);
    result.Should().Be(2.00m); // (51000.75 - 50000) / 50000 * 100 = 2.00%
}

[Fact]
public void CalculatePercentageChange_PriceDecreases_ReturnsNegativePercentage()
{
    var result = PriceCalculator.CalculatePercentageChange(49000.00m, 50000.00m);
    result.Should().Be(-2.00m); // (49000 - 50000) / 50000 * 100 = -2.00%
}

[Fact]
public void CalculatePercentageChange_ZeroOriginalPrice_ReturnsZero()
{
    var result = PriceCalculator.CalculatePercentageChange(0m, 50000.00m);
    result.Should().Be(0m);
}

// Test spread calculations
[Fact]
public void CalculateSpread_BuyAndSellPrices_ReturnsCorrectSpreadPercent()
{
    var result = PriceCalculator.CalculateSpread(50000.50m, 50010.25m);
    result.Should().BeApproximately(0.0195m, 0.0001m); // (50010.25 - 50000.50) / 50000.50 * 100
}

[Fact]
public void CalculateSpread_ZeroBuyPrice_ReturnsZero()
{
    var result = PriceCalculator.CalculateSpread(0m, 50010.25m);
    result.Should().Be(0m);
}

// Test mid-price calculations
[Fact]
public void CalculateMidPrice_TwoPrices_ReturnsArithmeticMean()
{
    var result = PriceCalculator.CalculateMidPrice(50000.50m, 50010.25m);
    result.Should().Be(50005.375m); // (50000.50 + 50010.25) / 2
}

// Test moving average calculations
[Fact]
public void CalculateMovingAverage_FewerPricesThanPeriod_ReturnsOverallAverage()
{
    var prices = new decimal[] { 50000.00m, 50100.50m, 50200.75m };
    var result = PriceCalculator.CalculateMovingAverage(prices, windowSize: 5);
    result.Should().BeApproximately(50100.42m, 0.01m); // Average of all 3 prices
}

[Fact]
public void CalculateMovingAverage_ExactPeriod_ReturnsLastNAverage()
{
    var prices = new decimal[] { 50000.00m, 50100.50m, 50200.75m, 50300.25m, 50400.00m };
    var result = PriceCalculator.CalculateMovingAverage(prices, windowSize: 3);
    result.Should().Be(50300.50m); // Average of last 3 prices: (50200.75 + 50300.25 + 50400.00) / 3
}

// Test standard deviation calculations
[Fact]
public void CalculateStandardDeviation_SinglePrice_ReturnsZero()
{
    var prices = new decimal[] { 50000.00m };
    var result = PriceCalculator.CalculateStandardDeviation(prices);
    result.Should().Be(0m);
}

[Fact]
public void CalculateStandardDeviation_IdenticalPrices_ReturnsZero()
{
    var prices = new decimal[] { 50000.00m, 50000.00m, 50000.00m };
    var result = PriceCalculator.CalculateStandardDeviation(prices);
    result.Should().Be(0m);
}

// Test price formatting
[Fact]
public void FormatPrice_WithCurrencySymbol_PrependsCurrencySymbol()
{
    var result = PriceCalculator.FormatPrice(50000.50m, "$");
    result.Should().Be("$50,000.50");
}

[Fact]
public void FormatPrice_NoCurrencySymbol_ReturnsPlainDecimal()
{
    var result = PriceCalculator.FormatPrice(50000.50m, null);
    result.Should().Be("50000.50");
}
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


## ValidationHelper

The `ValidationHelper` utility class provides static methods for validating various types of data used throughout the Binance P2P Monitor application. It includes methods for validating email addresses, cryptocurrency tickers, fiat currency codes, prices, thresholds, Telegram chat IDs, date ranges, collections, decimal precision, and string patterns. These validation methods are used extensively for input validation, API response validation, and data integrity checks.

```csharp
using BinanceP2pMonitor.Utilities;

// Validate an email address
bool isValidEmail = ValidationHelper.IsValidEmail("user@example.com");
Console.WriteLine($"Valid email: {isValidEmail}");

// Validate a cryptocurrency ticker (e.g., BTC, ETH, USDT)
bool isValidTicker = ValidationHelper.IsValidTicker("BTC");
Console.WriteLine($"Valid ticker: {isValidTicker}");

// Validate a fiat currency code (3-letter ISO code)
bool isValidFiat = ValidationHelper.IsValidFiatCode("USDT");
Console.WriteLine($"Valid fiat code: {isValidFiat}");

// Validate a price (must be between 0.00000001 and 1000000000 by default)
bool isValidPrice = ValidationHelper.IsValidPrice(50000.50m);
Console.WriteLine($"Valid price: {isValidPrice}");

// Validate a threshold percentage (must be between 0 and 100 by default)
bool isValidThreshold = ValidationHelper.IsValidThreshold(2.5m);
Console.WriteLine($"Valid threshold: {isValidThreshold}");

// Validate a Telegram chat ID (must be positive)
bool isValidChatId = ValidationHelper.IsValidTelegramChatId(-1001234567890);
Console.WriteLine($"Valid Telegram chat ID: {isValidChatId}");

// Validate a date range (start must be before end and in the past)
bool isValidDateRange = ValidationHelper.IsValidDateRange(
    DateTime.UtcNow.AddDays(-7), 
    DateTime.UtcNow.AddDays(-1)
);
Console.WriteLine($"Valid date range: {isValidDateRange}");

// Validate a collection (must not be null and must contain items)
bool isValidCollection = ValidationHelper.IsValidCollection(new List<string> { "item1", "item2" });
Console.WriteLine($"Valid collection: {isValidCollection}");

// Validate decimal precision (must have 2 or fewer decimal places)
bool isValidPrecision = ValidationHelper.IsValidPrecision(50000.50m, maxDecimalPlaces: 2);
Console.WriteLine($"Valid precision: {isValidPrecision}");

// Validate if a string matches a specific pattern
bool matchesPattern = ValidationHelper.MatchesPattern("BTCUSDT", "^[A-Z0-9]{6,12}$");
Console.WriteLine($"Matches pattern: {matchesPattern}");
```

## FormatHelper

The `FormatHelper` utility class provides static methods for formatting various types of data for display and output in the Binance P2P Monitor application. It includes methods for formatting currencies, percentages, timestamps, time intervals, large numbers, trading pairs, alert descriptions, price changes, and market information. The class also provides text wrapping functionality for formatting long strings across multiple lines.

```csharp
using BinanceP2pMonitor.Utilities;

// Format a currency value with commas and 2 decimal places
var formattedCurrency = FormatHelper.FormatCurrency(50000.5m);
Console.WriteLine(formattedCurrency); // "50,000.50"

// Format a currency value with custom decimal places
var formattedCurrency2 = FormatHelper.FormatCurrency(50000.5678m, 4);
Console.WriteLine(formattedCurrency2); // "50,000.5678"

// Format a percentage with 2 decimal places
var formattedPercentage = FormatHelper.FormatPercentage(2.3456m);
Console.WriteLine(formattedPercentage); // "2.35%"

// Format a timestamp in a specific format
dateTime = DateTime.UtcNow;
var formattedTimestamp = FormatHelper.FormatTimestamp(dateTime, "yyyy-MM-dd HH:mm:ss");
Console.WriteLine(formattedTimestamp); // "2024-01-15 14:30:45"

// Format time elapsed since a given date
var priceUpdateTime = DateTime.UtcNow.AddMinutes(-45);
var timeAgo = FormatHelper.FormatTimeAgo(priceUpdateTime);
Console.WriteLine(timeAgo); // "45m ago"

// Format a large number with abbreviation (K, M, B)
var formattedLargeNumber = FormatHelper.FormatLargeNumber(1500000);
Console.WriteLine(formattedLargeNumber); // "1.5M"

var formattedLargeNumber2 = FormatHelper.FormatLargeNumber(2500000000);
Console.WriteLine(formattedLargeNumber2); // "2.5B"

// Format a trading pair identifier
var formattedTradingPair = FormatHelper.FormatTradingPair("BTC", "USDT");
Console.WriteLine(formattedTradingPair); // "BTC/USDT"

// Format an alert description
var alertDescription = FormatHelper.FormatAlertDescription("BTC/USDT", "Price above threshold", 51000.00m);
Console.WriteLine(alertDescription); // "Alert on BTC/USDT: Price above threshold 51000.00%"

// Format a price change indicator (with arrow and percentage)
var formattedPriceChange = FormatHelper.FormatPriceChange(2.5m);
Console.WriteLine(formattedPriceChange); // "↑ 2.50%"

var formattedPriceChange2 = FormatHelper.FormatPriceChange(-1.25m);
Console.WriteLine(formattedPriceChange2); // "↓ 1.25%"

// Format a price change with ANSI color codes for terminal output
var formattedPriceChangeWithColors = FormatHelper.FormatPriceChange(3.75m, includeColorCodes: true);
Console.WriteLine(formattedPriceChangeWithColors); // "[32m↑ 3.75%[0m"

// Format market information for a trading pair
var formattedMarketInfo = FormatHelper.FormatMarketInfo("BTC", "USDT", 50000.50m, 2.5m);
Console.WriteLine(formattedMarketInfo); // "BTC/USDT: 50,000.50 ↑ 2.50%"

// Wrap a long text string into multiple lines with max width
var longText = "This is a very long error message that needs to be wrapped across multiple lines for proper display in the console interface";
var wrappedLines = FormatHelper.WrapText(longText, 40);
Console.WriteLine("Wrapped text:");
foreach (var line in wrappedLines)
{
    Console.WriteLine(line);
}
```

## HistoryRepositoryTests

The `HistoryRepositoryTests` class contains unit tests for the `HistoryRepository` class, verifying historical price data storage, retrieval, and management functionality. These tests ensure that history records are correctly added, retrieved by ID, queried by asset/fiat combinations within time windows, and that cleanup operations work as expected. The test suite also validates methods for counting total records and finding highest prices within specific time periods.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using Microsoft.Data.Sqlite;
using Xunit;

// Create in-memory database for testing
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();

var context = new DatabaseContext(connection);
var historyRepository = new HistoryRepository(context);

// Initialize database schema
context.ExecuteCommand(@"
CREATE TABLE PriceHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PriceId INTEGER NOT NULL,
    Asset TEXT NOT NULL,
    Fiat TEXT NOT NULL,
    BuyPrice REAL NOT NULL,
    SellPrice REAL NOT NULL,
    RecordedAt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    SpreadPercentage REAL NOT NULL,
    PriceChangePercent REAL NOT NULL,
    Notes TEXT
);");

// Test 1: AddAsync should add history record and return ID
var newHistory = new PriceHistory
{
    PriceId = 1,
    Asset = "USDT",
    Fiat = "UAH",
    BuyPrice = 38.0m,
    SellPrice = 38.5m,
    RecordedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    SpreadPercentage = 1.0m,
    PriceChangePercent = 0.5m,
    Notes = "Test History"
};

var historyId = await historyRepository.AddAsync(newHistory);
historyId.Should().BeGreaterThan(0);

// Test 2: GetByIdAsync should return history when exists
var retrievedHistory = await historyRepository.GetByIdAsync(historyId);
retrievedHistory.Should().NotBeNull();
retrievedHistory!.Asset.Should().Be("USDT");

// Test 3: GetByIdAsync should return null when history doesn't exist
var nullHistory = await historyRepository.GetByIdAsync(999);
nullHistory.Should().BeNull();

// Test 4: GetHistoryByAssetAndFiatAsync should filter by asset/fiat and time window
var btcHistory = await historyRepository.GetHistoryByAssetAndFiatAsync("USDT", "UAH", hours: 24);
btcHistory.Should().HaveCount(1);

// Test 5: DeleteOldRecordsAsync should remove records older than specified days
var cleanupResult = await historyRepository.DeleteOldRecordsAsync(daysOld: 30);
cleanupResult.Should().BeTrue();

// Test 6: GetTotalHistoryCountAsync should return correct count
var totalCount = await historyRepository.GetTotalHistoryCountAsync();
totalCount.Should().BeGreaterThanOrEqualTo(0);

// Test 7: GetHighestPriceAsync should find highest price within time window
var highestPrice = await historyRepository.GetHighestPriceAsync("USDT", "UAH", hours: 24);
highestPrice.Should().BeGreaterThan(0);

// Cleanup
connection.Close();
connection.Dispose();
```

## ValidationException

The `ValidationException` class is a custom exception type used throughout the Binance P2P Monitor application to handle validation failures. It extends the standard `Exception` class and provides additional functionality to track multiple validation errors through a `List<string> Errors` property. This exception is particularly useful for scenarios where multiple validation checks fail simultaneously, allowing all errors to be collected and reported together rather than throwing multiple exceptions.

```csharp
using BinanceP2pMonitor.Utilities;

// Create a validation exception with a single error message
var exception1 = new ValidationException("Price cannot be negative");
Console.WriteLine(exception1.Message); // "Price cannot be negative"
Console.WriteLine(exception1.Errors.Count); // 1

// Create a validation exception with multiple error messages
var errors = new List<string> {
    "Asset cannot be null or empty",
    "Fiat currency must be specified",
    "Buy price must be greater than 0"
};
var exception2 = new ValidationException(errors);
Console.WriteLine(exception2.Message); // "Asset cannot be null or empty; Fiat currency must be specified; Buy price must be greater than 0"
Console.WriteLine(exception2.Errors.Count); // 3

// Create a validation exception with both a base message and multiple errors
var exception3 = new ValidationException("Price validation failed", errors);
Console.WriteLine(exception3.Message); // "Price validation failed"
Console.WriteLine(exception3.Errors.Count); // 3

// Add additional errors to an existing exception
var exception4 = new ValidationException("Spread calculation error");
exception4.AddError("Spread percentage exceeds maximum threshold of 5%");
exception4.AddError("Current spread: 6.25%");
Console.WriteLine(exception4.Errors.Count); // 2

// Check if a validation exception contains specific errors
if (exception2.Errors.Any(e => e.Contains("Asset")))
{
    Console.WriteLine("Validation exception contains asset-related error");
}
```
## ConfigurationValidationTests

The `ConfigurationValidationTests` class provides unit tests for validating application configuration settings in the Binance P2P Monitor. It verifies that configuration values like database connection strings, monitoring intervals, alert thresholds, and other settings meet expected constraints and business rules. These tests ensure that invalid configurations are caught early and prevent runtime errors.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Configuration;
using FluentAssertions;
using Xunit;

// Create test instance
var configuration = new ConfigurationValidationTests();

// Test 1: Validate_ShouldNotThrowException_WhenSettingsAreValid
// Create valid configuration
var validSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30,
    EnableWebSocket = true,
    EnableTelegramNotifications = false
};

// Should not throw any exception
var act1 = () => configuration.Validate(validSettings);
act1.Should().NotThrow();

// Test 2: Validate_ShouldThrowException_WhenDatabaseConnectionStringIsInvalid
// Create invalid configuration with empty database connection string
var invalidDbSettings = new AppSettings
{
    DatabaseConnectionString = "",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30
};

// Should throw exception
var act2 = () => configuration.Validate(invalidDbSettings);
act2.Should().Throw<ValidationException>();

// Test 3: Validate_ShouldThrowException_WhenMonitoringIntervalSecondsIsInvalid
// Create invalid configuration with negative monitoring interval
var invalidIntervalSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = -1,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30
};

// Should throw exception
var act3 = () => configuration.Validate(invalidIntervalSettings);
act3.Should().Throw<ValidationException>();

// Test 4: Validate_ShouldThrowException_WhenAlertCooldownMinutesIsInvalid
// Create invalid configuration with zero alert cooldown
var invalidCooldownSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 0,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30
};

// Should throw exception
var act4 = () => configuration.Validate(invalidCooldownSettings);
act4.Should().Throw<ValidationException>();

// Test 5: Validate_ShouldThrowException_WhenMaxAlertsPerUserIsInvalid
// Create invalid configuration with negative max alerts
var invalidMaxAlertsSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = -5,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30
};

// Should throw exception
var act5 = () => configuration.Validate(invalidMaxAlertsSettings);
act5.Should().Throw<ValidationException>();

// Test 6: Validate_ShouldThrowException_WhenHistoryRetentionDaysIsInvalid
// Create invalid configuration with negative retention days
var invalidRetentionSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = -7
};

// Should throw exception
var act6 = () => configuration.Validate(invalidRetentionSettings);
act6.Should().Throw<ValidationException>();

// Test 7: Validate_ShouldThrowException_WhenDefaultPriceChangeThresholdIsNegative
// Create invalid configuration with negative price change threshold
var invalidPriceThresholdSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = -1.0m,
    DefaultSpreadThreshold = 1.5m,
    HistoryRetentionDays = 30
};

// Should throw exception
var act7 = () => configuration.Validate(invalidPriceThresholdSettings);
act7.Should().Throw<ValidationException>();

// Test 8: Validate_ShouldThrowException_WhenDefaultSpreadThresholdIsNegative
// Create invalid configuration with negative spread threshold
var invalidSpreadSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance-p2p.db;Version=3;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 15,
    MaxAlertsPerUser = 20,
    DefaultPriceChangeThreshold = 2.5m,
    DefaultSpreadThreshold = -0.5m,
    HistoryRetentionDays = 30
};

// Should throw exception
var act8 = () => configuration.Validate(invalidSpreadSettings);
act8.Should().Throw<ValidationException>();
```

## CommandParserTests

The `CommandParserTests` class contains unit tests for the `CommandParser` class, which is responsible for parsing command-line arguments into structured `CommandContext` objects. These tests verify that the parser correctly handles various argument patterns including commands, positional arguments, short and long options, flags, and mixed argument combinations. The test suite ensures proper parsing of help commands, command-only inputs, and complex argument scenarios with duplicate options and values containing spaces.

```csharp
using BinanceP2pMonitor.CLI;
using Microsoft.Extensions.Logging;
using NSubstitute;

// Create mock logger and service provider
var mockLogger = Substitute.For<ILogger<CommandParser>>();
var mockServiceProvider = Substitute.For<IServiceProvider>();

// Create the command parser
var commandParser = new CommandParser(mockLogger);

// Example 1: Parse help command (no arguments)
var helpContext = commandParser.Parse(Array.Empty<string>(), mockServiceProvider);
Console.WriteLine($"Command: {helpContext.CommandName}"); // "help"
Console.WriteLine($"Arguments: {helpContext.Arguments.Count}"); // 0
Console.WriteLine($"Options: {helpContext.Options.Count}"); // 0
Console.WriteLine($"Flags: {helpContext.Flags.Count}"); // 0

// Example 2: Parse command with positional arguments
var monitorContext = commandParser.Parse(new[] { "monitor", "USDT", "UAH" }, mockServiceProvider);
Console.WriteLine($"Command: {monitorContext.CommandName}"); // "monitor"
Console.WriteLine($"Arguments: {string.Join(", ", monitorContext.Arguments)}"); // "USDT, UAH"
Console.WriteLine($"Options: {monitorContext.Options.Count}"); // 0

// Example 3: Parse command with long options
var longOptionsContext = commandParser.Parse(
    new[] { "monitor", "--asset=USDT", "--fiat=UAH" },
    mockServiceProvider
);
Console.WriteLine($"Command: {longOptionsContext.CommandName}"); // "monitor"
Console.WriteLine($"Asset: {longOptionsContext.GetOption("asset")}"); // "USDT"
Console.WriteLine($"Fiat: {longOptionsContext.GetOption("fiat")}"); // "UAH"

// Example 4: Parse command with flags
var flagContext = commandParser.Parse(new[] { "monitor", "-v", "-d" }, mockServiceProvider);
Console.WriteLine($"Command: {flagContext.CommandName}"); // "monitor"
Console.WriteLine($"Verbose flag: {flagContext.HasFlag("v")}"); // true
Console.WriteLine($"Debug flag: {flagContext.HasFlag("d")}"); // true

// Example 5: Parse command with mixed arguments
var mixedContext = commandParser.Parse(
    new[] { "monitor", "BTC", "EUR", "--limit=10", "-v", "-o", "json" },
    mockServiceProvider
);
Console.WriteLine($"Command: {mixedContext.CommandName}"); // "monitor"
Console.WriteLine($"Arguments: {string.Join(", ", mixedContext.Arguments)}"); // "BTC, EUR"
Console.WriteLine($"Limit option: {mixedContext.GetOption("limit")}"); // "10"
Console.WriteLine($"Output option: {mixedContext.GetOption("o")}"); // "json"
Console.WriteLine($"Verbose flag: {mixedContext.HasFlag("v")}"); // true

// Example 6: Parse command with option values containing spaces
var spaceContext = commandParser.Parse(
    new[] { "alert", "--message=hello world", "-c", "USDT/UAH > 10" },
    mockServiceProvider
);
Console.WriteLine($"Message: {spaceContext.GetOption("message")}"); // "hello world"
Console.WriteLine($"Condition: {spaceContext.GetOption("c")}"); // "USDT/UAH > 10"

// Example 7: Parse command with duplicate options (last one wins)
var duplicateContext = commandParser.Parse(
    new[] { "command", "--option=first", "--option=second", "-f", "third", "-f", "fourth" },
    mockServiceProvider
);
Console.WriteLine($"Option value: {duplicateContext.GetOption("option")}"); // "second"
Console.WriteLine($"Flag value: {duplicateContext.GetOption("f")}"); // "fourth"

// Example 8: Parse command distinguishing between flags and positional arguments starting with dash
var dashContext = commandParser.Parse(
    new[] { "command", "-p", "value", "-123" },
    mockServiceProvider
);
Console.WriteLine($"Option p: {dashContext.GetOption("p")}"); // "value"
Console.WriteLine($"Argument: {dashContext.Arguments[0]}"); // "-123"
```

## RateLimiterTests

The `RateLimiterTests` class contains unit tests for the rate limiter functionality, verifying that requests are properly throttled according to configured limits. These tests ensure that the rate limiter correctly handles token bucket algorithm operations including request allowance, token refill, multiple independent keys, thread safety, and bucket management operations like reset and clearing.

```csharp
using BinanceP2pMonitor.Tests;
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

// Create a rate limiter with 100 requests per minute limit
var rateLimiter = new RateLimiter(maxRequests: 100, timeWindowSeconds: 60);

// Test 1: IsAllowed_ShouldAllowRequestsUpToMaxRequests - verify basic rate limiting
var key1 = "user123";
for (int i = 0; i < 100; i++)
{
    var isAllowed = rateLimiter.IsAllowed(key1);
    isAllowed.Should().BeTrue($"Request {i + 1} should be allowed");
}

// Test 2: IsAllowed_ShouldRefillTokensAfterTimeWindow - verify tokens are refilled after time window
var isAllowedAfterWindow = rateLimiter.IsAllowed(key1);
// After 100 requests, the next should be denied
isAllowedAfterWindow.Should().BeFalse("Request 101 should be denied");

// Wait for tokens to refill (simulate time passing)
await Task.Delay(61000); // Wait 61 seconds

var isAllowedAfterRefill = rateLimiter.IsAllowed(key1);
isAllowedAfterRefill.Should().BeTrue("Request after time window should be allowed");

// Test 3: IsAllowed_ShouldHandleMultipleKeysIndependently - verify different keys have independent rate limits
var key2 = "user456";
for (int i = 0; i < 100; i++)
{
    var isAllowed = rateLimiter.IsAllowed(key2);
    isAllowed.Should().BeTrue($"Request {i + 1} for key2 should be allowed");
}

// Test 4: IsAllowed_ShouldBeThreadSafe - verify thread safety with concurrent requests
var tasks = new List<Task<bool>>();
for (int i = 0; i < 50; i++)
{
    tasks.Add(Task.Run(() => rateLimiter.IsAllowed(key1)));
}

var results = await Task.WhenAll(tasks);
results.Should().AllSatisfy(result => result.Should().BeTrue("All concurrent requests should be allowed"));

// Test 5: GetRemainingTokens_ShouldReturnCorrectCount - verify remaining token count
var remainingTokens = rateLimiter.GetRemainingTokens(key1);
remainingTokens.Should().Be(100, "Should have 100 tokens initially");

// Use some tokens
for (int i = 0; i < 25; i++)
{
    rateLimiter.IsAllowed(key1);
}

var remainingAfterUsage = rateLimiter.GetRemainingTokens(key1);
remainingAfterUsage.Should().Be(75, "Should have 75 tokens after 25 requests");

// Test 6: GetRemainingTokens_ShouldReturnMaxRequestsForNonExistentKey - verify default for new keys
var newKeyRemaining = rateLimiter.GetRemainingTokens("newUser");
newKeyRemaining.Should().Be(100, "New key should have max requests available");

// Test 7: Reset_ShouldRestoreTokensForGivenKey - verify reset functionality
rateLimiter.IsAllowed(key1); // Use 1 token
rateLimiter.Reset(key1);
var tokensAfterReset = rateLimiter.GetRemainingTokens(key1);
tokensAfterReset.Should().Be(100, "Reset should restore all tokens");

// Test 8: Reset_ShouldNotAffectOtherKeys - verify reset only affects specified key
rateLimiter.IsAllowed(key2); // Use 1 token for key2
rateLimiter.Reset(key1); // Reset key1
var key2Tokens = rateLimiter.GetRemainingTokens(key2);
key2Tokens.Should().Be(99, "Reset of key1 should not affect key2");

// Test 9: Reset_ShouldDoNothingForNonExistentKey - verify reset on non-existent key
var beforeReset = rateLimiter.GetRemainingTokens("nonexistent");
rateLimiter.Reset("nonexistent");
var afterReset = rateLimiter.GetRemainingTokens("nonexistent");
afterReset.Should().Be(beforeReset, "Reset on non-existent key should not change anything");

// Test 10: ClearAll_ShouldClearAllBuckets - verify clear functionality
rateLimiter.IsAllowed(key1);
rateLimiter.IsAllowed(key2);
rateLimiter.ClearAll();
var key1AfterClear = rateLimiter.GetRemainingTokens(key1);
var key2AfterClear = rateLimiter.GetRemainingTokens(key2);
key1AfterClear.Should().Be(100, "ClearAll should reset key1 tokens");
key2AfterClear.Should().Be(100, "ClearAll should reset key2 tokens");

// Test 11: GetTimeUntilNextToken_ShouldReturnZero_WhenTokensAvailable - verify immediate availability
var timeUntilNext = rateLimiter.GetTimeUntilNextToken(key1);
timeUntilNext.Should().Be(TimeSpan.Zero, "Should return zero when tokens available");

// Test 12: GetTimeUntilNextToken_ShouldReturnPositiveTime_WhenNoTokensAvailable - verify wait time calculation
for (int i = 0; i < 100; i++)
{
    rateLimiter.IsAllowed(key1);
}

var waitTime = rateLimiter.GetTimeUntilNextToken(key1);
waitTime.Should().BeGreaterThan(TimeSpan.Zero, "Should return positive wait time when no tokens available");

// Test 13: GetTimeUntilNextToken_ShouldReturnNull_ForNonExistentKey - verify null for new keys
var newKeyWaitTime = rateLimiter.GetTimeUntilNextToken("newUser");
newKeyWaitTime.Should().BeNull("New key should return null (immediate availability)");
```

## BacktestOptions

// ... rest of content ...

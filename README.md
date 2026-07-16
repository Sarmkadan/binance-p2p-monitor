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

## BacktestOptions

// ... rest of content ...

![CI](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/binance-p2p-monitor)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)
[![Build](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/build.yml/badge.svg)](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/build.yml)
[![Docker](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/docker.yml/badge.svg)](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/docker.yml)

# binance-p2p-monitor

**Real-time Binance P2P price monitoring in .NET** — WebSocket feeds, Telegram alerts, spread analysis, and SQLite history.

A production-grade monitoring tool for Binance P2P traders who need reliable, low-latency price alerts across multiple trading pairs and fiat currencies.

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Examples](#examples)
- [API Reference](#api-reference)
- [Performance](#performance)
- [Troubleshooting](#troubleshooting)
- [Testing](#testing)
- [Related Projects](#related-projects)
- [Contributing](#contributing)
- [License](#license)

## Features

### Core Monitoring
- **Real-time WebSocket feeds** from Binance P2P API with automatic reconnection
- **Multi-asset monitoring** — BTC, ETH, BNB, USDT, and any supported pair
- **Multi-currency support** — USD, EUR, GBP, CNY, and 50+ fiat currencies
- **Configurable refresh intervals** — from 1 second to 5 minutes per pair
- **Rate-limited API calls** — burst handling with exponential backoff

### Alerts & Notifications
- **Price change alerts** — trigger when a pair moves beyond your threshold
- **Spread analysis** — identify buy/sell spread anomalies
- **Telegram notifications** — real-time alerts to your phone
- **Alert cooldown** — prevent alert fatigue with configurable wait times
- **Per-user alert limits** — cap notifications at 20 active alerts per user

### Data & Analytics
- **SQLite history** — persistent price data for analysis
- **Auto-cleanup** — retention policies on a configurable schedule
- **Export formats** — CSV, JSON, and formatted table output
- **Performance metrics** — track API latency, WebSocket uptime, and alert volume
- **Statistics collection** — hourly aggregation of monitoring KPIs

### Infrastructure
- **Dependency injection** — full service lifetime management
- **Event bus** — loosely-coupled component communication
- **Middleware pipeline** — logging, validation, exception handling
- **In-memory caching** — reduce redundant API calls by 60-80%
- **Hosted services** — background workers for monitoring and cleanup

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      CLI Entry Point                        │
│                    (CommandFactory)                         │
└────────┬────────────────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────────┐
    │  Command Pipeline                                  │
    │  ├─ MonitorCommand (real-time monitoring)         │
    │  ├─ AlertCommand (manage price alerts)            │
    │  ├─ HistoryCommand (query price history)          │
    │  ├─ ExportCommand (export data)                   │
    │  └─ StatusCommand (system health)                 │
    └────┬──────────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────────┐
    │  Service Layer                                     │
    │  ├─ PriceMonitoringService (WebSocket, REST)      │
    │  ├─ AlertService (evaluation & triggering)        │
    │  ├─ SpreadAnalysisService (pair analysis)         │
    │  ├─ PriceHistoryService (data aggregation)        │
    │  └─ WebSocketService (connection management)      │
    └────┬──────────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────────┐
    │  Data Access Layer                                 │
    │  ├─ PriceRepository (CRUD operations)             │
    │  ├─ AlertRepository (alert persistence)           │
    │  ├─ HistoryRepository (aggregation queries)       │
    │  └─ TradeOfferRepository (market data)            │
    └────┬──────────────────────────────────────────────┘
         │
    ┌────▼──────────────────────────────────────────────┐
    │  Data Layer                                        │
    │  ├─ DatabaseContext (schema management)           │
    │  ├─ SQLite Connection (persistence)               │
    │  └─ EventBus (inter-component messaging)          │
    └────────────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites
- .NET 10 SDK or runtime
- SQLite 3.x (included with System.Data.SQLite)
- Telegram Bot Token (optional, for notifications)
- Binance API key (optional, for advanced monitoring)

### 5-Minute Setup

```bash
# Clone the repository
git clone https://github.com/Sarmkadan/binance-p2p-monitor.git
cd binance-p2p-monitor

# Build the project
dotnet build -c Release

# Configure (copy template)
cp appsettings.json appsettings.local.json
# Edit appsettings.local.json with your API keys

# Run monitoring
dotnet run -- monitor --assets BTC,ETH --fiats USD,EUR --interval 30
```

## Installation

### Method 1: Build from Source

```bash
git clone https://github.com/Sarmkadan/binance-p2p-monitor.git
cd binance-p2p-monitor
dotnet build -c Release -o ./bin/release
./bin/release/binance-p2p-monitor --version
```

### Method 2: Docker

```bash
docker build -t binance-p2p-monitor .
docker run -it \
  -e AppSettings__TelegramBotToken="your-token" \
  binance-p2p-monitor \
  monitor --assets BTC --fiats USD
```

### Method 3: Docker Compose

```bash
docker-compose up -d
# Logs available via: docker-compose logs -f app
```

### Method 4: Release Binaries

Download pre-built binaries from [Releases](https://github.com/Sarmkadan/binance-p2p-monitor/releases).

```bash
# macOS/Linux
tar -xzf binance-p2p-monitor-1.2.0-linux-x64.tar.gz
./binance-p2p-monitor --help

# Windows
7z x binance-p2p-monitor-1.2.0-win-x64.zip
.\binance-p2p-monitor.exe --help
```

## Configuration

### appsettings.json Structure

```json
{
  "AppSettings": {
    "DatabaseConnectionString": "Data Source=binance_p2p.db;Version=3;",
    "BinanceApiKey": "your-api-key",
    "BinanceApiSecret": "your-api-secret",
    "TelegramBotToken": "your-bot-token",
    "TelegramAdminChatId": "your-chat-id",
    "MonitoringIntervalSeconds": 30,
    "AlertCooldownMinutes": 5,
    "MaxAlertsPerUser": 20,
    "DefaultPriceChangeThreshold": 2.0,
    "DefaultSpreadThreshold": 1.5,
    "HistoryRetentionDays": 30,
    "MaxHistoryRecords": 100000,
    "EnableWebSocket": true,
    "EnableTelegramNotifications": true,
    "EnableAutoCleanup": true,
    "LogLevel": "Information",
    "LogPath": "./logs",
    "MonitoredAssets": ["BTC", "ETH", "BNB", "USDT"],
    "MonitoredFiats": ["USD", "EUR", "GBP"]
  }
}
```

### Environment Variables

Override any setting via environment variables using the `AppSettings__` prefix:

```bash
export AppSettings__TelegramBotToken="bot-token-here"
export AppSettings__MonitoringIntervalSeconds="15"
dotnet run
```

### Advanced Configuration

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| DatabaseConnectionString | string | `binance_p2p_monitor.db` | SQLite connection URI |
| MonitoringIntervalSeconds | int | 30 | Polling frequency |
| AlertCooldownMinutes | int | 5 | Minutes between alert repeats |
| MaxAlertsPerUser | int | 20 | Alert cap per trader |
| DefaultPriceChangeThreshold | decimal | 2.0 | % change to trigger alert |
| DefaultSpreadThreshold | decimal | 1.5 | % spread to trigger alert |
| HistoryRetentionDays | int | 30 | Days to keep historical data |
| MaxHistoryRecords | int | 100000 | Max rows in price history |
| DatabaseCommandTimeoutSeconds | int | 30 | Query timeout |
| EnableWebSocket | bool | true | Use WebSocket feeds |
| EnableTelegramNotifications | bool | true | Send Telegram alerts |
| EnableAutoCleanup | bool | true | Auto-delete old records |
| LogLevel | string | Information | Verbosity: Debug/Info/Warning/Error |

## Usage

### Monitor Command

Real-time monitoring with WebSocket feeds.

```bash
dotnet run -- monitor \
  --assets BTC,ETH,BNB \
  --fiats USD,EUR \
  --interval 30 \
  --output json
```

**Flags:**
- `--assets` — Comma-separated crypto pairs (default: BTC,ETH,BNB,USDT)
- `--fiats` — Comma-separated fiat currencies (default: USD,EUR,GBP)
- `--interval` — Monitoring frequency in seconds (default: 30)
- `--output` — Format: json|csv|table (default: table)

### Alert Command

Create and manage price alerts.

```bash
# Create a BTC/USD alert: trigger when price moves >2%
dotnet run -- alert \
  --create \
  --asset BTC \
  --fiat USD \
  --type price_change \
  --threshold 2.0 \
  --user trader123

# List active alerts
dotnet run -- alert --list

# Remove alert by ID
dotnet run -- alert --remove 42

# Pause all alerts
dotnet run -- alert --pause-all
```

### History Command

Query price history.

```bash
# Last 100 BTC/USD prices
dotnet run -- history \
  --asset BTC \
  --fiat USD \
  --limit 100 \
  --format json

# Price range for analysis
dotnet run -- history \
  --asset ETH \
  --fiat USD \
  --from "2026-04-01" \
  --to "2026-05-01"
```

### Export Command

Export data to CSV or JSON.

```bash
# Export all prices
dotnet run -- export \
  --format csv \
  --output prices.csv \
  --include-alerts

# Export alerts
dotnet run -- export \
  --format json \
  --output alerts.json \
  --entity alerts
```

### Status Command

Check system health and statistics.

```bash
dotnet run -- status
```

Output includes:
- Uptime, monitoring duration
- WebSocket connection status
- Price sample count
- Alert trigger count
- Database size
- Performance metrics (latency, throughput)

## Examples

See the `examples/` directory for complete working programs:

1. **real-time-monitor.cs** — WebSocket price feed with console output
2. **telegram-alerts.cs** — Multi-asset monitoring with Telegram notifications
3. **spread-analyzer.cs** — Identify buy/sell spread anomalies
4. **price-exporter.cs** — Export history to CSV with aggregation
5. **auto-trader-monitor.cs** — Monitor prices and trigger buy signals
6. **slack-integration.cs** — Custom webhook for Slack notifications
7. **historical-analysis.cs** — Analyze price trends over time

### Example 1: Real-Time Monitoring

```csharp
var monitoringService = host.Services.GetRequiredService<IPriceMonitoringService>();

// Start listening for BTC/USD
var subscription = await monitoringService.MonitorPriceAsync("BTC", "USD", 
    priceChange: (price) =>
    {
        Console.WriteLine($"BTC/USD: {price.Bid:F2} bid, {price.Ask:F2} ask");
    },
    interval: TimeSpan.FromSeconds(30));

// Unsubscribe when done
await monitoringService.UnsubscribePriceAsync(subscription);
```

### Example 2: Alert Configuration

```csharp
var alertService = host.Services.GetRequiredService<IAlertService>();

// Create alert: notify if BTC drops 5% or rises 3%
await alertService.CreateAlertAsync(new PriceAlert
{
    Asset = "BTC",
    Fiat = "USD",
    AlertType = AlertType.PriceChange,
    UpperThreshold = 3.0m,
    LowerThreshold = -5.0m,
    UserId = "trader@example.com",
    IsActive = true
});

// Evaluate and trigger alerts
var triggered = await alertService.EvaluateAlertsAsync(price);
foreach (var alert in triggered)
{
    Console.WriteLine($"Alert {alert.Id}: {alert.Asset}/{alert.Fiat}");
}
```

### Example 3: Spread Analysis

```csharp
var spreadService = host.Services.GetRequiredService<ISpreadAnalysisService>();

var spread = await spreadService.AnalyzePairAsync("BTC", "USD");

Console.WriteLine($"Buy Price: {spread.BuyPrice:F2}");
Console.WriteLine($"Sell Price: {spread.SellPrice:F2}");
Console.WriteLine($"Spread: {spread.SpreadPercentage:F2}%");
Console.WriteLine($"Average Price: {spread.AveragePrice:F2}");
```

## API Reference

### Interfaces

#### IPriceMonitoringService

```csharp
Task<Price> GetPriceAsync(string asset, string fiat);
Task<IEnumerable<Price>> GetPricesAsync(string asset, IEnumerable<string> fiats);
Task<string> MonitorPriceAsync(string asset, string fiat, 
    Action<Price> onPriceChange, TimeSpan interval);
Task UnsubscribePriceAsync(string subscriptionId);
```

#### IAlertService

```csharp
Task<PriceAlert> CreateAlertAsync(PriceAlert alert);
Task RemoveAlertAsync(int alertId);
Task<IEnumerable<PriceAlert>> GetActiveAlertsAsync(string userId);
Task<IEnumerable<PriceAlert>> EvaluateAlertsAsync(Price price);
```

#### ISpreadAnalysisService

```csharp
Task<Spread> AnalyzePairAsync(string asset, string fiat);
Task<IEnumerable<Spread>> AnalyzeAssetsAsync(IEnumerable<string> assets, string fiat);
Task<decimal> CalculateAverageSpreadAsync(string asset, string fiat, int hours = 24);
```

#### IPriceHistoryService

```csharp
Task RecordPriceAsync(Price price);
Task<IEnumerable<PriceHistory>> GetHistoryAsync(string asset, string fiat, 
    int limit = 100);
Task<IEnumerable<PriceHistory>> GetHistoryRangeAsync(string asset, string fiat, 
    DateTime from, DateTime to);
Task<PriceHistory> GetAggregatedAsync(string asset, string fiat, TimeSpan period);
```

## Performance

### System Benchmarks (Intel i7-11700K, 32GB RAM)

| Operation | Time | Throughput |
|-----------|------|-----------|
| Price fetch (REST) | 150-200ms | 50 pairs/sec |
| Price fetch (WebSocket) | 20-50ms | 500 pairs/sec |
| Alert evaluation | <1ms per alert | 10k alerts/sec |
| History query (1000 rows) | 5-10ms | Instant |
| Spread analysis | 30ms | 30 pairs/sec |

### Micro-Benchmarks (BenchmarkDotNet 0.14, .NET 10, Intel i9-12900K, 32GB DDR5)

Run the benchmarks yourself:

```bash
cd benchmarks/binance-p2p-monitor.Benchmarks
dotnet run -c Release -- --filter *
```

#### PriceCalculator

| Method | Mean | Error | StdDev | Allocated |
|--------|------|-------|--------|-----------|
| CalculateSpread | 18.4 ns | 0.12 ns | 0.11 ns | - |
| CalculatePercentageChange | 17.9 ns | 0.09 ns | 0.08 ns | - |
| FormatPrice (no symbol) | 84.3 ns | 0.41 ns | 0.38 ns | 72 B |
| FormatPrice (with symbol) | 96.1 ns | 0.53 ns | 0.50 ns | 96 B |
| CalculateMovingAverage (n=1000, period=20) | 312 ns | 1.8 ns | 1.7 ns | - |
| CalculateMovingAverage (n=1000, period=200) | 2.81 μs | 0.014 μs | 0.013 μs | - |
| CalculateStandardDeviation (n=1000) | 5.62 μs | 0.031 μs | 0.029 μs | - |
| CalculateStandardDeviation (n=50) | 284 ns | 1.5 ns | 1.4 ns | - |

#### SpreadAnalysis

| Method | Mean | Error | StdDev | Allocated |
|--------|------|-------|--------|-----------|
| AnalyzeSpread (inline arithmetic) | 12.1 ns | 0.07 ns | 0.06 ns | - |
| ComputeSpreadStatistics (n=500, loop) | 3.94 μs | 0.021 μs | 0.020 μs | - |
| FindAnomalies_ZScore (n=500, loop) | 4.71 μs | 0.028 μs | 0.026 μs | 2.1 KB |
| FindAnomalies_ZScore (n=500, ArrayPool) | 4.68 μs | 0.025 μs | 0.024 μs | 64 B |

#### StringExtensions

| Method | Mean | Error | StdDev | Allocated |
|--------|------|-------|--------|-----------|
| SplitCamelCase (cached regex) | 748 ns | 4.3 ns | 4.0 ns | 416 B |
| ToSnakeCase (cached regex) | 612 ns | 3.1 ns | 2.9 ns | 384 B |
| ToPascalCase | 124 ns | 0.8 ns | 0.8 ns | 192 B |
| Truncate (triggered) | 22.8 ns | 0.14 ns | 0.13 ns | 72 B |
| Truncate (no-op) | 3.1 ns | 0.02 ns | 0.02 ns | - |
| ToDecimalOrNull (valid, span) | 81.4 ns | 0.48 ns | 0.45 ns | - |
| ToIntOrNull (valid, span) | 38.7 ns | 0.23 ns | 0.22 ns | - |

### Resource Usage

- **Memory**: 80-150MB baseline, +2MB per 10k historical records
- **Disk**: SQLite grows ~1MB per 10k price records per day
- **Network**: 500KB-1MB per day per pair (WebSocket mode)
- **CPU**: <5% on single core (monitoring mode)

### Optimization Tips

1. **Enable caching** — Reduces API calls by 60-80%
2. **Use WebSocket** — 10x faster than polling
3. **Increase monitoring interval** — Trade latency for throughput
4. **Configure retention policies** — Prevent database bloat
5. **Batch API calls** — Monitor 20 pairs in 1 API call

## Troubleshooting

### WebSocket Connection Failed

**Symptom:** "WebSocket connection failed" in logs

**Solutions:**
1. Check firewall/NAT settings
2. Verify Binance API endpoint is accessible
3. Check internet connection stability
4. Reduce monitoring interval to allow recovery time
5. Enable verbose logging: `"LogLevel": "Debug"`

### Telegram Notifications Not Sending

**Symptom:** Alerts trigger but no Telegram messages arrive

**Solutions:**
1. Verify bot token: `curl https://api.telegram.org/botYOUR_TOKEN/getMe`
2. Check chat ID matches: `/start` bot and get your chat ID
3. Ensure bot has message permissions
4. Check `EnableTelegramNotifications: true` in config
5. View notification errors: `"LogLevel": "Debug"`

### Database Locked

**Symptom:** "Database is locked" error

**Solutions:**
1. Only one instance can access database at a time
2. Check for zombie processes: `lsof binance_p2p.db`
3. Delete `.db-wal` and `.db-shm` files if locked
4. Restart the application
5. Consider remote database (PostgreSQL) for concurrent access

### High Memory Usage

**Symptom:** Memory grows unbounded over time

**Solutions:**
1. Check `MaxHistoryRecords` setting
2. Enable `EnableAutoCleanup`
3. Reduce monitoring interval to decrease in-memory cache
4. Monitor active alerts: `alert --list | wc -l`
5. Restart application daily if long-running

### Performance Degradation

**Symptom:** Response times increase over time

**Solutions:**
1. Run database maintenance: `VACUUM` in SQLite
2. Check disk I/O: `iostat -x 1`
3. Increase `DatabaseCommandTimeoutSeconds`
4. Archive old price history
5. Consider upgrading to SSD

## Testing

Run the full test suite:

```bash
dotnet test
```

Run with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Run only a specific test class:

```bash
dotnet test --filter "FullyQualifiedName~SpreadAnalysisServiceTests"
```

Run the benchmarks:

```bash
cd benchmarks/binance-p2p-monitor.Benchmarks
dotnet run -c Release -- --filter *
```

Tests are located in `tests/binance-p2p-monitor.Tests/` and cover services, repositories, utilities, and CLI commands using xUnit and FluentAssertions.

## Related Projects

- [telegram-bot-framework-dotnet](https://github.com/sarmkadan/telegram-bot-framework-dotnet) - Opinionated Telegram bot framework for .NET - commands, menus, state machine, middleware
- [redis-cache-patterns](https://github.com/sarmkadan/redis-cache-patterns) - Production-ready Redis caching patterns for .NET - cache-aside, write-through, distributed lock

### Integration Examples

**Using with telegram-bot-framework-dotnet** — expose live P2P spread data as bot commands:

```csharp
services.AddTelegramBotFramework(opt => opt.Token = config["TelegramBotToken"])
    .AddCommand<MonitorBotCommand>("/monitor")
    .AddCommand<AlertBotCommand>("/alert");

// /monitor BTC — replies with live spread
public async Task ExecuteAsync(IBotContext ctx)
{
    var spread = await _spreadService.AnalyzePairAsync(ctx.Args[0], "USD");
    await ctx.ReplyAsync($"{ctx.Args[0]}/USD spread: {spread.SpreadPercentage:F2}%");
}
```

**Using with redis-cache-patterns** — share the price cache across multiple monitor instances:

```csharp
services.AddRedisCachePatterns(opt => opt.ConnectionString = config["Redis"])
    .UseCacheAside<Price>(ttl: TimeSpan.FromSeconds(30));

// Cache miss falls through to Binance API; hit returns cached price in <1 ms
var price = await _cache.GetOrSetAsync(
    $"p2p:{asset}:{fiat}",
    () => _monitoringService.GetPriceAsync(asset, fiat));
```

## Contributing

Contributions welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Write tests for new functionality
4. Ensure all tests pass: `dotnet test`
5. Format code: `dotnet format`
6. Commit with descriptive messages
7. Push and open a pull request

### Code Standards

- Use latest C# language features (.NET 10)
- Enable nullable reference types
- Follow PascalCase for public members
- Add XML doc comments for public APIs
- Keep methods under 30 lines when possible
- Write unit tests for business logic

## License

MIT License © 2026 Vladyslav Zaiets

See [LICENSE](LICENSE) for details.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)

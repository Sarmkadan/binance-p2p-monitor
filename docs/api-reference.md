# API Reference

Complete reference for all public interfaces, command-line options, and integration points.

## CLI Commands

### monitor

Start real-time price monitoring.

**Syntax:**
```bash
dotnet run -- monitor [--assets PAIRS] [--fiats CURRENCIES] [--interval SECONDS] [--output FORMAT]
```

**Options:**
| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--assets` | string | BTC,ETH,BNB,USDT | Comma-separated crypto assets to monitor |
| `--fiats` | string | USD,EUR,GBP | Comma-separated fiat currencies |
| `--interval` | int | 30 | Polling interval in seconds (min: 1, max: 300) |
| `--output` | string | table | Output format: `table`, `json`, `csv` |
| `--continuous` | bool | true | Keep monitoring until Ctrl+C |
| `--include-spread` | bool | false | Show bid/ask spread in output |
| `--log-file` | string | (none) | Write output to file |

**Examples:**
```bash
# Monitor BTC only, every 10 seconds
dotnet run -- monitor --assets BTC --interval 10

# Monitor multiple pairs, JSON output
dotnet run -- monitor --assets BTC,ETH,BNB --fiats USD,EUR --output json

# With spread analysis, save to file
dotnet run -- monitor --include-spread --log-file prices.csv --output csv
```

### alert

Manage price alerts.

**Syntax:**
```bash
dotnet run -- alert [OPERATION] [--asset ASSET] [--fiat FIAT] [--type TYPE] [--threshold VALUE] [--user USER]
```

**Operations:**
| Operation | Description |
|-----------|-------------|
| `--create` | Create new alert |
| `--list` | List all active alerts |
| `--remove ID` | Remove alert by ID |
| `--pause ID` | Pause alert without deleting |
| `--resume ID` | Resume paused alert |
| `--pause-all` | Pause all alerts temporarily |
| `--clear-history` | Clear alert trigger history |

**Alert Types:**
| Type | Trigger Condition |
|------|-------------------|
| `price_change` | Price moves by threshold % from baseline |
| `spread_anomaly` | Bid/ask spread exceeds threshold % |
| `price_level` | Price crosses absolute threshold value |

**Options:**
| Option | Type | Required | Description |
|--------|------|----------|-------------|
| `--asset` | string | yes | Crypto asset (e.g., BTC, ETH) |
| `--fiat` | string | yes | Fiat currency (e.g., USD, EUR) |
| `--type` | string | yes | Alert type |
| `--threshold` | decimal | yes | Threshold value (% or absolute) |
| `--user` | string | yes | User ID for alert |
| `--cooldown` | int | no | Minutes between repeats (default: 5) |
| `--lower` | bool | no | For price_change: trigger on decrease |
| `--upper` | bool | no | For price_change: trigger on increase |
| `--enabled` | bool | no | Initial state (default: true) |

**Examples:**
```bash
# Alert if BTC/USD drops 5% or more
dotnet run -- alert --create --asset BTC --fiat USD --type price_change \
  --threshold 5.0 --user trader1 --lower

# Alert if ETH/EUR rises 3% or more
dotnet run -- alert --create --asset ETH --fiat EUR --type price_change \
  --threshold 3.0 --user trader1 --upper

# Alert if spread exceeds 2%
dotnet run -- alert --create --asset BTC --fiat USD --type spread_anomaly \
  --threshold 2.0 --user trader1

# List all alerts
dotnet run -- alert --list

# Remove specific alert
dotnet run -- alert --remove 5
```

### history

Query price history.

**Syntax:**
```bash
dotnet run -- history [--asset ASSET] [--fiat FIAT] [--limit LIMIT] [--from DATE] [--to DATE] [--format FORMAT]
```

**Options:**
| Option | Type | Description |
|--------|------|-------------|
| `--asset` | string | Asset symbol (required) |
| `--fiat` | string | Fiat currency (required) |
| `--limit` | int | Max records to return (default: 100) |
| `--from` | datetime | Start date (ISO format: 2026-05-01) |
| `--to` | datetime | End date (ISO format: 2026-05-04) |
| `--format` | string | Output format: `table`, `json`, `csv` |
| `--aggregate` | string | Aggregation period: `5m`, `15m`, `1h`, `1d` |

**Examples:**
```bash
# Last 100 prices
dotnet run -- history --asset BTC --fiat USD --limit 100

# Price range for analysis
dotnet run -- history --asset ETH --fiat USD --from 2026-04-01 --to 2026-05-04

# Hourly averages
dotnet run -- history --asset BTC --fiat EUR --aggregate 1h --format csv

# JSON export for processing
dotnet run -- history --asset BNB --fiat USD --limit 500 --format json
```

### export

Export data to file.

**Syntax:**
```bash
dotnet run -- export [--format FORMAT] [--output FILE] [--entity ENTITY] [--limit LIMIT]
```

**Options:**
| Option | Type | Description |
|--------|------|-------------|
| `--format` | string | Output format: `csv`, `json` (required) |
| `--output` | string | Output file path (required) |
| `--entity` | string | Data type: `prices`, `alerts`, `offers` (default: prices) |
| `--limit` | int | Max records to export (default: 10000) |
| `--include-alerts` | bool | Include triggered alert events |
| `--include-stats` | bool | Include system statistics |

**Examples:**
```bash
# Export recent prices as CSV
dotnet run -- export --format csv --output prices.csv --limit 5000

# Export all alerts as JSON
dotnet run -- export --format json --output alerts.json --entity alerts

# Export with metadata
dotnet run -- export --format csv --output export.csv \
  --include-alerts --include-stats
```

### status

Display system status and statistics.

**Syntax:**
```bash
dotnet run -- status [--format FORMAT]
```

**Output:**
```
╔═══════════════════════════════════════════════════╗
║           Binance P2P Monitor Status              ║
╠═══════════════════════════════════════════════════╣
║ Uptime:                     4h 23m 12s            ║
║ Database:                   Connected ✓           ║
║ WebSocket:                  Connected ✓           ║
║ Telegram:                   Configured ✓          ║
╠═══════════════════════════════════════════════════╣
║ Monitoring Statistics                             ║
├─────────────────────────────────────────────────┤
║ Price updates:              45,234                ║
║ Active alerts:              8                     ║
║ Triggered alerts:           3 (last 24h)         ║
║ Database records:           123,456               ║
║ Database size:              45.2 MB               ║
╠═══════════════════════════════════════════════════╣
║ Performance Metrics                               ║
├─────────────────────────────────────────────────┤
║ Avg API response:           156ms                 ║
║ Cache hit ratio:            87%                   ║
║ WebSocket uptime:           99.8%                 ║
╚═══════════════════════════════════════════════════╝
```

**Options:**
| Option | Type | Description |
|--------|------|-------------|
| `--format` | string | Output format: `table`, `json` |
| `--detailed` | bool | Show extended metrics |

### version

Display application version.

**Syntax:**
```bash
dotnet run -- version
```

### help

Display help for commands.

**Syntax:**
```bash
dotnet run -- help [COMMAND]
```

**Examples:**
```bash
# General help
dotnet run -- help

# Command-specific help
dotnet run -- help monitor
dotnet run -- help alert
```

## Service Interfaces

### IPriceMonitoringService

Real-time price monitoring.

```csharp
public interface IPriceMonitoringService
{
    // Fetch current price
    Task<Price> GetPriceAsync(string asset, string fiat);
    
    // Fetch multiple prices
    Task<IEnumerable<Price>> GetPricesAsync(string asset, IEnumerable<string> fiats);
    
    // Start monitoring with callback
    Task<string> MonitorPriceAsync(
        string asset, 
        string fiat, 
        Action<Price> onPriceChange, 
        TimeSpan interval);
    
    // Stop monitoring
    Task UnsubscribePriceAsync(string subscriptionId);
}
```

**Example Usage:**
```csharp
var service = serviceProvider.GetRequiredService<IPriceMonitoringService>();

// Single price fetch
var price = await service.GetPriceAsync("BTC", "USD");
Console.WriteLine($"BTC/USD: {price.Bid} - {price.Ask}");

// Batch fetch
var prices = await service.GetPricesAsync("BTC", new[] { "USD", "EUR", "GBP" });

// Subscribe to updates
var subscriptionId = await service.MonitorPriceAsync(
    "BTC", "USD",
    price => Console.WriteLine($"New price: {price.Bid}"),
    interval: TimeSpan.FromSeconds(30));

// Unsubscribe
await service.UnsubscribePriceAsync(subscriptionId);
```

### IAlertService

Alert management and evaluation.

```csharp
public interface IAlertService
{
    // Create alert
    Task<PriceAlert> CreateAlertAsync(PriceAlert alert);
    
    // Get alerts
    Task<IEnumerable<PriceAlert>> GetActiveAlertsAsync(string userId);
    Task<IEnumerable<PriceAlert>> GetAlertAsync(int alertId);
    
    // Update alert
    Task UpdateAlertAsync(PriceAlert alert);
    
    // Remove alert
    Task RemoveAlertAsync(int alertId);
    
    // Evaluate against price
    Task<IEnumerable<PriceAlert>> EvaluateAlertsAsync(Price price);
}
```

**Example Usage:**
```csharp
var service = serviceProvider.GetRequiredService<IAlertService>();

// Create alert
var alert = new PriceAlert
{
    Asset = "BTC",
    Fiat = "USD",
    AlertType = AlertType.PriceChange,
    UpperThreshold = 3.0m,      // Trigger on 3% increase
    LowerThreshold = -5.0m,     // Trigger on 5% decrease
    UserId = "trader@example.com",
    CooldownMinutes = 5,
    IsActive = true
};
await service.CreateAlertAsync(alert);

// Get user's alerts
var alerts = await service.GetActiveAlertsAsync("trader@example.com");
foreach (var a in alerts)
{
    Console.WriteLine($"Alert {a.Id}: {a.Asset}/{a.Fiat}");
}

// Evaluate on new price
var price = await priceService.GetPriceAsync("BTC", "USD");
var triggered = await service.EvaluateAlertsAsync(price);
```

### ISpreadAnalysisService

Bid/ask spread analysis.

```csharp
public interface ISpreadAnalysisService
{
    // Single pair analysis
    Task<Spread> AnalyzePairAsync(string asset, string fiat);
    
    // Multiple pairs
    Task<IEnumerable<Spread>> AnalyzeAssetsAsync(
        IEnumerable<string> assets, 
        string fiat);
    
    // Historical average
    Task<decimal> CalculateAverageSpreadAsync(
        string asset, 
        string fiat, 
        int hours = 24);
}
```

**Example Usage:**
```csharp
var service = serviceProvider.GetRequiredService<ISpreadAnalysisService>();

// Analyze BTC/USD spread
var spread = await service.AnalyzePairAsync("BTC", "USD");
Console.WriteLine($"Spread: {spread.SpreadPercentage:F2}%");
Console.WriteLine($"Buy: {spread.BuyPrice:F2}, Sell: {spread.SellPrice:F2}");

// Analyze multiple assets
var spreads = await service.AnalyzeAssetsAsync(
    new[] { "BTC", "ETH", "BNB" }, 
    "USD");

// Get 24-hour average spread
var avgSpread = await service.CalculateAverageSpreadAsync("BTC", "USD", hours: 24);
```

### IPriceHistoryService

Historical data aggregation.

```csharp
public interface IPriceHistoryService
{
    // Record price
    Task RecordPriceAsync(Price price);
    
    // Query history
    Task<IEnumerable<PriceHistory>> GetHistoryAsync(
        string asset, 
        string fiat, 
        int limit = 100);
    
    // Range query
    Task<IEnumerable<PriceHistory>> GetHistoryRangeAsync(
        string asset, 
        string fiat, 
        DateTime from, 
        DateTime to);
    
    // Aggregated data
    Task<PriceHistory> GetAggregatedAsync(
        string asset, 
        string fiat, 
        TimeSpan period);
}
```

**Example Usage:**
```csharp
var service = serviceProvider.GetRequiredService<IPriceHistoryService>();

// Record price
var price = new Price { Asset = "BTC", Fiat = "USD", Bid = 45000, Ask = 45100 };
await service.RecordPriceAsync(price);

// Query recent history
var history = await service.GetHistoryAsync("BTC", "USD", limit: 1000);

// Query date range
var filtered = await service.GetHistoryRangeAsync(
    "BTC", "USD",
    from: new DateTime(2026, 04, 01),
    to: new DateTime(2026, 05, 04));

// Get hourly aggregates
var hourly = await service.GetAggregatedAsync("BTC", "USD", TimeSpan.FromHours(1));
```

### IWebSocketService

WebSocket connection management.

```csharp
public interface IWebSocketService
{
    // Start connection
    Task ConnectAsync(string assetSymbol);
    
    // Handle message
    Task HandleMessageAsync(string message);
    
    // Disconnect
    Task DisconnectAsync();
    
    // Connection state
    bool IsConnected { get; }
    DateTime LastMessageTime { get; }
}
```

## Data Models

### Price

```csharp
public class Price
{
    public int Id { get; set; }
    public string Asset { get; set; }          // "BTC", "ETH", etc.
    public string Fiat { get; set; }           // "USD", "EUR", etc.
    public decimal Bid { get; set; }           // Buy price
    public decimal Ask { get; set; }           // Sell price
    public DateTime Timestamp { get; set; }
    public int Volume { get; set; }            // Trade volume
}
```

### PriceAlert

```csharp
public class PriceAlert
{
    public int Id { get; set; }
    public string Asset { get; set; }
    public string Fiat { get; set; }
    public AlertType AlertType { get; set; }
    public decimal? UpperThreshold { get; set; }
    public decimal? LowerThreshold { get; set; }
    public string UserId { get; set; }
    public int CooldownMinutes { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Spread

```csharp
public class Spread
{
    public string Asset { get; set; }
    public string Fiat { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal SpreadPercentage { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Error Codes

| Code | Meaning | Recovery |
|------|---------|----------|
| `ERR_CONFIG_INVALID` | Configuration validation failed | Check appsettings.json |
| `ERR_DATABASE_CONNECT` | Cannot connect to database | Verify SQLite file exists |
| `ERR_API_TIMEOUT` | Binance API call timed out | Retry, check network |
| `ERR_WEBSOCKET_FAILED` | WebSocket connection failed | Retry with exponential backoff |
| `ERR_TELEGRAM_FAILED` | Telegram notification failed | Check bot token/chat ID |
| `ERR_RATE_LIMIT` | API rate limit exceeded | Reduce monitoring frequency |
| `ERR_INVALID_ASSET` | Unknown asset symbol | Use `--help monitor` for list |

## Environment Variables

Override configuration via environment variables:

```bash
# Database
AppSettings__DatabaseConnectionString="Data Source=/data/binance_p2p.db;Version=3;"

# API Keys
AppSettings__BinanceApiKey="your-api-key"
AppSettings__BinanceApiSecret="your-api-secret"

# Telegram
AppSettings__TelegramBotToken="123456789:ABCDefGhIjKlMn"
AppSettings__TelegramAdminChatId="987654321"

# Monitoring
AppSettings__MonitoringIntervalSeconds="30"
AppSettings__AlertCooldownMinutes="5"

# Features
AppSettings__EnableWebSocket="true"
AppSettings__EnableTelegramNotifications="true"
AppSettings__EnableAutoCleanup="true"

# Logging
AppSettings__LogLevel="Information"
AppSettings__LogPath="/var/log/binance-monitor"
```

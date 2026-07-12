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

Copy `appsettings.json` and set your values:

```json
{
  "AppSettings": {
    "DatabaseConnectionString": "DataSource=monitor.db",
    "TelegramBotToken": "your-token",
    "TelegramChatId": 123456789,
    "EnableTelegramNotifications": true,
    "DefaultSpreadThreshold": 0.3,
    "MaxAlertsPerUser": 20
  }
}
```

> **Tip:** Use the provided `appsettings.example.json` as a template and rename it to `appsettings.json` before editing.

## Configuration

The application reads its configuration from the **AppSettings** section of `appsettings.json`.  
All settings are bound to the `BinanceP2PMonitorOptions` class and validated at startup.

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| **DatabaseConnectionString** | `string` | *required* | SQLite connection string for the local database. |
| **BinanceApiKey** | `string` (optional) | empty | API key for Binance (required when `EnableWebSocket` is `true`). |
| **BinanceApiSecret** | `string` (optional) | empty | API secret for Binance. |
| **TelegramBotToken** | `string` (optional) | empty | Bot token for Telegram notifications. |
| **TelegramAdminChatId** | `string` (optional) | empty | Chat ID of the admin to receive alerts. |
| **MonitoringIntervalSeconds** | `int` | `30` | How often (in seconds) the monitor polls Binance. |
| **AlertCooldownMinutes** | `int` | `5` | Minimum minutes between alerts for the same condition. |
| **MaxAlertsPerUser** | `int` | `20` | Upper limit of alerts a single user can receive per day. |
| **DefaultPriceChangeThreshold** | `double` | `2.0` | Percentage change that triggers a price‑change alert. |
| **DefaultSpreadThreshold** | `double` | `1.5` | Percentage spread that triggers a spread alert. |
| **HistoryRetentionDays** | `int` | `30` | Number of days to keep price history records. |
| **MaxHistoryRecords** | `int` | `100000` | Maximum number of history rows stored in the DB. |
| **DatabaseCommandTimeoutSeconds** | `int` | `30` | Timeout for SQLite commands. |
| **EnableWebSocket** | `bool` | `true` | Turn on/off Binance WebSocket streaming. |
| **EnableTelegramNotifications** | `bool` | `true` | Turn on/off Telegram notifications. |
| **EnableAutoCleanup** | `bool` | `true` | Automatically purge old history according to retention settings. |
| **DailySummaryHourUtc** | `int` (0‑23) | `9` | Hour (UTC) when the daily summary is sent. |
| **LogLevel** | `string` | `Information` | Minimum log level for the application. |
| **LogPath** | `string` | `./logs` | Directory where log files are written. |
| **MonitoredAssets** | `string[]` | `[ "BTC", "ETH", "BNB", "USDT" ]` | List of crypto assets to monitor. |
| **MonitoredFiats** | `string[]` | `[ "USD", "EUR", "GBP" ]` | List of fiat currencies to monitor. |

### Using the Options

```csharp
builder.Services.Configure<BinanceP2PMonitorOptions>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddOptions<BinanceP2PMonitorOptions>()
                .Bind(builder.Configuration.GetSection("AppSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();
```

The above registration ensures the application will fail to start if any required setting is missing or contains an invalid value.

### Example file

An example configuration file without real secrets is provided as `appsettings.example.json`.  
Copy it to `appsettings.json` and fill in your own values before running the application.

## Usage

```bash
dotnet run -- monitor           # start monitoring
dotnet run -- status            # show current prices
dotnet run -- alert --add       # create alert
dotnet run -- history --hours 24
dotnet run -- export --format csv
dotnet run -- backtest
dotnet run -- help
```

## Examples

Check the [examples/](examples/) directory for practical implementation guidance:

- [BasicUsage.cs](examples/BasicUsage.cs) - Minimal setup
- [AdvancedUsage.cs](examples/AdvancedUsage.cs) - Custom configuration
- [IntegrationExample.cs](examples/IntegrationExample.cs) - ASP.NET DI integration

## Docker

You can use Docker to run the monitor as a containerized service.

### Building and Running with Docker

```bash
# Build the image
docker build -t binance-p2p-monitor .

# Run the container
docker run -v $(pwd)/data:/app/data binance-p2p-monitor monitor
```

### Using Docker Compose

For a managed experience with environment variable configuration, use `docker-compose`:

```bash
# Start the service
docker-compose up -d

# Check logs
docker-compose logs -f app
```

## Testing

```bash
dotnet test
```

212 unit tests covering service logic and repository integrations.

## Performance Benchmarks

This project includes performance benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org/).

### Running Benchmarks

```bash
dotnet run -c Release --project benchmarks/binance-p2p-monitor.Benchmarks/
```

You can run all benchmarks by passing `*` as an argument:

```bash
dotnet run -c Release --project benchmarks/binance-p2p-monitor.Benchmarks/ -- "*"
```

## ConsoleOutputWriterExtensions

The `ConsoleOutputWriterExtensions` class provides a set of formatting and output utility methods for the `ConsoleOutputWriter`. These extensions facilitate consistent and visually organized console output, including success/error messages, section headers, key-value pairs, progress indicators, and separators.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

var writer = new ConsoleOutputWriter();

// Write formatted messages with context/codes
writer.WriteSuccessWithContext("Monitoring started", "Binance P2P");
writer.WriteErrorWithCode("Connection failed", "ERR001");
writer.WriteWarningWithSource("High latency detected", "WebSocketService");
writer.WriteInfoWithTimestamp("Checking prices...", DateTime.Now);

// Layout and formatting
writer.WriteSectionWithSubtitle("System Status", "All services running");
writer.WriteKeyValueHighlighted("Status", "Online", 15, true);
writer.WriteBlankLines(1);
writer.WriteSeparator("End of Report", '=');

// Progress tracking
writer.WriteProgress(50, 100, "Syncing data");
```

## PriceAlertExtensions

The `PriceAlertExtensions` class provides extension methods for the `PriceAlert` model to enhance alert management functionality. These extensions enable alert cloning, threshold updates, trigger tracking, age calculation, and note management for price monitoring scenarios.

### Usage

```csharp
using BinanceP2pMonitor.Models;

// Create a new price alert
var alert = new PriceAlert
{
    Asset = "BTC",
    Fiat = "USD",
    AlertType = AlertType.PriceIncrease,
    Threshold = 2.5m,
    Condition = "Price increased by 2.5% or more",
    IsEnabled = true,
    UserId = "user123",
    CreatedAt = DateTime.UtcNow,
    Notes = "Monitoring for significant price movements"
};

// Clone an alert for modification
var alertCopy = alert.Clone();

// Check if alert has triggered at least N times
if (alert.HasTriggeredAtLeast(3))
{
    Console.WriteLine("Alert has triggered 3+ times");
}

// Update the threshold value
if (alert.UpdateThreshold(3.0m))
{
    Console.WriteLine("Threshold updated successfully");
}

// Get the age of the alert in days
double ageInDays = alert.GetAgeInDays();
Console.WriteLine($"Alert age: {ageInDays:F2} days");

// Check if alert should fire based on current conditions
bool shouldFire = alert.ShouldFire(2.6m, 5);
Console.WriteLine($"Should fire: {shouldFire}");

// Append additional notes to the alert
alert.AppendNotes("Updated monitoring criteria on 2026-07-12");
```

## License

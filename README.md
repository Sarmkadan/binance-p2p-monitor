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

## License

MIT

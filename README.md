![CI](https://github.com/sarmkadan/binance-p2p-monitor/actions/workflows/ci.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/binance-p2p-monitor)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

# binance-p2p-monitor

CLI tool for monitoring Binance P2P prices, tracking spread anomalies, and sending Telegram alerts. Stores history in SQLite.

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

## Docker

```bash
docker build -t binance-p2p-monitor .
docker run -v $(pwd)/data:/app/data binance-p2p-monitor monitor
```

## Testing

```bash
dotnet test
```

212 unit tests covering service logic and repository integrations.

## License

MIT

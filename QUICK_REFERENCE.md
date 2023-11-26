# Quick Reference Guide

Fast lookup for common commands and configurations.

## Installation

```bash
# Clone
git clone https://github.com/Sarmkadan/binance-p2p-monitor.git
cd binance-p2p-monitor

# Build
dotnet build -c Release

# Configure
cp appsettings.json appsettings.local.json
# Edit with your Telegram bot token and API keys

# Run
dotnet run -- monitor --assets BTC,ETH --fiats USD,EUR
```

## Docker Quick Start

```bash
# Build image
docker build -t binance-p2p-monitor .

# Run container
docker run -d \
  -e AppSettings__TelegramBotToken="bot-token" \
  -e AppSettings__TelegramAdminChatId="chat-id" \
  -v $(pwd)/data:/data \
  binance-p2p-monitor:latest

# With docker-compose
docker-compose up -d
docker-compose logs -f
```

## Commands Quick Reference

| Command | Purpose | Example |
|---------|---------|---------|
| `monitor` | Start real-time monitoring | `dotnet run -- monitor --assets BTC --fiats USD` |
| `alert` | Manage price alerts | `dotnet run -- alert --create --asset BTC --type price_change --threshold 5.0` |
| `history` | Query price history | `dotnet run -- history --asset BTC --limit 100 --format json` |
| `export` | Export data to file | `dotnet run -- export --format csv --output prices.csv` |
| `status` | System health check | `dotnet run -- status` |
| `help` | Display help | `dotnet run -- help monitor` |

## Alert Types

| Type | Trigger | Example |
|------|---------|---------|
| `price_change` | Price moves X% | `--threshold 5.0 --lower` (drop 5%) |
| `spread_anomaly` | Spread > X% | `--threshold 2.0` (spread exceeds 2%) |
| `price_level` | Price crosses value | `--threshold 45000` (BTC at $45k) |

## Configuration Essentials

```json
{
  "AppSettings": {
    "TelegramBotToken": "123456:ABC...",      // From @BotFather
    "TelegramAdminChatId": "987654321",        // Your chat ID
    "MonitoringIntervalSeconds": 30,           // How often to check
    "AlertCooldownMinutes": 5,                 // Min between alerts
    "EnableWebSocket": true,                   // Use WebSocket (faster)
    "EnableTelegramNotifications": true,       // Send alerts
    "MonitoredAssets": ["BTC", "ETH"],         // What to monitor
    "MonitoredFiats": ["USD", "EUR"]           // Which currencies
  }
}
```

## Environment Variables

```bash
# Override config without editing file
export AppSettings__TelegramBotToken="your-token"
export AppSettings__TelegramAdminChatId="your-id"
export AppSettings__MonitoringIntervalSeconds="60"
dotnet run
```

## Alert Examples

### Example 1: BTC drops 5%
```bash
dotnet run -- alert --create \
  --asset BTC \
  --fiat USD \
  --type price_change \
  --threshold 5.0 \
  --user trader1 \
  --lower
```

### Example 2: ETH rises 3%
```bash
dotnet run -- alert --create \
  --asset ETH \
  --fiat USD \
  --type price_change \
  --threshold 3.0 \
  --user trader1 \
  --upper
```

### Example 3: High spread (>2%)
```bash
dotnet run -- alert --create \
  --asset BTC \
  --fiat USD \
  --type spread_anomaly \
  --threshold 2.0 \
  --user trader1
```

## Data Export

```bash
# CSV format (spreadsheet)
dotnet run -- export --format csv --output prices.csv

# JSON format (processing)
dotnet run -- export --format json --output prices.json --limit 5000

# Specific date range
dotnet run -- history --asset BTC --fiat USD \
  --from 2026-04-01 --to 2026-05-01 --format csv
```

## Build & Development

```bash
# Development build
dotnet build -c Debug

# Release build
dotnet build -c Release

# Run tests
dotnet test

# Format code
dotnet format

# Code quality check
dotnet format --verify-no-changes

# Publish self-contained
dotnet publish -c Release -r linux-x64 --self-contained -o publish/

# View help
dotnet run -- help
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Database is locked" | `rm binance_p2p.db-wal binance_p2p.db-shm` then restart |
| "Configuration validation failed" | Check `appsettings.json` syntax (JSON format) |
| "WebSocket timeout" | Increase interval or check internet connection |
| "Telegram not working" | Verify bot token: `curl api.telegram.org/botTOKEN/getMe` |
| High memory usage | Reduce `MaxHistoryRecords` in config |

## Performance Tuning

```json
{
  "AppSettings": {
    "MonitoringIntervalSeconds": 60,        // Increase for less load
    "MaxHistoryRecords": 50000,              // Decrease for less memory
    "HistoryRetentionDays": 7,               // Keep less history
    "AlertCooldownMinutes": 15,              // Reduce alert frequency
    "EnableAutoCleanup": true                // Auto-delete old records
  }
}
```

## Monitoring Commands

```bash
# Check system health
dotnet run -- status

# View recent prices
dotnet run -- history --asset BTC --fiat USD --limit 20

# List active alerts
dotnet run -- alert --list

# Check logs
tail -f logs/*.log

# Database info
sqlite3 binance_p2p.db "SELECT COUNT(*) FROM prices;"
```

## Deployment

```bash
# Systemd service (Linux)
sudo systemctl start binance-p2p-monitor
sudo systemctl status binance-p2p-monitor
sudo journalctl -u binance-p2p-monitor -f

# Docker container
docker ps
docker logs binance-monitor
docker stop binance-monitor

# Stop all
killall dotnet
```

## Backup & Restore

```bash
# Backup database
cp binance_p2p.db binance_p2p_$(date +%Y%m%d).db

# Restore database
cp binance_p2p_20260505.db binance_p2p.db

# Backup config
cp appsettings.json appsettings_$(date +%Y%m%d).json
```

## Linux Systemd Setup

```bash
# Create service
sudo tee /etc/systemd/system/binance-p2p-monitor.service > /dev/null <<EOF
[Unit]
Description=Binance P2P Monitor
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/binance-p2p-monitor
ExecStart=/opt/binance-p2p-monitor/binance-p2p-monitor monitor
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Enable & start
sudo systemctl daemon-reload
sudo systemctl enable binance-p2p-monitor
sudo systemctl start binance-p2p-monitor

# Check status
sudo systemctl status binance-p2p-monitor
```

## Common Use Cases

### Trader Setup
```bash
# Monitor main pairs every minute
dotnet run -- monitor \
  --assets BTC,ETH,BNB \
  --fiats USD,EUR \
  --interval 60

# Create alerts for trading signals
dotnet run -- alert --create --asset BTC --fiat USD \
  --type price_change --threshold 2.0 --user trader1 --lower
```

### Analyst Setup
```bash
# Detailed monitoring with spread analysis
dotnet run -- monitor \
  --assets BTC,ETH \
  --fiats USD,EUR,GBP \
  --interval 30 \
  --include-spread \
  --output json

# Export for analysis
dotnet run -- history --asset BTC --fiat USD \
  --limit 10000 --format json
```

### Production Setup
```bash
# Background monitoring via systemd
# Edit /etc/systemd/system/binance-p2p-monitor.service
# Set: ExecStart=/opt/binance-p2p-monitor/binance-p2p-monitor monitor

# Daily backup
0 2 * * * cp binance_p2p.db /backups/binance_p2p_$(date +\%Y\%m\%d).db

# Database cleanup
0 3 * * 0 sqlite3 binance_p2p.db "DELETE FROM prices WHERE \
  datetime(timestamp) < datetime('now', '-30 days');"
```

## Support

| Topic | Resource |
|-------|----------|
| Installation Issues | [docs/getting-started.md](docs/getting-started.md) |
| API Usage | [docs/api-reference.md](docs/api-reference.md) |
| Architecture | [docs/architecture.md](docs/architecture.md) |
| Deployment | [docs/deployment.md](docs/deployment.md) |
| FAQ | [docs/faq.md](docs/faq.md) |
| Examples | [examples/](examples/) |
| Contributing | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Bugs | [GitHub Issues](https://github.com/Sarmkadan/binance-p2p-monitor/issues) |

## Version Info

```bash
# Check installed version
dotnet run -- version

# Check .NET version
dotnet --version

# Check Git status
git status
git log --oneline -5
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Ctrl+C` | Stop monitoring |
| `Ctrl+D` | Exit CLI (Linux/macOS) |
| `↑/↓` | Command history |
| `Tab` | Command completion |

## Key Files

| File | Purpose |
|------|---------|
| `appsettings.json` | Configuration defaults |
| `binance_p2p.db` | SQLite database |
| `logs/` | Application logs |
| `Dockerfile` | Docker image |
| `.github/workflows/build.yml` | CI/CD |
| `Makefile` | Build commands |
| `src/Program.cs` | Application entry point |

## Performance Metrics

| Metric | Typical Value | Notes |
|--------|---------------|-------|
| CPU usage | <5% | Single core, idle monitoring |
| Memory | 80-150MB | Baseline + cache |
| API latency | 150-200ms | REST API |
| WebSocket latency | 20-50ms | Real-time |
| Database size | ~10MB/day | Per 10k prices/day |

## Links

- 🌐 [Project Website](https://github.com/Sarmkadan/binance-p2p-monitor)
- 💼 [Author Portfolio](https://sarmkadan.com)
- 📧 [Contact](https://sarmkadan.com)
- 💬 [Telegram](https://t.me/sarmkadan)

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

# Getting Started with binance-p2p-monitor

This guide walks you through installation, initial configuration, and your first monitoring session.

## Prerequisites

- **Operating System:** Windows, macOS, or Linux
- **.NET 10 SDK:** Download from [dotnet.microsoft.com](https://dotnet.microsoft.com)
  - Verify: `dotnet --version` should show 10.0 or later
- **Optional:** Telegram Bot (for alerts)
  - Create via [@BotFather](https://t.me/botfather) on Telegram
  - Note: Bot Token and Chat ID

## Step 1: Clone & Build

```bash
# Clone repository
git clone https://github.com/Sarmkadan/binance-p2p-monitor.git
cd binance-p2p-monitor

# Build in Release mode
dotnet build -c Release

# Verify build
dotnet run -- --version
# Output: binance-p2p-monitor v1.2.0
```

## Step 2: Configure Credentials

### Basic Configuration (No Alerts)

```bash
# Create local config (overwrites appsettings.json)
cp appsettings.json appsettings.local.json
```

Minimal `appsettings.local.json`:

```json
{
  "AppSettings": {
    "DatabaseConnectionString": "Data Source=binance_p2p.db;Version=3;",
    "MonitoringIntervalSeconds": 30,
    "EnableWebSocket": true,
    "EnableTelegramNotifications": false,
    "MonitoredAssets": ["BTC", "ETH"],
    "MonitoredFiats": ["USD", "EUR"]
  }
}
```

### With Telegram Alerts

1. **Create Telegram Bot:**
   - Message [@BotFather](https://t.me/botfather)
   - `/newbot` → name your bot → save token

2. **Get your Chat ID:**
   - Start bot: `/start`
   - Visit: `https://api.telegram.org/botYOUR_TOKEN/getUpdates`
   - Find `"id"` in response

3. **Update config:**

```json
{
  "AppSettings": {
    "TelegramBotToken": "123456789:ABCDefGhIjKlMnOpQrStUvWxYz",
    "TelegramAdminChatId": "987654321",
    "EnableTelegramNotifications": true,
    "AlertCooldownMinutes": 5,
    "DefaultPriceChangeThreshold": 2.0
  }
}
```

## Step 3: First Run

### Test the Connection

```bash
# Check status (verify database and services)
dotnet run -- status

# Expected output:
# ✓ Database connected
# ✓ WebSocket ready
# ✓ Telegram notifications enabled
```

### Start Monitoring

```bash
# Monitor BTC and ETH against USD and EUR
dotnet run -- monitor --assets BTC,ETH --fiats USD,EUR

# Output (every 30 seconds):
# [10:45:23] BTC/USD: bid=45234.50 ask=45250.00
# [10:45:23] BTC/EUR: bid=41200.30 ask=41220.50
# [10:45:23] ETH/USD: bid=2850.75 ask=2860.25
# [10:45:23] ETH/EUR: bid=2600.40 ask=2610.80
```

Press `Ctrl+C` to stop.

## Step 4: Create Your First Alert

### Price Change Alert (BTC drops 5%)

```bash
dotnet run -- alert \
  --create \
  --asset BTC \
  --fiat USD \
  --type price_change \
  --threshold 5.0 \
  --user "my-trader-id" \
  --lower
```

This creates an alert that fires when BTC/USD drops 5% or more from the baseline.

### View Alert Details

```bash
dotnet run -- alert --list

# Output:
# ID | Asset | Fiat | Type | Threshold | Status
# 1  | BTC   | USD  | price_change | 5.0% | ACTIVE
```

### Remove Alert

```bash
dotnet run -- alert --remove 1
```

## Step 5: Background Monitoring

### Run as Daemon (Linux/macOS)

```bash
# Create service file
sudo tee /etc/systemd/system/binance-p2p-monitor.service > /dev/null <<EOF
[Unit]
Description=Binance P2P Monitor
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/binance-p2p-monitor
ExecStart=/usr/bin/dotnet run --no-build -c Release
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

# Start service
sudo systemctl enable binance-p2p-monitor
sudo systemctl start binance-p2p-monitor
sudo systemctl status binance-p2p-monitor
```

### Run in Docker

```bash
# Build image
docker build -t binance-p2p-monitor:latest .

# Run container
docker run -d \
  --name binance-monitor \
  -e AppSettings__TelegramBotToken="your-token" \
  -e AppSettings__TelegramAdminChatId="your-id" \
  -v $(pwd)/data:/app/data \
  binance-p2p-monitor:latest
```

### Monitor Logs

```bash
# View last 100 lines
docker logs -n 100 binance-monitor

# Follow live logs
docker logs -f binance-monitor
```

## Step 6: Data Export

### Export Price History

```bash
# Export last 1000 prices as CSV
dotnet run -- export \
  --format csv \
  --output prices.csv \
  --limit 1000

# Check file
head -5 prices.csv
# asset,fiat,bid,ask,timestamp
# BTC,USD,45234.50,45250.00,2026-05-04T10:45:23Z
```

### Export Alerts

```bash
dotnet run -- export \
  --format json \
  --output alerts.json \
  --entity alerts
```

## Monitoring Checklist

Before running in production, verify:

- [ ] Configuration file created and tested
- [ ] Database location is writable
- [ ] Telegram bot token is valid (if using alerts)
- [ ] API rate limits are acceptable for your interval
- [ ] Logs directory exists: `mkdir -p logs`
- [ ] Backup important data: `cp binance_p2p.db binance_p2p.db.backup`
- [ ] Test alert triggering with manual threshold
- [ ] Monitor resource usage for 1 hour

## Troubleshooting

### "Configuration validation failed"

**Cause:** Missing or invalid appsettings.json

**Fix:**
```bash
# Restore defaults
cp appsettings.json appsettings.backup.json
git checkout appsettings.json

# Then customize
cp appsettings.json appsettings.local.json
```

### "Database is locked"

**Cause:** Multiple instances accessing same database

**Fix:**
```bash
# Check processes
pgrep -a "binance-p2p-monitor"

# Kill other instances
killall dotnet

# Remove lock files
rm -f binance_p2p.db-wal binance_p2p.db-shm

# Restart
dotnet run
```

### "WebSocket connection timeout"

**Cause:** Network connectivity or Binance API unavailable

**Fix:**
1. Check internet: `ping 8.8.8.8`
2. Check Binance status: https://www.binance.us/en/support/announcement/official
3. Increase timeout in config: `"DatabaseCommandTimeoutSeconds": 60`
4. Try with polling mode: `"EnableWebSocket": false`

### "Telegram bot not responding"

**Cause:** Invalid token or chat ID

**Fix:**
```bash
# Test bot token
curl https://api.telegram.org/bot YOUR_TOKEN/getMe

# Get your chat ID
# Message bot, then:
curl https://api.telegram.org/botYOUR_TOKEN/getUpdates | jq '.result[0].message.chat.id'
```

## Next Steps

1. **Configure Multiple Assets:** Update `MonitoredAssets` in config
2. **Set Up Alerts:** Create price change and spread alerts for your pairs
3. **Schedule Exports:** Set up cron job for daily price exports
4. **Monitor Performance:** Check `status` command regularly
5. **Review Logs:** Archive logs weekly: `gzip logs/*.log`

## Resources

- [Full Configuration Reference](./configuration.md)
- [API Reference](./api-reference.md)
- [Deployment Guide](./deployment.md)
- [Architecture Overview](./architecture.md)
- [FAQ](./faq.md)

For issues, open a GitHub issue with:
- Error message (from logs)
- Configuration (sanitized)
- Steps to reproduce
- Your OS and .NET version

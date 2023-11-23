# Frequently Asked Questions

## Installation & Setup

### Q: Do I need a Binance API key to use this?

**A:** No, the monitor works with public P2P data. API key/secret are optional and only needed for authenticated endpoints (future feature). However, Telegram notifications require a bot token.

### Q: Can I run multiple instances of the monitor?

**A:** Yes, but each instance needs its own SQLite database file. Set different `DatabaseConnectionString` for each:

```json
{
  "AppSettings": {
    "DatabaseConnectionString": "Data Source=binance_p2p_instance1.db;"
  }
}
```

### Q: What are the minimum hardware requirements?

**A:** 
- CPU: 1 core (x86/ARM)
- RAM: 256MB
- Disk: 500MB free space
- Network: 1 Mbps stable connection

## Configuration

### Q: How often should I monitor prices?

**A:** Recommended intervals:
- **Day trading**: 10-30 seconds (more API calls, more accurate)
- **Swing trading**: 1-5 minutes (balanced)
- **Long-term holding**: 1-24 hours (minimal load)

Consider Binance rate limits (100 req/min per IP).

### Q: What's the difference between price_change and spread_anomaly alerts?

**A:** 
- **price_change**: Triggers when crypto price moves by X% (e.g., BTC drops 5%)
- **spread_anomaly**: Triggers when bid/ask spread exceeds X% (unusual market condition)

Use price_change for trading signals, spread_anomaly for market health monitoring.

### Q: Can I monitor multiple fiats for the same asset?

**A:** Yes, configure in `MonitoredFiats`:

```json
{
  "MonitoredFiats": ["USD", "EUR", "GBP", "CNY", "JPY"]
}
```

Each fiat gets separate price monitoring and alerts.

## Alerts & Notifications

### Q: How do I prevent alert fatigue?

**A:** Use `AlertCooldownMinutes`:

```json
{
  "AppSettings": {
    "AlertCooldownMinutes": 15  // Minimum 15 min between repeat alerts
  }
}
```

Also set reasonable thresholds (don't alert on every 0.1% move).

### Q: Can I set different thresholds for different times of day?

**A:** Not yet in the GUI, but you can:
1. Create multiple alert instances with different cooldowns
2. Manually pause/resume alerts via CLI
3. Use the API to dynamically update thresholds

### Q: How do I test if my Telegram bot is working?

**A:** 

```bash
# Get bot info
curl https://api.telegram.org/botYOUR_TOKEN/getMe

# Manually send test message
curl -X POST https://api.telegram.org/botYOUR_TOKEN/sendMessage \
  -d chat_id=YOUR_CHAT_ID \
  -d text="Test message"
```

### Q: Can I send alerts to multiple Telegram chats?

**A:** Currently supports one chat ID. Workaround:
1. Create a Telegram group
2. Add your bot to the group
3. Get group chat ID: Use bot `/start` command, check `getUpdates` response
4. Use group ID in `TelegramAdminChatId`

## Performance & Troubleshooting

### Q: Why is the monitor using so much memory?

**A:** Likely causes:
1. Too many historical records in memory cache
2. Large alert history
3. Memory leak in WebSocket

**Solutions:**
```json
{
  "AppSettings": {
    "MaxHistoryRecords": 50000,        // Reduce from 100k
    "HistoryRetentionDays": 7,         // Delete older records
    "CacheTTLSeconds": 300              // Longer cache timeout
  }
}
```

### Q: Database is locked - how do I fix it?

**A:** The `.db-wal` and `.db-shm` files indicate active connections. If stuck:

```bash
# Stop the monitor
systemctl stop binance-p2p-monitor

# Remove lock files
rm binance_p2p.db-wal binance_p2p.db-shm

# Restart
systemctl start binance-p2p-monitor
```

### Q: Price updates suddenly stopped - why?

**A:** Common causes:
1. WebSocket disconnected, waiting for reconnection (logs will show)
2. Binance API rate limit hit (check cooldown timer)
3. Network connectivity issue (check `ping 8.8.8.8`)
4. Application crashed (check `status` command and logs)

**Recovery:**
```bash
dotnet run -- status
sudo journalctl -u binance-p2p-monitor -n 100
systemctl restart binance-p2p-monitor
```

### Q: How do I reduce database size?

**A:** SQLite databases can grow large:

```bash
# Delete old records
sqlite3 binance_p2p.db "DELETE FROM prices WHERE timestamp < datetime('now', '-30 days');"

# Shrink database file
sqlite3 binance_p2p.db "VACUUM;"

# Check size
ls -lh binance_p2p.db
```

### Q: WebSocket connection keeps failing - what's wrong?

**A:** Troubleshoot with these steps:

```bash
# 1. Check internet
ping 8.8.8.8

# 2. Check Binance API availability
curl -s https://www.binance.us/en/support/announcement/official | grep -i "status\|maintenance"

# 3. Check logs
grep -i "websocket\|connection" logs/*.log

# 4. Try polling mode (disable WebSocket)
```

In config:
```json
{
  "AppSettings": {
    "EnableWebSocket": false  // Fall back to REST API polling
  }
}
```

## Data & Exports

### Q: Can I export my entire price history?

**A:** Yes:

```bash
dotnet run -- export \
  --format csv \
  --output full_history.csv \
  --limit 1000000  # No limit
```

Large exports may take time. Use `--limit` to export in batches.

### Q: How do I analyze price data in Excel or R?

**A:** Export as CSV:

```bash
dotnet run -- export --format csv --output prices.csv
# Open in Excel: Data → From Text/CSV → prices.csv
```

For R:
```r
prices <- read.csv("prices.csv")
plot(prices$timestamp, prices$bid, type="l", main="BTC/USD")
```

### Q: Can I export alert history?

**A:** Yes, with metadata:

```bash
dotnet run -- export \
  --format json \
  --output alerts_history.json \
  --entity alerts \
  --include-alerts
```

## Advanced Usage

### Q: Can I use this with a proxy?

**A:** Not yet built-in, but you can:

1. Use environment variable for HTTP_PROXY (Windows/Linux)
2. Run in Docker with proxy settings
3. Use a local VPN client

### Q: Can I monitor Binance Futures P2P prices?

**A:** Currently P2P spot only. Futures support would require different API endpoints.

### Q: How do I set up alerts for multiple assets efficiently?

**A:** Use a script to create bulk alerts:

```bash
#!/bin/bash
for asset in BTC ETH BNB XRP ADA; do
  for fiat in USD EUR GBP; do
    dotnet run -- alert --create \
      --asset $asset \
      --fiat $fiat \
      --type price_change \
      --threshold 2.0 \
      --user trader1
  done
done
```

### Q: Can I trigger custom actions on alerts?

**A:** Not directly, but you can:
1. Export alert history periodically
2. Parse JSON to trigger webhooks
3. Integrate with IFTTT or automation platform

## Development & Contribution

### Q: Can I modify the source code for my own use?

**A:** Yes! It's open source (MIT license). You can:
1. Fork the repository
2. Modify the code
3. Deploy your own version

See Contributing section in README.

### Q: How do I add support for a new exchange?

**A:** The architecture is extensible:

1. Create new interface `INewExchangeMonitoringService`
2. Implement service with API/WebSocket logic
3. Register in `Program.cs`
4. Create command handler

Start with a copy of `BinanceP2pMonitoringService`.

### Q: Can I add support for new notification channels?

**A:** Yes! Create implementation of `INotificationClient`:

```csharp
public class SlackNotificationClient : INotificationClient
{
    public async Task SendAlertAsync(PriceAlert alert, Price price)
    {
        // Send to Slack webhook
    }
}
```

Register in DI container and use in AlertService.

### Q: How do I run unit tests?

**A:** 

```bash
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Specific test class
dotnet test --filter "ClassName=AlertServiceTests"
```

## Licensing & Support

### Q: What license is this project under?

**A:** MIT License. You can use, modify, and distribute freely. See LICENSE file.

### Q: How do I report a bug?

**A:** 

1. Check [GitHub Issues](https://github.com/Sarmkadan/binance-p2p-monitor/issues)
2. Open new issue with:
   - Error message from logs
   - Steps to reproduce
   - Your OS and .NET version
   - Sanitized configuration (remove secrets)

### Q: Is there commercial support available?

**A:** The project is maintained as open source. For:
- Custom features
- Dedicated support
- Consulting

Contact: vladyslav.zaiets@amdaris.com

### Q: How often is the project updated?

**A:** 
- Bug fixes: Within 1-2 weeks
- Feature requests: Considered quarterly
- Security updates: ASAP
- Dependencies: Monthly

Follow releases on [GitHub](https://github.com/Sarmkadan/binance-p2p-monitor).

## Specific Use Cases

### Q: Can I use this for automated trading?

**A:** The monitor provides real-time prices and alerts. Automated trading requires:

1. Trade execution API (not in monitor)
2. Risk management logic
3. Order placement on exchange

You could use monitor as price source for a bot built separately.

### Q: Can I monitor P2P prices on other exchanges?

**A:** Currently Binance only. To add support:

1. Study target exchange's API
2. Create new monitoring service
3. Integrate with event bus
4. Create CLI commands

### Q: How do I use this in my trading bot?

**A:** Use the service interfaces directly:

```csharp
// In your bot
var priceService = host.Services.GetRequiredService<IPriceMonitoringService>();
var alertService = host.Services.GetRequiredService<IAlertService>();

// Subscribe to alerts
eventBus.Subscribe<AlertTriggeredEvent>(async @event =>
{
    // Execute trade
    await PlaceOrderAsync(@event.Alert);
});
```

See `examples/` directory for working code.

### Q: Can I use this for arbitrage detection?

**A:** Yes! Compare P2P prices:

```csharp
var btcUsd = await priceService.GetPriceAsync("BTC", "USD");
var btcEur = await priceService.GetPriceAsync("BTC", "EUR");

// Calculate spread and fees
var arbitrage = CalculateArbitrage(btcUsd, btcEur, usdEurRate);
```

## Still Have Questions?

- 📖 Check [README.md](../README.md) for overview
- 🏗️ Read [Architecture](./architecture.md) for design details
- 🚀 See [Deployment](./deployment.md) for production setup
- 📚 Check [API Reference](./api-reference.md) for code examples
- 💬 Ask on [GitHub Discussions](https://github.com/Sarmkadan/binance-p2p-monitor/discussions)

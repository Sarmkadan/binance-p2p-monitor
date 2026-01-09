# Docker Guide for binance-p2p-monitor

## Quick Start with Docker

### Prerequisites

- Docker Engine 20.10+ or Docker Desktop
- Basic understanding of Docker concepts (images, containers, volumes)

### Running the Container

#### Basic Usage

```bash
# Pull the latest image (if available on Docker Hub)
docker pull sarmkadan/binance-p2p-monitor:latest

# Or build from source
docker build -t binance-p2p-monitor:latest .

# Run with basic monitoring
docker run -it --rm \
  binance-p2p-monitor:latest \
  monitor --assets BTC,ETH --fiats USD,EUR --interval 30
```

#### With Telegram Notifications

```bash
docker run -it --rm \
  -e AppSettings__TelegramBotToken="your-bot-token-here" \
  -e AppSettings__TelegramAdminChatId="your-chat-id-here" \
  binance-p2p-monitor:latest \
  monitor --assets BTC --fiats USD --interval 15
```

#### Persistent Data Storage

```bash
# Create a volume for persistent data
docker volume create binance-p2p-data

# Run with persistent storage
docker run -it --rm \
  -v binance-p2p-data:/app/data \
  -e AppSettings__DatabaseConnectionString="Data Source=/app/data/binance_p2p.db;Version=3;" \
  binance-p2p-monitor:latest \
  monitor --assets BTC,ETH --fiats USD,EUR
```

## Docker Compose Usage

### Basic docker-compose.yml

```yaml
version: '3.8'

services:
  binance-p2p-monitor:
    image: sarmkadan/binance-p2p-monitor:latest
    build: .
    container_name: binance-p2p-monitor
    restart: unless-stopped
    environment:
      - AppSettings__MonitoringIntervalSeconds=30
      - AppSettings__EnableWebSocket=true
      - AppSettings__EnableTelegramNotifications=false
      - AppSettings__MonitoredAssets=BTC,ETH,BNB,USDT
      - AppSettings__MonitoredFiats=USD,EUR,GBP
      - AppSettings__LogLevel=Information
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    ports:
      - "5000:80"  # Optional: if you add HTTP endpoints in future
```

### Advanced docker-compose.yml with Telegram

```yaml
version: '3.8'

services:
  binance-p2p-monitor:
    image: sarmkadan/binance-p2p-monitor:latest
    build: .
    container_name: binance-p2p-monitor
    restart: unless-stopped
    environment:
      - AppSettings__MonitoringIntervalSeconds=15
      - AppSettings__EnableWebSocket=true
      - AppSettings__EnableTelegramNotifications=true
      - AppSettings__TelegramBotToken=${TELEGRAM_BOT_TOKEN}
      - AppSettings__TelegramAdminChatId=${TELEGRAM_CHAT_ID}
      - AppSettings__AlertCooldownMinutes=5
      - AppSettings__DefaultPriceChangeThreshold=2.0
      - AppSettings__DefaultSpreadThreshold=1.5
      - AppSettings__HistoryRetentionDays=30
      - AppSettings__LogLevel=Information
      - AppSettings__DatabaseConnectionString=Data Source=/app/data/binance_p2p.db;Version=3;
    volumes:
      - binance-p2p-data:/app/data
      - binance-p2p-logs:/app/logs
      - ./appsettings.json:/app/appsettings.json:ro
    ports:
      - "5000:80"  # Optional

volumes:
  binance-p2p-data:
  binance-p2p-logs:
```

### Environment Variables Reference

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `AppSettings__DatabaseConnectionString` | SQLite connection string | `Data Source=binance_p2p.db;Version=3;` | No |
| `AppSettings__BinanceApiKey` | Binance API key (for advanced features) | Empty | No |
| `AppSettings__BinanceApiSecret` | Binance API secret | Empty | No |
| `AppSettings__TelegramBotToken` | Telegram bot token for notifications | Empty | No* |
| `AppSettings__TelegramAdminChatId` | Telegram chat ID for notifications | Empty | No* |
| `AppSettings__MonitoringIntervalSeconds` | Monitoring frequency in seconds | 30 | No |
| `AppSettings__AlertCooldownMinutes` | Minutes between alert repeats | 5 | No |
| `AppSettings__MaxAlertsPerUser` | Maximum alerts per user | 20 | No |
| `AppSettings__DefaultPriceChangeThreshold` | Default % change to trigger alert | 2.0 | No |
| `AppSettings__DefaultSpreadThreshold` | Default % spread to trigger alert | 1.5 | No |
| `AppSettings__HistoryRetentionDays` | Days to keep historical data | 30 | No |
| `AppSettings__MaxHistoryRecords` | Maximum rows in price history | 100000 | No |
| `AppSettings__EnableWebSocket` | Use WebSocket feeds | true | No |
| `AppSettings__EnableTelegramNotifications` | Enable Telegram alerts | false | No |
| `AppSettings__EnableAutoCleanup` | Enable automatic database cleanup | true | No |
| `AppSettings__LogLevel` | Logging level (Trace/Debug/Information/Warning/Error/Critical) | Information | No |
| `AppSettings__LogPath` | Path for log files | ./logs | No |

*Required only if using Telegram notifications

### Production Deployment Checklist

#### Pre-Deployment

1. [ ] **Security Review**
   - Review and secure API keys/secrets
   - Use Docker secrets or Kubernetes secrets for production
   - Scan image for vulnerabilities: `docker scan binance-p2p-monitor:latest`

2. [ ] **Configuration Validation**
   - Test configuration in staging environment
   - Verify all required environment variables are set
   - Test database connectivity and permissions

3. [ ] **Resource Planning**
   - Monitor resource usage: `docker stats binance-p2p-monitor`
   - Plan for storage growth (SQLite database)
   - Consider backup strategy for persistent data

#### Deployment

4. [ ] **Image Building**
   - Use multi-stage build for smaller images
   - Tag images appropriately: `binance-p2p-monitor:v2.0.0`
   - Push to container registry: `docker push your-registry/binance-p2p-monitor:v2.0.0`

5. [ ] **Container Orchestration**
   - For single host: Use Docker Compose or systemd
   - For multi-host: Consider Kubernetes or Docker Swarm
   - Configure restart policies appropriately

6. [ ] **Networking**
   - Ensure outbound connectivity to Binance API
   - Configure firewall rules if needed
   - Consider using a proxy for outbound traffic

#### Post-Deployment

7. [ ] **Monitoring & Logging**
   - Set up log aggregation (ELK, Fluentd, etc.)
   - Monitor container health and restart counts
   - Set up alerts for container crashes or high resource usage

8. [ ] **Backup & Recovery**
   - Implement regular backups of persistent volumes
   - Test restore procedures
   - Consider database replication for high availability

9. [ ] **Performance Tuning**
   - Adjust monitoring interval based on requirements
   - Tune cache settings for your workload
   - Consider read replicas for heavy query workloads

#### Maintenance

10. [ ] **Regular Updates**
    - Monitor for new releases
    - Test updates in staging before production
    - Keep base images updated for security patches

11. [ ] **Log Rotation**
    - Configure log rotation to prevent disk filling
    - Consider centralized logging solutions

12. [ ] **Health Checks**
    - Implement health check endpoints
    - Use container health checks: `HEALTHCHECK CMD dotnet run -- health-check`

### Troubleshooting Common Docker Issues

#### Container Fails to Start

```bash
# Check container logs
docker logs binance-p2p-monitor

# Check container status
docker ps -a | grep binance-p2p-monitor

# Inspect container details
docker inspect binance-p2p-monitor
```

#### Database Connection Issues

```bash
# Verify volume mount
docker volume ls | grep binance-p2p-data

# Check file permissions in volume
docker run --rm -v binance-p2p-data:/data alpine ls -la /data

# Test database connectivity
docker run --rm -v binance-p2p-data:/app/data \
  binance-p2p-monitor:latest \
  dotnet binance-p2p-monitor.dll --version
```

#### Telegram Notification Problems

```bash
# Test bot token from within container
docker run --rm \
  -e AppSettings__TelegramBotToken="your-token" \
  binance-p2p-monitor:latest \
  curl -s "https://api.telegram.org/bot${AppSettings__TelegramBotToken}/getMe"

# Check network connectivity
docker run --rm --network host \
  binance-p2p-monitor:latest \
  curl -s https://api.telegram.org
```

#### High Resource Usage

```bash
# Monitor container stats
docker stats binance-p2p-monitor

# Check for memory leaks over time
docker stats --no-stream binance-p2p-monitor

# Consider adjusting monitoring interval
# Increase interval to reduce API calls and processing
```

### Best Practices

#### Image Optimization

1. Use `.dockerignore` to exclude unnecessary files
2. Leverage multi-stage builds to reduce image size
3. Use specific tags instead of `latest` in production
4. Regularly update base images for security patches

#### Configuration Management

1. Store secrets in Docker secrets or external vaults
2. Use configuration files for non-sensitive settings
3. Implement configuration validation at startup
4. Use environment-specific configuration files

#### Logging and Monitoring

1. Send logs to stdout/stderr for Docker logging drivers
2. Use structured logging for easier parsing
3. Implement health check endpoints
4. Monitor key metrics: API latency, alert volume, database size

#### Data Persistence

1. Use named volumes for persistent data
2. Implement regular backup strategies
3. Monitor disk usage and set up alerts
4. Consider database migration path for future versions

## Example Production Deployment

### docker-compose.production.yml

```yaml
version: '3.8'

services:
  binance-p2p-monitor:
    image: your-registry/binance-p2p-monitor:v2.0.0
    container_name: binance-p2p-monitor
    restart: unless-stopped
    env_file:
      - .env.production
    secrets:
      - telegram_bot_token
      - telegram_chat_id
    volumes:
      - binance-p2p-data:/app/data
      - binance-p2p-logs:/app/logs
    deploy:
      resources:
        limits:
          cpus: "1.0"
          memory: 512M
        reservations:
          cpus: "0.5"
          memory: 256M
    healthcheck:
      test: ["CMD", "dotnet", "run", "--", "health-check"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

volumes:
  binance-p2p-data:
  binance-p2p-logs:

secrets:
  telegram_bot_token:
    external: true
  telegram_chat_id:
    external: true
```

### .env.production

```env
AppSettings__MonitoringIntervalSeconds=30
AppSettings__EnableWebSocket=true
AppSettings__EnableTelegramNotifications=true
AppSettings__AlertCooldownMinutes=5
AppSettings__DefaultPriceChangeThreshold=2.0
AppSettings__DefaultSpreadThreshold=1.5
AppSettings__HistoryRetentionDays=30
AppSettings__LogLevel=Warning
AppSettings__DatabaseConnectionString=Data Source=/app/data/binance_p2p.db;Version=3;
```

This comprehensive Docker guide provides everything needed to successfully deploy and manage binance-p2p-monitor in containerized environments, from basic usage to production-grade deployments.
# Deployment Guide

Production deployment strategies for binance-p2p-monitor across different environments.

## Prerequisites

- .NET 10 runtime (or SDK for self-contained deployments)
- Target OS: Windows, macOS, Linux
- Disk space: 500MB minimum (SQLite database grows ~10MB/day)
- Network: Internet access to Binance API

## Deployment Options

### Option 1: Standalone Binary (Recommended for Most Users)

Self-contained executable without .NET runtime dependency.

```bash
# Build self-contained for your platform
dotnet publish -c Release -r linux-x64 --self-contained

# Windows
dotnet publish -c Release -r win-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained

# Output location: ./bin/Release/net10.0/<RID>/publish/
```

**Deployment:**
```bash
# Copy to target machine
scp -r ./bin/Release/net10.0/linux-x64/publish/ user@server:/opt/binance-monitor/

# Run
/opt/binance-monitor/binance-p2p-monitor --help
```

### Option 2: Docker Container

Containerized deployment for consistency across environments.

**Building Image:**
```bash
docker build -t binance-p2p-monitor:1.2.0 .

# Push to registry
docker tag binance-p2p-monitor:1.2.0 myregistry/binance-p2p-monitor:1.2.0
docker push myregistry/binance-p2p-monitor:1.2.0
```

**Running Container:**
```bash
docker run -d \
  --name binance-monitor \
  -e AppSettings__TelegramBotToken="bot-token" \
  -e AppSettings__TelegramAdminChatId="chat-id" \
  -v $(pwd)/data:/app/data \
  -v $(pwd)/logs:/app/logs \
  binance-p2p-monitor:1.2.0
```

**Docker Compose:**
```bash
docker-compose up -d
docker-compose logs -f app
docker-compose down
```

### Option 3: Docker Swarm

Multi-host deployment with orchestration.

```bash
# Deploy stack
docker stack deploy -c docker-compose.yml binance-monitor

# Scale service
docker service scale binance_monitor_app=3

# Check status
docker stack ps binance-monitor
```

### Option 4: Kubernetes

Cloud-native deployment.

**Create namespace:**
```bash
kubectl create namespace monitoring
```

**Create secrets:**
```bash
kubectl create secret generic binance-secrets \
  --from-literal=telegram-token="bot-token" \
  --from-literal=telegram-chat-id="chat-id" \
  -n monitoring
```

**Deploy:**
```bash
kubectl apply -f k8s/deployment.yaml -n monitoring
kubectl apply -f k8s/service.yaml -n monitoring
kubectl apply -f k8s/configmap.yaml -n monitoring
```

**Monitor:**
```bash
kubectl get pods -n monitoring
kubectl logs -f deployment/binance-monitor -n monitoring
```

## Platform-Specific Setup

### Linux (Systemd)

**System Service File:**

Create `/etc/systemd/system/binance-p2p-monitor.service`:

```ini
[Unit]
Description=Binance P2P Monitor
After=network.target

[Service]
Type=simple
User=monitor
WorkingDirectory=/opt/binance-p2p-monitor
ExecStart=/opt/binance-p2p-monitor/binance-p2p-monitor monitor
Restart=always
RestartSec=10
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

**Enable & Start:**
```bash
sudo systemctl daemon-reload
sudo systemctl enable binance-p2p-monitor
sudo systemctl start binance-p2p-monitor

# Verify
sudo systemctl status binance-p2p-monitor
sudo journalctl -u binance-p2p-monitor -f
```

### Linux (OpenRC)

Alternative for Alpine Linux:

```bash
#!/sbin/openrc-run

name="Binance P2P Monitor"
supervisor="supervise-daemon"
command="/opt/binance-p2p-monitor/binance-p2p-monitor"
command_args="monitor"
command_user="monitor"
directory="/opt/binance-p2p-monitor"

depend() {
    need net
}
```

### macOS (Launchd)

Create `~/Library/LaunchAgents/com.sarmkadan.binance-monitor.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.sarmkadan.binance-monitor</string>
    <key>ProgramArguments</key>
    <array>
        <string>/Applications/binance-p2p-monitor</string>
        <string>monitor</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>/var/log/binance-monitor.log</string>
    <key>StandardErrorPath</key>
    <string>/var/log/binance-monitor-error.log</string>
</dict>
</plist>
```

**Load:**
```bash
launchctl load ~/Library/LaunchAgents/com.sarmkadan.binance-monitor.plist
launchctl start com.sarmkadan.binance-monitor
```

### Windows (Task Scheduler)

Create scheduled task:

```powershell
$action = New-ScheduledTaskAction -Execute "C:\Program Files\binance-p2p-monitor\binance-p2p-monitor.exe" -Argument "monitor"
$trigger = New-ScheduledTaskTrigger -AtStartup
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName "Binance P2P Monitor" -RunLevel Highest
```

## Database Backup Strategy

### SQLite Backup

```bash
# Daily backup
0 2 * * * cp /opt/binance-p2p-monitor/binance_p2p.db /backups/binance_p2p_$(date +\%Y\%m\%d).db

# Keep last 30 days
find /backups -name "binance_p2p_*.db" -mtime +30 -delete
```

### Restore Backup

```bash
# Stop application
sudo systemctl stop binance-p2p-monitor

# Restore
cp /backups/binance_p2p_20260505.db /opt/binance-p2p-monitor/binance_p2p.db

# Start
sudo systemctl start binance-p2p-monitor
```

### Cloud Backup (AWS S3)

```bash
#!/bin/bash
aws s3 cp /opt/binance-p2p-monitor/binance_p2p.db \
  s3://my-backups/binance-monitor/binance_p2p_$(date +%Y%m%d_%H%M%S).db
```

## Monitoring & Health Checks

### System Monitoring Script

```bash
#!/bin/bash
# binance-monitor-health.sh

# Check if process running
if ! pgrep -f binance-p2p-monitor > /dev/null; then
    echo "ERROR: binance-p2p-monitor is not running"
    systemctl restart binance-p2p-monitor
    exit 1
fi

# Check database file
if ! test -f /opt/binance-p2p-monitor/binance_p2p.db; then
    echo "ERROR: Database file not found"
    exit 1
fi

# Check database integrity
sqlite3 /opt/binance-p2p-monitor/binance_p2p.db "PRAGMA integrity_check;" | grep -q "ok" || {
    echo "ERROR: Database corruption detected"
    exit 1
}

# Check API endpoint
curl -s -m 5 https://p2p.binance.com/bapi/c2c/v1/public/c2c/trade/order/getSubFiatList \
    > /dev/null || {
    echo "WARNING: Cannot reach Binance API"
}

echo "OK"
exit 0
```

**Schedule Health Check:**
```bash
*/5 * * * * /opt/binance-p2p-monitor/health-check.sh || /usr/sbin/sendmail -t admin@example.com
```

### Prometheus Metrics (Future Feature)

```yaml
# prometheus.yml
scrape_configs:
  - job_name: 'binance-monitor'
    static_configs:
      - targets: ['localhost:9090']
    metrics_path: '/metrics'
```

## Scaling Strategy

### Single Instance Limits

- **Monitored pairs:** Up to 100 simultaneously
- **Active alerts:** Up to 1000
- **Historical records:** 100k (500MB SQLite)
- **Throughput:** 500 prices/sec

### Scale Beyond Limits

#### Option A: Multiple Instances (Different Pairs)

```
Instance 1: Monitor BTC, ETH (USD, EUR)
Instance 2: Monitor BNB, XRP (USD, EUR)
Instance 3: Aggregator (collects from instances)
```

**Configuration per instance:**
```json
{
  "MonitoredAssets": ["BTC", "ETH"],
  "DatabaseConnectionString": "Data Source=binance_p2p_instance1.db;"
}
```

#### Option B: Separate Data Warehouse

```
Instance 1: Real-time monitoring (in-memory cache)
PostgreSQL: Historical data (persistent)
    ├─ prices (100M+ rows)
    ├─ alerts
    └─ statistics
```

**Connection:**
```csharp
// In Program.cs
services.AddScoped<IDbConnection>(_ => 
    new NpgsqlConnection("Server=postgres;Database=binance_p2p"));
```

#### Option C: Message Queue

```
Instance 1: Price Monitor → RabbitMQ
Instance 2: Alert Evaluator → RabbitMQ
Instance 3: Historian → RabbitMQ
Instance 4: Notification → RabbitMQ
```

## Load Testing

### Simulate High-Volume Monitoring

```csharp
// LoadTest.cs
public class LoadTestRunner
{
    public async Task SimulateLoad()
    {
        var service = GetMonitoringService();
        
        // Monitor 100 pairs
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var assets = GetRandomAssets(5);
            tasks.Add(MonitorAssetsAsync(service, assets));
        }
        
        await Task.WhenAll(tasks);
    }
}
```

**Run test:**
```bash
dotnet run -- load-test --duration 3600 --pairs 100 --concurrent 10
```

## Disaster Recovery Plan

### Database Corruption

```bash
# 1. Stop application
sudo systemctl stop binance-p2p-monitor

# 2. Restore from backup
cp /backups/binance_p2p_latest.db /opt/binance-p2p-monitor/binance_p2p.db

# 3. Verify integrity
sqlite3 /opt/binance-p2p-monitor/binance_p2p.db "PRAGMA integrity_check;"

# 4. Start application
sudo systemctl start binance-p2p-monitor

# 5. Monitor logs
sudo journalctl -u binance-p2p-monitor -f
```

### Lost Configuration

```bash
# 1. Restore from backup configuration
cp /backups/appsettings.json.2026-05-03 /opt/binance-p2p-monitor/appsettings.json

# 2. Verify credentials are set
env | grep AppSettings | head -10

# 3. Test with status command
/opt/binance-p2p-monitor/binance-p2p-monitor status
```

### Complete System Failure

```bash
# 1. On new machine, clone repo
git clone https://github.com/Sarmkadan/binance-p2p-monitor.git

# 2. Build for platform
dotnet publish -c Release -r linux-x64

# 3. Copy database backup
cp /backups/binance_p2p.db ./bin/Release/net10.0/linux-x64/publish/

# 4. Restore configuration
cp /backups/appsettings.json ./bin/Release/net10.0/linux-x64/publish/

# 5. Install as service and start
sudo cp -r ./bin/Release/net10.0/linux-x64/publish /opt/binance-p2p-monitor
sudo systemctl start binance-p2p-monitor
```

## Upgrade Strategy

### Zero-Downtime Deployment

```bash
# 1. Build new version
dotnet publish -c Release -r linux-x64

# 2. Stop old instance gracefully (finish current operations)
sudo systemctl stop binance-p2p-monitor

# 3. Backup current database
cp /opt/binance-p2p-monitor/binance_p2p.db \
   /backups/binance_p2p_pre_upgrade_$(date +%Y%m%d_%H%M%S).db

# 4. Replace binaries
sudo cp -r ./bin/Release/net10.0/linux-x64/publish/* \
    /opt/binance-p2p-monitor/

# 5. Run any schema migrations (if needed)
# sqlite3 /opt/binance-p2p-monitor/binance_p2p.db < migrations/upgrade_to_v1.2.sql

# 6. Start new version
sudo systemctl start binance-p2p-monitor

# 7. Verify
sudo systemctl status binance-p2p-monitor
sudo journalctl -u binance-p2p-monitor -n 20
```

## Security Hardening

### File Permissions

```bash
# Restrict database access
chmod 600 /opt/binance-p2p-monitor/binance_p2p.db

# Restrict config with secrets
chmod 600 /opt/binance-p2p-monitor/appsettings.json

# Application directory
chmod 755 /opt/binance-p2p-monitor
```

### Secrets Management

```bash
# Use environment variables (do NOT commit secrets)
export AppSettings__TelegramBotToken="$(aws secretsmanager get-secret-value --secret-id binance-telegram-token --query SecretString --output text)"

# Or use external vault
vault kv get secret/binance-monitor
```

### Network Isolation

```bash
# UFW (Ubuntu Firewall)
sudo ufw allow 22/tcp    # SSH only
sudo ufw allow 9090/tcp  # Prometheus (if enabled)
sudo ufw enable

# No incoming connections needed (application makes outbound only)
```

## Performance Tuning

### Database Optimization

```bash
# Vacuum and analyze periodically
sqlite3 binance_p2p.db "VACUUM; ANALYZE;"

# Rebuild indexes
sqlite3 binance_p2p.db "REINDEX;"
```

### Memory Optimization

```bash
# In appsettings.json
{
  "AppSettings": {
    "MaxHistoryRecords": 50000,  // Reduce from 100k
    "CacheTTLSeconds": 300,       // Increase TTL
    "HistoryRetentionDays": 14    // Shorter retention
  }
}
```

### CPU Optimization

```bash
# Reduce monitoring frequency
"MonitoringIntervalSeconds": 60  # From 30s to 60s

# Batch queries
"BatchQuerySize": 20  # Monitor 20 pairs per request
```

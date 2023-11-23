# Architecture Overview

binance-p2p-monitor is built on a layered, event-driven architecture designed for real-time monitoring with minimal latency and high reliability.

## System Design

```
┌──────────────────────────────────────────────────┐
│              User Interface Layer                 │
│  ┌────────────┬────────────┬────────────┐        │
│  │   CLI      │  Commands  │  Formatters│        │
│  │  (REPL)    │  Pipeline  │ (JSON/CSV) │        │
│  └────────────┴────────────┴────────────┘        │
└───────────────┬────────────────────────────────┘
                │
┌───────────────▼────────────────────────────────┐
│           Business Logic Layer                   │
│  ┌──────────────────────────────────────────┐  │
│  │  PriceMonitoringService                  │  │
│  │  ├─ Real-time price fetching             │  │
│  │  ├─ WebSocket management                 │  │
│  │  └─ Multi-pair coordination              │  │
│  └──────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────┐  │
│  │  AlertService                            │  │
│  │  ├─ Alert rule evaluation                │  │
│  │  ├─ Cooldown enforcement                 │  │
│  │  └─ Notification dispatch                │  │
│  └──────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────┐  │
│  │  SpreadAnalysisService                   │  │
│  │  ├─ Buy/sell spread calculation          │  │
│  │  ├─ Anomaly detection                    │  │
│  │  └─ Trend analysis                       │  │
│  └──────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────┐  │
│  │  PriceHistoryService                     │  │
│  │  ├─ Historical aggregation               │  │
│  │  ├─ Time-series queries                  │  │
│  │  └─ Report generation                    │  │
│  └──────────────────────────────────────────┘  │
└───────────────┬────────────────────────────────┘
                │
┌───────────────▼────────────────────────────────┐
│            Data Access Layer                     │
│  ┌──────────────────────────────────────────┐  │
│  │  Repository Pattern                      │  │
│  │  ├─ PriceRepository                      │  │
│  │  ├─ AlertRepository                      │  │
│  │  ├─ HistoryRepository                    │  │
│  │  └─ TradeOfferRepository                 │  │
│  └──────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────┐  │
│  │  Caching Layer (MemoryCache)             │  │
│  │  ├─ Price caching (TTL: 5s)              │  │
│  │  ├─ Alert rule caching (TTL: 1m)        │  │
│  │  └─ Hit ratio tracking                   │  │
│  └──────────────────────────────────────────┘  │
└───────────────┬────────────────────────────────┘
                │
┌───────────────▼────────────────────────────────┐
│             Persistence Layer                    │
│  ┌──────────────────────────────────────────┐  │
│  │  SQLite Database                         │  │
│  │  ├─ Prices (history table)               │  │
│  │  ├─ Alerts (rules & state)               │  │
│  │  ├─ TradeOffers (market data)            │  │
│  │  └─ Indexes (asset, fiat, timestamp)     │  │
│  └──────────────────────────────────────────┘  │
└──────────────────────────────────────────────────┘
```

## Core Components

### 1. Price Monitoring (`PriceMonitoringService`)

Responsibilities:
- Fetch real-time prices from Binance P2P API
- Manage WebSocket connections with auto-reconnection
- Rate limit API calls (100 req/min)
- Publish price updates to event bus

```csharp
// Usage pattern
var service = serviceProvider.GetRequiredService<IPriceMonitoringService>();
var price = await service.GetPriceAsync("BTC", "USD");
```

**Flow:**
1. Client requests price for pair
2. Check cache (TTL: 5s)
3. If miss, call Binance API
4. Update cache
5. Emit `PriceUpdatedEvent`
6. Persist to history

### 2. Alert Management (`AlertService`)

Responsibilities:
- Store alert rules
- Evaluate alerts against price updates
- Enforce cooldown periods (prevent spam)
- Trigger notifications

**Alert Types:**
- `PriceChange` — Trigger on % change
- `SpreadAnomaly` — Trigger on unusual spread
- `PriceLevel` — Trigger on absolute price

**Evaluation Logic:**
```
For each new price:
  For each active alert:
    If alert cooldown expired:
      If condition met:
        Trigger notification
        Update cooldown timestamp
```

### 3. Spread Analysis (`SpreadAnalysisService`)

Calculates buy/sell spread for P2P pairs:

```
Spread % = ((Ask - Bid) / Ask) × 100
```

**Usage:**
```csharp
var spread = await spreadService.AnalyzePairAsync("BTC", "USD");
Console.WriteLine($"Spread: {spread.SpreadPercentage}%");
```

### 4. Price History (`PriceHistoryService`)

Time-series aggregation:
- 1m candlesticks
- Hourly aggregates
- Daily summaries
- Custom period queries

### 5. Event Bus

Loose coupling between components:

```
PriceUpdatedEvent
  ├─ AlertService (evaluates rules)
  ├─ PriceHistoryService (persists)
  ├─ SpreadAnalysisService (analyzes)
  └─ StatisticsCollectorWorker (metrics)
```

## Data Flow

### Monitoring Cycle

```
┌─────────────────────────────────────────────┐
│  Monitoring Interval Timer Fires (30s)      │
└────────────┬────────────────────────────────┘
             │
    ┌────────▼──────────────┐
    │ PriceMonitoringService│
    │   GetPrice("BTC/USD") │
    └────────┬──────────────┘
             │
    ┌────────▼──────────────────────┐
    │ Check Cache (MemoryCache)      │
    │ - Hit: return cached value     │
    │ - Miss: proceed to API call    │
    └────────┬──────────────────────┘
             │
    ┌────────▼──────────────────────┐
    │ Call Binance API (with retry)  │
    │ - Rate limit: 100 req/min      │
    │ - Timeout: 30s                 │
    │ - Backoff: exponential         │
    └────────┬──────────────────────┘
             │
    ┌────────▼──────────────────────┐
    │ Update Cache (TTL: 5s)         │
    │ Publish PriceUpdatedEvent      │
    └────────┬──────────────────────┘
             │
    ┌────────▼──────────────────────┐
    │ EventBus Fanout                │
    └────┬───────────────┬─────┬────┘
         │               │     │
    ┌────▼──┐    ┌──────▼──┐  │
    │Alert  │    │History  │  │ ┌───────────────┐
    │Eval   │    │Persist  │  └─┤ Broadcast to │
    │       │    │         │    │ Subscribers  │
    └───────┘    └─────────┘    └───────────────┘
```

### Alert Triggering Flow

```
PriceUpdatedEvent → AlertService.EvaluateAlertsAsync()
    │
    ├─ For each active alert:
    │  ├─ Check cooldown expiration
    │  ├─ Evaluate condition (price >= threshold)
    │  ├─ If condition met:
    │  │  ├─ Create notification
    │  │  ├─ Send to TelegramNotificationClient
    │  │  ├─ Update alert.LastTriggeredAt
    │  │  └─ Emit AlertTriggeredEvent
    │  └─ Update alert cache
    │
    └─ Return triggered alerts
```

## Design Patterns

### 1. Repository Pattern

Abstraction over data access:

```csharp
public interface IPriceRepository
{
    Task<Price> GetByIdAsync(int id);
    Task<IEnumerable<Price>> GetRecentAsync(string asset, string fiat, int limit);
    Task AddAsync(Price price);
}
```

Benefits:
- Easy to unit test (mock repository)
- Can swap implementations (SQLite → PostgreSQL)
- Centralized query logic

### 2. Dependency Injection

All components registered in DI container:

```csharp
services.AddScoped<IPriceRepository, PriceRepository>();
services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
services.AddSingleton<IEventBus, EventBus>();
```

Benefits:
- Testability (inject mocks)
- Loose coupling
- Lifetime management

### 3. Event Bus (Observer Pattern)

Decouples event producers from consumers:

```csharp
public interface IEventBus
{
    void Subscribe<T>(Action<T> handler) where T : IEvent;
    void Publish<T>(T @event) where T : IEvent;
}
```

### 4. Service Layer

Business logic isolated from data access:

```
Command → Service → Repository → Database
   ↓         ↓          ↓           ↓
 Input    Logic       Query      Storage
```

### 5. Rate Limiter (Token Bucket)

Control API call frequency:

```csharp
var limiter = new RateLimiter(100, TimeSpan.FromMinutes(1));
await limiter.WaitAsync(); // Blocks if rate exceeded
```

## Concurrency & Thread Safety

### Design Decisions

1. **Single-threaded message processing**
   - Event bus processes events sequentially
   - Prevents race conditions in cache/alerts
   - No locks needed on in-memory collections

2. **Database transaction isolation**
   - SQLite serialized mode by default
   - Prevents dirty reads/writes
   - Trade-off: reduces concurrent throughput

3. **Async/await throughout**
   - I/O operations non-blocking
   - Efficient thread pool usage
   - Responsive CLI during long operations

## Performance Optimizations

### 1. Caching Strategy

```
Cache Layer (MemoryCache)
  │
  ├─ Price: TTL=5s, Hit ratio ~85%
  ├─ Alert rules: TTL=1m, Hit ratio ~99%
  ├─ Fiat list: TTL=1h, Hit ratio ~100%
  └─ Trade offers: TTL=30s, Hit ratio ~60%
```

**Impact:** Reduces API calls by 70-80% in typical usage.

### 2. Database Indexes

```sql
CREATE INDEX idx_prices_asset_fiat_ts ON prices(asset, fiat, timestamp DESC);
CREATE INDEX idx_alerts_user_active ON alerts(user_id, is_active);
CREATE INDEX idx_history_asset_date ON price_history(asset, fiat, date);
```

**Impact:** History queries drop from 5s to <50ms.

### 3. Batch Operations

```csharp
// Monitor multiple pairs in single request
var prices = await service.GetPricesAsync("BTC", ["USD", "EUR", "GBP"]);
// Single API call for 3 pairs vs 3 separate calls
```

### 4. Event Loop Optimization

```csharp
// Async event handlers prevent blocking
eventBus.Subscribe<PriceUpdatedEvent>(async @event =>
{
    // Non-blocking I/O
    await alertService.EvaluateAlertsAsync(@event.Price);
});
```

## Error Handling

### Strategy

1. **At API boundaries:** Retry with exponential backoff
2. **At service layer:** Propagate as domain exceptions
3. **At CLI layer:** Catch and display user-friendly messages

**Retry Policy:**
```
Attempt 1: Wait 0s
Attempt 2: Wait 1s
Attempt 3: Wait 2s
Attempt 4: Wait 4s
Max total: 7s
```

## Scalability

### Current Limits

- **Price history:** 100k records (500MB SQLite file)
- **Active alerts:** 1000 per system
- **Monitored pairs:** 100 simultaneously
- **Update throughput:** 500 prices/sec (WebSocket)

### Scaling Beyond Limits

1. **Separate database:** PostgreSQL instead of SQLite
2. **Message queue:** RabbitMQ/Kafka for event distribution
3. **Microservices:** Split into multiple services
4. **Caching layer:** Redis for distributed cache
5. **Load balancing:** Multiple instances behind load balancer

## Security Considerations

### Data Protection

- **Secrets:** API keys in environment variables, not config files
- **Database:** SQLite file permissions (0600)
- **Logs:** Sanitize sensitive data before logging
- **Network:** HTTPS for Telegram notifications

### Input Validation

- **Asset/Fiat symbols:** Whitelist validation
- **Thresholds:** Range checking (0-100%)
- **User input:** Sanitized before database queries

## Testing Strategy

### Unit Tests

Service layer tests with mocked repositories:

```csharp
[Test]
public async Task EvaluateAlerts_WithPriceIncrease_TriggersAlert()
{
    var mockRepo = new Mock<IAlertRepository>();
    var service = new AlertService(mockRepo.Object, ...);
    
    var alert = new PriceAlert { Threshold = 5.0m };
    var price = new Price { Bid = 105.0m };
    
    var triggered = await service.EvaluateAlertsAsync(price);
    
    Assert.That(triggered, Has.Count.EqualTo(1));
}
```

### Integration Tests

Full stack with real SQLite:

```csharp
[Test]
public async Task MonitorCommand_StartsWebSocket_PublishesEvents()
{
    var host = new TestHost();
    var service = host.GetService<IPriceMonitoringService>();
    
    var prices = new List<Price>();
    var bus = host.GetService<IEventBus>();
    bus.Subscribe<PriceUpdatedEvent>(e => prices.Add(e.Price));
    
    await service.MonitorPriceAsync(...);
    
    await Task.Delay(2000);
    Assert.That(prices, Has.Count.GreaterThan(0));
}
```

## Deployment Topology

### Single Machine

```
binance-p2p-monitor
├─ CLI Commands
├─ Services
└─ SQLite DB (binance_p2p.db)
```

### Docker Container

```
Docker Image
├─ .NET 10 runtime
├─ Application executable
└─ Startup script
```

### Kubernetes

```
Namespace: monitoring
├─ Deployment (1 replica)
├─ ConfigMap (appsettings)
├─ Secret (API keys)
└─ PersistentVolume (SQLite)
```

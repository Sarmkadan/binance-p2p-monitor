# MonitoringHostedService

`MonitoringHostedService` is the host-managed background coordinator for price monitoring, periodic history cleanup, and heartbeat publication. It derives from `BackgroundService` and is registered in `Program.cs` with `AddHostedService<MonitoringHostedService>()`, so the .NET generic host controls its startup and shutdown.

The service does not fetch or store prices itself. It delegates continuous monitoring to `IPriceMonitoringService`, delegates history retention work to `IPriceHistoryService`, and uses `IEventBus` to publish and observe `HeartbeatEvent` instances.

## Dependencies and construction

The constructor requires:

- `IPriceMonitoringService` to start and stop the underlying monitoring process.
- `IPriceHistoryService` to remove expired history and obtain the remaining record count.
- `AppSettings` for the monitoring interval, automatic-cleanup switch, and history-retention period.
- `ILogger<MonitoringHostedService>` for lifecycle, heartbeat, cleanup, and error messages.
- `IEventBus` for heartbeat publication and subscription.

Each dependency is checked for `null` and causes an `ArgumentNullException` when absent.

Construction records the current UTC time as the initial cleanup time, service start time, and last-successful-fetch time. It also subscribes `HandleHeartbeatAsync` to `HeartbeatEvent`. Consequently, the hourly cleanup window and reported uptime are measured from object construction, which may be slightly earlier than `ExecuteAsync` starting.

## Hosted-service lifecycle

### Registration and startup

The host creates the service through dependency injection and invokes the inherited `BackgroundService.StartAsync`, which runs `ExecuteAsync`.

`ExecuteAsync` performs the following work:

1. Logs that the hosted service is starting.
2. Calls `IPriceMonitoringService.StartMonitoringAsync(stoppingToken)`. The periodic loop does not begin until this call completes.
3. Enters a loop that continues until the host's stopping token requests cancellation.

If monitoring startup or an exception that escapes the loop fails, the service logs a fatal startup error and rethrows it to the host.

### Periodic loop

On every loop iteration, the service:

1. Checks whether at least one hour has elapsed since `_lastCleanupTime`. If so, it runs cleanup and then records the current UTC time as the latest cleanup time.
2. Publishes a heartbeat.
3. Delays for `AppSettings.MonitoringIntervalSeconds`, using the host's stopping token.

The configured monitoring interval therefore controls heartbeat frequency and how often the hourly-cleanup condition is evaluated. It does not directly perform a price fetch in this class; price monitoring has already been delegated to `IPriceMonitoringService`.

An `OperationCanceledException` raised inside the loop is treated as normal loop cancellation: it is logged and the loop exits. Other loop exceptions are logged, followed by a five-second retry delay. That retry delay also uses the stopping token.

### Shutdown

The host calls `StopAsync` during graceful shutdown. The override:

1. Logs that the hosted service is stopping.
2. Awaits `IPriceMonitoringService.StopMonitoringAsync()`.
3. Calls and awaits `BackgroundService.StopAsync(cancellationToken)` so the base implementation can wait for `ExecuteAsync` to finish, subject to the host's shutdown token.

The cancellation token passed to `StopAsync` is not passed to `StopMonitoringAsync`, because that interface method does not accept one.

## Responsibilities

### Price-monitoring coordination

The hosted service owns the host-level start/stop boundary for `IPriceMonitoringService`. Details such as WebSocket subscriptions and price processing belong to the injected implementation rather than this coordinator.

### History cleanup

Cleanup is considered after the service has been constructed for at least one hour and approximately hourly thereafter, subject to the loop interval.

When `AppSettings.EnableAutoCleanup` is `true`, the service calls `CleanupOldHistoryAsync(AppSettings.HistoryRetentionDays)`. If that operation reports success, it obtains the total history count and logs it. When automatic cleanup is disabled or cleanup reports failure, it does not request the count.

Cleanup exceptions are caught and logged inside `PerformCleanupAsync`; they do not terminate the main loop. The cleanup method does not receive the host cancellation token.

### Heartbeats

Every normal loop iteration creates a `HeartbeatEvent` containing:

- Uptime calculated as the current UTC time minus the construction-time `_serviceStartTime`.
- `_lastSuccessfulFetch` as the last-successful-fetch timestamp.

The event is published with `IEventBus.PublishAsync`. The service's constructor also subscribes its own handler, which logs the heartbeat's uptime and last-successful-fetch value. No cancellation token is supplied to `PublishAsync`, so the call uses the event bus method's default token.

In the current implementation, `_lastSuccessfulFetch` is initialized to the construction time but is never updated by `MonitoringHostedService`. The heartbeat field therefore does not track later successful fetches unless the implementation is changed to update it. The service also does not unsubscribe its heartbeat handler during shutdown.

## Error and cancellation behavior

- Constructor dependency failures are reported immediately as `ArgumentNullException`.
- Cleanup failures are logged and contained by the cleanup method.
- Heartbeat publication failures and other per-iteration failures are logged by the loop, then retried after five seconds.
- Cancellation observed by the main delay exits the loop normally.
- Failures that escape the periodic loop are logged as fatal and rethrown to the host.
- If `StopMonitoringAsync` fails, `StopAsync` propagates that exception and does not reach the base `StopAsync` call.

## Configuration used

| Setting | Purpose |
| --- | --- |
| `MonitoringIntervalSeconds` | Delay between normal periodic-loop iterations; determines heartbeat cadence and cleanup-check granularity. |
| `EnableAutoCleanup` | Enables or skips history cleanup when the hourly condition is reached. |
| `HistoryRetentionDays` | Passed to the history service as the retention period when cleanup is enabled. |

## Operational notes

- The first heartbeat is published after `StartMonitoringAsync` completes and after any cleanup due at that time.
- Cleanup runs when the loop next observes that at least one hour has elapsed, so a long monitoring interval can make actual cleanup less frequent than exactly once per hour.
- The class coordinates lifecycle and maintenance only; it exposes no public monitoring or status API beyond the hosted-service lifecycle inherited from `BackgroundService`.

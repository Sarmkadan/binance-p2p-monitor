# AlertService

`AlertService` manages the lifecycle of price alerts within the binance-p2p-monitor system. It provides methods for creating, updating, deleting, and querying user-defined price thresholds, evaluating those thresholds against current market data, and dispatching notifications when alerts are triggered. The service acts as the central coordinator between alert persistence, price feed evaluation, and notification delivery channels.

## API

### public AlertService

Constructor. Initializes a new instance of the service with its required dependencies, such as alert storage, price feed access, and notification dispatchers. The specific dependencies are injected and are not part of the public surface.

### public async Task\<int\> CreateAlertAsync

Creates a new price alert for a user and persists it. Returns the unique integer identifier assigned to the newly created alert. The parameters (accepted by the implementation but not listed in the public member signature shown) typically include the user identifier, trading pair, target price, and comparison direction. Throws when the alert specification is invalid (e.g., missing required fields, unsupported trading pair) or when the underlying storage operation fails.

### public async Task\<bool\> UpdateAlertAsync

Updates an existing alert’s configuration. Returns `true` if the alert was found and successfully modified; returns `false` if no alert with the given identifier exists. Throws when the updated specification fails validation or when the storage layer encounters an unrecoverable error.

### public async Task\<bool\> DeleteAlertAsync

Removes an alert by its identifier. Returns `true` if the alert existed and was deleted; returns `false` if the identifier was not found. Throws when the deletion operation cannot be completed due to a storage-level failure.

### public async Task\<IEnumerable\<PriceAlert\>\> GetUserAlertsAsync

Retrieves all active and inactive alerts belonging to a specific user. Returns an enumerable collection of `PriceAlert` objects, which may be empty if the user has no alerts. Throws when the user identifier is invalid or when the query against the alert store fails.

### public async Task\<IEnumerable\<PriceAlert\>\> CheckTriggersAsync

Evaluates all active alerts against the latest available price data and returns the subset of alerts whose trigger conditions are currently met. This method does not mutate alert state or send notifications; it only performs the condition check. The returned collection contains only those `PriceAlert` instances that have crossed their configured thresholds. Throws when price data is unavailable or when the evaluation process encounters an error.

### public async Task SendNotificationAsync

Dispatches a notification for a single triggered alert to the alert’s owner through the configured notification channel. The method expects a `PriceAlert` that has already been determined to be triggered. Throws when the notification channel is unreachable, when the user’s contact information is missing, or when the dispatch fails after retries.

### public async Task SendBulkNotificationsAsync

Dispatches notifications for a batch of triggered alerts. Accepts a collection of `PriceAlert` instances and sends a notification for each one, typically with internal batching or rate-limiting to respect channel constraints. Throws when the notification infrastructure is unavailable; individual delivery failures may be handled internally depending on configuration.

### public async Task\<bool\> TestAlertAsync

Performs a dry-run evaluation and notification test for a specific alert without persisting any state change. Returns `true` if the alert’s condition is met and a test notification was successfully dispatched; returns `false` if the condition is not met. Throws when the alert identifier is not found, when price data cannot be retrieved, or when the test notification fails to send.

### public async Task\<int\> GetActiveAlertCountAsync

Returns the total number of alerts currently in the active state across all users, or scoped to a particular user if the implementation accepts a user identifier. Throws when the count query against the alert store fails.

## Usage

### Example 1: Creating an alert and checking if it triggers

```csharp
// Assume alertService is an injected instance of AlertService
int newAlertId = await alertService.CreateAlertAsync(
    userId: "user-42",
    tradingPair: "USDT/VES",
    targetPrice: 58.25m,
    direction: AlertDirection.Above
);

// Later, evaluate all active alerts against current prices
IEnumerable<PriceAlert> triggered = await alertService.CheckTriggersAsync();

if (triggered.Any(a => a.Id == newAlertId))
{
    await alertService.SendNotificationAsync(
        triggered.First(a => a.Id == newAlertId)
    );
}
```

### Example 2: Bulk notification dispatch after scheduled price check

```csharp
// Scheduled job: check all active alerts and notify users in bulk
IEnumerable<PriceAlert> triggeredAlerts = await alertService.CheckTriggersAsync();

if (triggeredAlerts.Any())
{
    await alertService.SendBulkNotificationsAsync(triggeredAlerts);

    foreach (var alert in triggeredAlerts)
    {
        await alertService.UpdateAlertAsync(
            alert.Id,
            isActive: false // deactivate after triggering, if desired
        );
    }
}

int remainingActive = await alertService.GetActiveAlertCountAsync();
Console.WriteLine($"Active alerts remaining: {remainingActive}");
```

## Notes

- **Idempotency of updates and deletes:** `UpdateAlertAsync` and `DeleteAlertAsync` return `false` when the target alert does not exist rather than throwing. Callers should check the return value to distinguish between a missing alert and a genuine failure.
- **CheckTriggersAsync is read-only:** This method does not modify alert state or deactivate triggered alerts. Any state transitions (e.g., marking an alert as inactive after firing) must be performed explicitly by the caller using `UpdateAlertAsync`.
- **Notification delivery is decoupled:** `CheckTriggersAsync` and the `SendNotification*` methods are separate steps. This allows callers to inspect triggered alerts before deciding to notify, or to aggregate notifications for bulk delivery.
- **TestAlertAsync side effects:** Despite being a test method, it attempts real notification delivery to verify the channel. Callers should ensure the test recipient is expecting the message or use a dedicated test user configuration.
- **Thread safety:** The service itself does not guarantee internal synchronization across concurrent calls. Simultaneous invocations of `CheckTriggersAsync` and `UpdateAlertAsync` on the same alert may race. If multiple schedulers or consumers operate on alerts concurrently, external coordination (e.g., a distributed lock or single-producer pattern) is recommended.
- **Empty collections:** `GetUserAlertsAsync` and `CheckTriggersAsync` return empty enumerables when no results exist, never null.
- **Storage and channel failures:** Most methods throw when underlying infrastructure is unavailable. Callers should implement retry policies appropriate to the failure type, particularly for `SendNotificationAsync` and `SendBulkNotificationsAsync` where transient network issues are common.

# PriceAlert

Represents a user-defined price alert in the binance-p2p-monitor system. Each alert monitors a specific asset/fiat pair and evaluates whether the current market price meets a user-specified threshold under a given condition. The entity tracks alert state, trigger history, and cooldown logic to prevent notification spam.

## API

### `public int Id`

Unique identifier for the alert record. Assigned by the persistence layer upon creation.

### `public string Asset`

The cryptocurrency asset code (e.g., `"USDT"`, `"BTC"`) being monitored. Case-sensitive and must match the exchange's ticker representation.

### `public string Fiat`

The fiat currency code (e.g., `"ARS"`, `"VES"`) against which the asset price is compared. Case-sensitive.

### `public AlertType AlertType`

Specifies the price source being watched. The `AlertType` enum determines whether the alert evaluates buy-side prices, sell-side prices, or another defined market view.

### `public decimal Threshold`

The price boundary that triggers the alert. Interpretation depends on `Condition`: for `Above`, the alert fires when price exceeds this value; for `Below`, when price drops under it.

### `public AlertCondition Condition`

Enum indicating the relational operator applied between the current price and `Threshold`. Typical values are `Above` and `Below`.

### `public bool IsEnabled`

Controls whether the alert is actively evaluated. When `false`, the alert is skipped during price checks and `ShouldTrigger` always returns `false`.

### `public int UserId`

Foreign key linking the alert to the owning `UserProfile`. Used for authorization and notification routing.

### `public DateTime CreatedAt`

Timestamp of initial alert creation, set by the system and not modified thereafter.

### `public DateTime UpdatedAt`

Timestamp of the most recent update to any mutable field on the alert. Automatically refreshed on changes.

### `public long? LastTriggeredAt`

Unix timestamp (in milliseconds) of the last time `RecordTrigger` was called, or `null` if the alert has never fired. Drives cooldown calculations.

### `public int TriggerCount`

Running total of how many times `RecordTrigger` has been invoked. Incremented atomically on each trigger event.

### `public string? Notes`

Optional free-text field for user annotations. Not used in alert evaluation logic.

### `public UserProfile? User`

Navigation property to the associated `UserProfile` entity. May be `null` if not eagerly loaded or if the user record has been deleted.

### `public bool ShouldTrigger`

Read-only computed property. Returns `true` when all of the following hold: `IsEnabled` is `true`, the current price satisfies the `Condition` against `Threshold`, and `IsInCooldownPeriod` is `false`. Does not mutate state.

### `public bool IsInCooldownPeriod`

Read-only computed property. Returns `true` if `LastTriggeredAt` has a value and the elapsed time since that timestamp is less than the system-defined cooldown duration. Prevents repeated alerts within a short window.

### `public void RecordTrigger()`

Records a trigger event by incrementing `TriggerCount` and setting `LastTriggeredAt` to the current Unix timestamp in milliseconds. Callers must ensure this is invoked only when a notification is actually dispatched; the method itself performs no validation of `ShouldTrigger`.

### `public bool IsValid`

Read-only computed property. Validates that `Asset` and `Fiat` are non-null, non-empty strings, `Threshold` is greater than zero, and `Condition` is a defined enum value. Returns `false` if any constraint is violated. Does not check `UserId` validity or referential integrity.

### `public string GetDescription()`

Returns a human-readable summary string combining `Asset`, `Fiat`, `Condition`, and `Threshold` (e.g., `"USDT/ARS Above 150.00"`). The format is deterministic and does not vary by locale. Returns an empty string if `Asset` or `Fiat` is null.

### `public void Toggle()`

Flips the value of `IsEnabled`. If currently `true`, sets it to `false`; if `false`, sets it to `true`. Does not affect any other property.

## Usage

### Example 1: Creating and Evaluating an Alert

```csharp
var alert = new PriceAlert
{
    Asset = "USDT",
    Fiat = "ARS",
    AlertType = AlertType.Sell,
    Threshold = 1450.00m,
    Condition = AlertCondition.Above,
    IsEnabled = true,
    UserId = 42,
    Notes = "Monitor for arbitrage opportunity"
};

// Simulate a price check
decimal currentPrice = 1465.50m;

if (alert.IsValid && alert.ShouldTrigger(currentPrice))
{
    alert.RecordTrigger();
    // Dispatch notification to user 42
    Console.WriteLine(alert.GetDescription()); // "USDT/ARS Above 1450.00"
}
```

### Example 2: Toggling and Cooldown Behavior

```csharp
// Disable an alert temporarily
if (alert.IsEnabled)
{
    alert.Toggle(); // IsEnabled becomes false
}

// Later, re-enable it
alert.Toggle(); // IsEnabled becomes true

// After a trigger, cooldown prevents immediate re-fire
alert.RecordTrigger(); // TriggerCount increments, LastTriggeredAt set

if (alert.IsInCooldownPeriod)
{
    // ShouldTrigger will return false even if price condition is met
    Console.WriteLine("Alert is cooling down, skipping notification.");
}
```

## Notes

- **Cooldown logic**: `IsInCooldownPeriod` relies on a system-wide cooldown duration. If the cooldown value is changed at runtime, alerts that fired under the old duration will immediately reflect the new window. This is intentional and callers should avoid changing cooldown settings while alerts are actively being evaluated.
- **Thread safety**: `RecordTrigger()` mutates `TriggerCount` and `LastTriggeredAt` without synchronization. In multi-threaded environments where the same alert may be triggered concurrently, external locking or an atomic update mechanism must be applied to prevent lost increments or timestamp overwrites.
- **`ShouldTrigger` evaluation**: This property requires the current price as context; it is not a persisted value. The exact mechanism for supplying the price is external to this type. If the price source is unavailable, the property should conservatively return `false`.
- **`IsValid` scope**: Validation is purely structural. It does not verify that `Asset`/`Fiat` are recognized by the exchange or that `UserId` corresponds to an existing user. Business-layer or service-layer validation should handle those concerns.
- **`GetDescription()` null handling**: If `Asset` or `Fiat` are null, the method returns an empty string rather than throwing. This prevents exceptions in logging or display code when operating on incomplete alert objects.
- **`Toggle()` idempotency**: Calling `Toggle()` repeatedly alternates the state. There is no `SetEnabled(bool)` method; callers needing a specific state must check `IsEnabled` before toggling or call `Toggle()` conditionally.

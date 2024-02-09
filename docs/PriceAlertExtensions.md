# PriceAlertExtensions

Static utility class providing extension methods for `PriceAlert` instances. It centralises common operations such as cloning, threshold management, age calculation, trigger evaluation, and note appending, enabling consistent manipulation of price alert objects throughout the monitoring system.

## API

### `Clone`

```csharp
public static PriceAlert Clone(this PriceAlert alert)
```

Creates a deep copy of the given `PriceAlert`. All properties, including nested collections and threshold data, are duplicated into a new instance.

**Parameters:**
- `alert` — The source `PriceAlert` to clone. Must not be `null`.

**Return value:**
A new `PriceAlert` object that is a field-by-field copy of the original.

**Throws:**
- `ArgumentNullException` — when `alert` is `null`.

---

### `HasTriggeredAtLeast`

```csharp
public static bool HasTriggeredAtLeast(this PriceAlert alert, int count)
```

Determines whether the alert has been triggered a minimum number of times.

**Parameters:**
- `alert` — The `PriceAlert` to inspect. Must not be `null`.
- `count` — The minimum number of triggers required.

**Return value:**
`true` if the alert’s recorded trigger count is greater than or equal to `count`; otherwise `false`.

**Throws:**
- `ArgumentNullException` — when `alert` is `null`.
- `ArgumentOutOfRangeException` — when `count` is negative.

---

### `UpdateThreshold`

```csharp
public static bool UpdateThreshold(this PriceAlert alert, decimal newThreshold)
```

Replaces the current price threshold of the alert with a new value, returning whether the change was applied.

**Parameters:**
- `alert` — The `PriceAlert` to modify. Must not be `null`.
- `newThreshold` — The new threshold value to set.

**Return value:**
`true` if the threshold was successfully updated; `false` if the new value is identical to the existing threshold or otherwise rejected by internal validation.

**Throws:**
- `ArgumentNullException` — when `alert` is `null`.
- `ArgumentOutOfRangeException` — when `newThreshold` is zero or negative.

---

### `GetAgeInDays`

```csharp
public static double GetAgeInDays(this PriceAlert alert)
```

Calculates the age of the alert in days based on its creation timestamp and the current system time.

**Parameters:**
- `alert` — The `PriceAlert` to evaluate. Must not be `null`.

**Return value:**
A `double` representing the number of days (including fractional days) since the alert was created.

**Throws:**
- `ArgumentNullException` — when `alert` is `null`.

---

### `ShouldFire`

```csharp
public static bool ShouldFire(this PriceAlert alert, decimal currentPrice)
```

Evaluates whether the alert should fire given the current market price, considering its threshold, direction (above/below), and any suppression rules such as cooldown periods or maximum fire counts.

**Parameters:**
- `alert` — The `PriceAlert` to evaluate. Must not be `null`.
- `currentPrice` — The latest price to compare against the alert’s threshold.

**Return value:**
`true` if all firing conditions are met; otherwise `false`.

**Throws:**
- `ArgumentNullException` — when `alert` is `null`.
- `ArgumentOutOfRangeException` — when `currentPrice` is zero or negative.

---

### `AppendNotes`

```csharp
public static void AppendNotes(this PriceAlert alert, string notes)
```

Appends a text entry to the alert’s internal notes collection, typically used for audit trails or manual annotations.

**Parameters:**
- `alert` — The `PriceAlert` to annotate. Must not be `null`.
- `notes` — The string to append. Can be empty but not `null`.

**Return value:**
None (void).

**Throws:**
- `ArgumentNullException` — when `alert` or `notes` is `null`.

## Usage

### Example 1: Cloning and adjusting an alert

```csharp
PriceAlert original = repository.GetAlert(alertId);
PriceAlert copy = original.Clone();

// Raise the threshold by 2% on the copy without affecting the original.
decimal raisedThreshold = original.Threshold * 1.02m;
bool updated = copy.UpdateThreshold(raisedThreshold);

if (updated)
{
    copy.AppendNotes($"Threshold adjusted from {original.Threshold} to {raisedThreshold} on {DateTime.UtcNow:O}");
    repository.Save(copy);
}
```

### Example 2: Monitoring loop with age and trigger checks

```csharp
foreach (var alert in activeAlerts)
{
    double ageDays = alert.GetAgeInDays();

    // Stale alerts older than 30 days are skipped.
    if (ageDays > 30)
        continue;

    // Avoid re-firing alerts that have already triggered 3 times today.
    if (alert.HasTriggeredAtLeast(3))
        continue;

    if (alert.ShouldFire(currentMarketPrice))
    {
        notificationService.Send(alert);
        alert.AppendNotes($"Fired at price {currentMarketPrice} on {DateTime.UtcNow:O}");
    }
}
```

## Notes

- All methods are `static` extension methods and operate on the `PriceAlert` instance passed as the first argument. They do not maintain any shared state.
- `Clone` performs a deep copy; modifying the clone does not affect the original and vice versa.
- `UpdateThreshold` returns `false` when the new threshold equals the existing value. Callers should check the return value before assuming a change occurred.
- `GetAgeInDays` uses `DateTime.UtcNow` (or equivalent system time) at the moment of invocation. Repeated calls will return progressively larger values.
- `ShouldFire` may incorporate internal cooldown logic. Consecutive calls with the same price may return `true` only once until the cooldown expires.
- `AppendNotes` is not thread-safe by default. If multiple threads may annotate the same `PriceAlert` concurrently, external synchronisation is required.
- All methods throw `ArgumentNullException` when the `alert` parameter is `null`. Methods accepting numeric or string inputs throw `ArgumentOutOfRangeException` or `ArgumentNullException` respectively for invalid arguments.

# PriceAlertTestsExtensions

`PriceAlertTestsExtensions` is a static utility class designed to simplify unit testing and integration testing of the `PriceAlert` and `Spread` types within the `binance-p2p-monitor` project. It provides factory methods for creating test instances, a suite of predicate and state-inspection helpers to assert alert conditions, and mutating helpers to simulate trigger events, toggle enabled state, and update statistical data—all without requiring direct access to internal implementation details.

## API

### `CreateTestAlert`

```csharp
public static PriceAlert CreateTestAlert
```

**Purpose**: Creates a fully configured `PriceAlert` instance suitable for test scenarios.  
**Parameters**: None.  
**Return value**: A new `PriceAlert` object with default test values.  
**Throws**: Never throws under normal conditions; may throw if the underlying `PriceAlert` constructor throws due to environmental issues (e.g., missing dependencies).

### `CreateTestSpread`

```csharp
public static Spread CreateTestSpread
```

**Purpose**: Creates a `Spread` instance populated with sample data for testing.  
**Parameters**: None.  
**Return value**: A new `Spread` object with predefined test values.  
**Throws**: Same constraints as `CreateTestAlert`.

### `ShouldTrigger`

```csharp
public static bool ShouldTrigger
```

**Purpose**: Evaluates whether the associated `PriceAlert` meets its trigger criteria based on current market conditions and configuration.  
**Parameters**: None (operates on internal state or a contextually bound alert).  
**Return value**: `true` if the alert should fire; otherwise `false`.  
**Throws**: May throw if required underlying data is null or in an invalid state.

### `IsInCooldownPeriod`

```csharp
public static bool IsInCooldownPeriod
```

**Purpose**: Determines if the alert is currently within a cooldown window, during which repeated triggers are suppressed.  
**Parameters**: None.  
**Return value**: `true` if the cooldown period is active; otherwise `false`.  
**Throws**: May throw if the last trigger timestamp is corrupted or unreadable.

### `TriggerCount`

```csharp
public static int TriggerCount
```

**Purpose**: Exposes the number of times the alert has been triggered.  
**Parameters**: None.  
**Return value**: A non-negative integer representing the total trigger count.  
**Throws**: Does not throw.

### `LastTriggeredAt`

```csharp
public static DateTime? LastTriggeredAt
```

**Purpose**: Returns the timestamp of the most recent trigger event, if any.  
**Parameters**: None.  
**Return value**: A `DateTime?` that is `null` if the alert has never triggered, or the last trigger time in UTC.  
**Throws**: Does not throw.

### `IsEnabled`

```csharp
public static bool IsEnabled
```

**Purpose**: Indicates whether the alert is currently active and permitted to evaluate triggers.  
**Parameters**: None.  
**Return value**: `true` if enabled; `false` if disabled.  
**Throws**: Does not throw.

### `GetDescription`

```csharp
public static string GetDescription
```

**Purpose**: Produces a human-readable description of the alert’s configuration and current state.  
**Parameters**: None.  
**Return value**: A `string` containing the formatted description.  
**Throws**: May throw if internal state required for the description is missing.

### `IsValid` (PriceAlert)

```csharp
public static bool IsValid
```

**Purpose**: Validates the integrity and completeness of a `PriceAlert` instance.  
**Parameters**: None.  
**Return value**: `true` if the alert passes all validation rules; otherwise `false`.  
**Throws**: Does not throw; invalid states are represented by the return value.

### `GetRiskLevel`

```csharp
public static string GetRiskLevel
```

**Purpose**: Computes or retrieves a risk classification label for the current alert context.  
**Parameters**: None.  
**Return value**: A `string` such as `"Low"`, `"Medium"`, or `"High"`.  
**Throws**: May throw if the underlying data necessary for risk calculation is unavailable.

### `IsHighSpread`

```csharp
public static bool IsHighSpread
```

**Purpose**: Tests whether the current spread exceeds the threshold considered “high”.  
**Parameters**: None.  
**Return value**: `true` if the spread is high; otherwise `false`.  
**Throws**: May throw if the spread value is null or the threshold configuration is missing.

### `IsLowSpread`

```csharp
public static bool IsLowSpread
```

**Purpose**: Tests whether the current spread falls below the threshold considered “low”.  
**Parameters**: None.  
**Return value**: `true` if the spread is low; otherwise `false`.  
**Throws**: Same constraints as `IsHighSpread`.

### `IsNormal`

```csharp
public static bool IsNormal
```

**Purpose**: Determines if the alert or spread is in a nominal, non-exceptional state.  
**Parameters**: None.  
**Return value**: `true` if conditions are normal; otherwise `false`.  
**Throws**: May throw if the definition of “normal” cannot be resolved.

### `GetVarianceFromAverage`

```csharp
public static decimal GetVarianceFromAverage
```

**Purpose**: Calculates the deviation of the current value from a computed or stored average.  
**Parameters**: None.  
**Return value**: A `decimal` representing the variance (positive or negative).  
**Throws**: May throw if the average or current value is unavailable.

### `SampleCount`

```csharp
public static long SampleCount
```

**Purpose**: Returns the number of data samples used in statistical calculations.  
**Parameters**: None.  
**Return value**: A `long` count of samples.  
**Throws**: Does not throw.

### `IsValid` (Spread)

```csharp
public static bool IsValid
```

**Purpose**: Validates the integrity of a `Spread` instance.  
**Parameters**: None.  
**Return value**: `true` if the spread is valid; otherwise `false`.  
**Throws**: Does not throw.

### `RecordTrigger`

```csharp
public static PriceAlert RecordTrigger
```

**Purpose**: Simulates a trigger event on the alert, incrementing the trigger count and updating the last triggered timestamp.  
**Parameters**: None.  
**Return value**: The updated `PriceAlert` instance (enables fluent chaining).  
**Throws**: May throw if the alert is in an invalid state that prevents recording.

### `Toggle`

```csharp
public static PriceAlert Toggle
```

**Purpose**: Switches the enabled/disabled state of the alert.  
**Parameters**: None.  
**Return value**: The `PriceAlert` instance with its `IsEnabled` state flipped.  
**Throws**: May throw if the underlying state cannot be mutated.

### `UpdateStatistics`

```csharp
public static Spread UpdateStatistics
```

**Purpose**: Recalculates and updates the statistical properties of a `Spread` (e.g., average, variance, sample count) based on current data.  
**Parameters**: None.  
**Return value**: The updated `Spread` instance.  
**Throws**: May throw if the data set is empty or contains invalid entries.

## Usage

### Example 1: Testing Alert Trigger and Cooldown Behavior

```csharp
[Fact]
public void Alert_ShouldHonorCooldownAfterTrigger()
{
    // Arrange
    var alert = PriceAlertTestsExtensions.CreateTestAlert();
    
    // Act — first trigger
    var triggeredAlert = PriceAlertTestsExtensions.RecordTrigger;
    bool shouldTriggerNow = PriceAlertTestsExtensions.ShouldTrigger;
    bool inCooldown = PriceAlertTestsExtensions.IsInCooldownPeriod;
    
    // Assert
    Assert.True(shouldTriggerNow);           // initial conditions allow trigger
    Assert.False(inCooldown);                // not yet in cooldown before recording
    Assert.Equal(1, PriceAlertTestsExtensions.TriggerCount);
    Assert.NotNull(PriceAlertTestsExtensions.LastTriggeredAt);
    
    // Act — immediate re-evaluation
    bool shouldTriggerAgain = PriceAlertTestsExtensions.ShouldTrigger;
    bool stillInCooldown = PriceAlertTestsExtensions.IsInCooldownPeriod;
    
    // Assert
    Assert.False(shouldTriggerAgain);        // suppressed by cooldown
    Assert.True(stillInCooldown);
}
```

### Example 2: Validating Spread Statistics and Risk Classification

```csharp
[Fact]
public void Spread_ShouldClassifyRiskBasedOnVariance()
{
    // Arrange
    var spread = PriceAlertTestsExtensions.CreateTestSpread();
    var updatedSpread = PriceAlertTestsExtensions.UpdateStatistics;
    
    // Act
    decimal variance = PriceAlertTestsExtensions.GetVarianceFromAverage;
    string riskLevel = PriceAlertTestsExtensions.GetRiskLevel;
    bool isHigh = PriceAlertTestsExtensions.IsHighSpread;
    bool isValid = PriceAlertTestsExtensions.IsValid;
    long samples = PriceAlertTestsExtensions.SampleCount;
    
    // Assert
    Assert.True(isValid);
    Assert.True(samples > 0);
    Assert.NotEqual(0m, variance);
    Assert.NotNull(riskLevel);
    
    // Risk classification should align with spread thresholds
    if (isHigh)
        Assert.Equal("High", riskLevel);
    else if (PriceAlertTestsExtensions.IsLowSpread)
        Assert.Equal("Low", riskLevel);
    else if (PriceAlertTestsExtensions.IsNormal)
        Assert.Equal("Medium", riskLevel);
}
```

## Notes

- **Static nature and test isolation**: All members are static, meaning they do not inherently carry instance state. Tests using these helpers must ensure that any ambient state (e.g., static backing fields, shared configuration) is reset between test runs to avoid cross-test contamination.
- **Null and invalid state handling**: Members like `ShouldTrigger`, `IsInCooldownPeriod`, `GetVarianceFromAverage`, and `GetRiskLevel` may throw if the underlying data they operate on is null or malformed. Always pair them with `IsValid` checks when dealing with potentially incomplete test fixtures.
- **Thread safety**: These static members are not designed for concurrent invocation from multiple threads. In parallel test execution environments, external synchronization or isolation (e.g., per-thread test data) is required to prevent race conditions on shared static state.
- **`RecordTrigger` and `Toggle` mutability**: These methods mutate the alert and return the same instance for fluent use. In test assertions, capture the returned reference to verify state changes; relying on a previously held reference without reassignment may lead to observing stale state.
- **`UpdateStatistics` side effects**: Calling `UpdateStatistics` recalculates internal aggregates. Subsequent calls to `GetVarianceFromAverage`, `SampleCount`, and spread classification properties will reflect the updated values. Ensure the underlying data source is populated before invoking this method.
- **`IsValid` overloads**: There are two `IsValid` members—one for `PriceAlert` and one for `Spread`. The context in which they are called determines which type is validated. Tests should explicitly arrange the correct type before asserting validity.
- **Cooldown logic dependency**: `IsInCooldownPeriod` relies on `LastTriggeredAt` and an internal cooldown duration. If `RecordTrigger` has never been called, `LastTriggeredAt` remains `null` and `IsInCooldownPeriod` should return `false`. Tests that manipulate time (e.g., via `DateTime` abstraction) must account for this dependency.

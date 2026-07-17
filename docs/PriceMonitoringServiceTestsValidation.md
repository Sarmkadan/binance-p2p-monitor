# PriceMonitoringServiceTestsValidation

A static validation utility class that provides extension methods for validating test data used by `PriceMonitoringServiceTests`. It offers validation for `PriceMonitoringServiceTests`, `Price`, and `AppSettings` instances to ensure test data integrity and consistency.

## API

### Validate(PriceMonitoringServiceTests)

Validates the test setup and configuration of a `PriceMonitoringServiceTests` instance.

- **Parameters**: `value` – The `PriceMonitoringServiceTests` instance to validate
- **Returns**: An `IReadOnlyList<string>` containing validation problems; empty if the instance is valid
- **Throws**: `ArgumentNullException` if `value` is `null`

### IsValid(PriceMonitoringServiceTests)

Checks whether a `PriceMonitoringServiceTests` instance is valid.

- **Parameters**: `value` – The `PriceMonitoringServiceTests` instance to check
- **Returns**: `true` if the instance is valid; otherwise, `false`
- **Throws**: `ArgumentNullException` if `value` is `null`

### EnsureValid(PriceMonitoringServiceTests)

Ensures that a `PriceMonitoringServiceTests` instance is valid, throwing an exception if it is not.

- **Parameters**: `value` – The `PriceMonitoringServiceTests` instance to validate
- **Throws**:
  - `ArgumentNullException` if `value` is `null`
  - `ArgumentException` if the instance is invalid, with a message listing all validation problems

### Validate(Price)

Validates that a `Price` instance used in tests is valid.

- **Parameters**: `price` – The `Price` instance to validate
- **Returns**: An `IReadOnlyList<string>` containing validation problems; empty if the instance is valid
- **Throws**: `ArgumentNullException` if `price` is `null`

The validation checks the following conditions:
- `Asset` is not null, empty, or longer than 20 characters
- `Fiat` is not null, empty, or longer than 10 characters
- `BuyPrice` is greater than 0
- `SellPrice` is greater than 0 and not less than `BuyPrice`
- `BuyChangePercent` is between 0 and 100
- `SellChangePercent` is between 0 and 100
- `Timestamp`, `CreatedAt`, and `UpdatedAt` are set to valid `DateTime` values

### IsValid(Price)

Checks whether a `Price` instance is valid.

- **Parameters**: `price` – The `Price` instance to check
- **Returns**: `true` if the instance is valid; otherwise, `false`
- **Throws**: `ArgumentNullException` if `price` is `null`

### EnsureValid(Price)

Ensures that a `Price` instance is valid, throwing an exception if it is not.

- **Parameters**: `price` – The `Price` instance to validate
- **Throws**:
  - `ArgumentNullException` if `price` is `null`
  - `ArgumentException` if the instance is invalid, with a message listing all validation problems

### Validate(AppSettings)

Validates that an `AppSettings` instance used in tests is valid.

- **Parameters**: `settings` – The `AppSettings` instance to validate
- **Returns**: An `IReadOnlyList<string>` containing validation problems; empty if the instance is valid
- **Throws**: `ArgumentNullException` if `settings` is `null`

The validation checks the following conditions:
- `DatabaseConnectionString` is not null, empty, or whitespace
- `MonitoringIntervalSeconds` is greater than 0
- `AlertCooldownMinutes` is greater than 0
- `MaxAlertsPerUser` is greater than 0
- `HistoryRetentionDays` is greater than 0
- `SpreadAnalysisHistoryHours` is greater than 0
- `DefaultPriceChangeThreshold` is not negative
- `DefaultSpreadThreshold` is not negative

### IsValid(AppSettings)

Checks whether an `AppSettings` instance is valid.

- **Parameters**: `settings` – The `AppSettings` instance to check
- **Returns**: `true` if the instance is valid; otherwise, `false`
- **Throws**: `ArgumentNullException` if `settings` is `null`

### EnsureValid(AppSettings)

Ensures that an `AppSettings` instance is valid, throwing an exception if it is not.

- **Parameters**: `settings` – The `AppSettings` instance to validate
- **Throws**:
  - `ArgumentNullException` if `settings` is `null`
  - `ArgumentException` if the instance is invalid, with a message listing all validation problems

## Usage

Example 1: Validating test data before running assertions

```csharp
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Tests;

var price = new Price
{
    Asset = "USDT",
    Fiat = "USD",
    BuyPrice = 1.00m,
    SellPrice = 1.01m,
    BuyChangePercent = 1.5m,
    SellChangePercent = 1.6m,
    Timestamp = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

var validationErrors = price.Validate();
if (validationErrors.Count > 0)
{
    foreach (var error in validationErrors)
    {
        Console.WriteLine(error);
    }
    return;
}

// Proceed with test logic knowing data is valid
```

Example 2: Ensuring configuration validity in test setup

```csharp
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Tests;

var settings = new AppSettings
{
    DatabaseConnectionString = "Server=localhost;Database=testdb;",
    MonitoringIntervalSeconds = 30,
    AlertCooldownMinutes = 5,
    MaxAlertsPerUser = 10,
    HistoryRetentionDays = 30,
    SpreadAnalysisHistoryHours = 24,
    DefaultPriceChangeThreshold = 2.0m,
    DefaultSpreadThreshold = 0.5m
};

settings.EnsureValid();

// Configuration is guaranteed to be valid at this point
```

## Notes

- All validation methods throw `ArgumentNullException` when passed a `null` argument, ensuring null safety.
- Validation methods return `IReadOnlyList<string>` to allow inspection of all problems without early exit, which is useful for test diagnostics.
- `EnsureValid` methods throw `ArgumentException` with a detailed message listing every validation failure, aiding in test debugging.
- The validation logic is synchronous and not designed for high-frequency validation; it is intended for test setup and data integrity checks.
- No thread-safety concerns arise from the public API, as the methods are stateless and operate only on their parameters.
# ConfigurationValidationTests

The `ConfigurationValidationTests` class contains unit tests that verify the behavior of the configuration validation logic used by the Binance P2P monitor application. Each test method exercises a specific validation rule, asserting that the validator either accepts valid settings or throws an appropriate exception for invalid input.

## API

### Validate_ShouldNotThrowException_WhenSettingsAreValid
- **Purpose**: Confirms that the validator does not throw when all configuration settings are within acceptable ranges.
- **Parameters**: None.
- **Return Value**: `void` (the test passes if no exception is thrown; otherwise the test framework throws an assertion exception).
- **When it throws**: Throws if the validator unexpectedly throws an exception for a valid configuration, indicating a regression in validation logic.

### Validate_ShouldThrowException_WhenDatabaseConnectionStringIsInvalid
- **Purpose**: Verifies that an invalid database connection string causes the validator to throw.
- **Parameters**: None.
- **Return Value**: `void` (the test passes if the validator throws; otherwise the test framework throws an assertion exception).
- **When it throws**: Throws if the validator fails to throw for an invalid connection string, indicating missing validation.

### Validate_ShouldThrowException_WhenMonitoringIntervalSecondsIsInvalid
- **Purpose**: Ensures that a monitoring interval outside the allowed range triggers an exception.
- **Parameters**: None.
- **Return Value**: `void` (passes when the validator throws; fails otherwise).
- **When it throws**: Throws if the validator does not react to an invalid interval value.

### Validate_ShouldThrowException_WhenAlertCooldownMinutesIsInvalid
- **Purpose**: Checks that an invalid alert cooldown period results in a validation exception.
- **Parameters**: None.
- **Return Value**: `void` (passes when the validator throws; fails otherwise).
- **When it throws**: Throws if the validator does not detect an invalid cooldown value.

### Validate_ShouldThrowException_WhenMaxAlertsPerUserIsInvalid
- **Purpose**: Asserts that setting a maximum alerts per user outside the permitted bounds causes the validator to throw.
- **Parameters**: None.
- **Return Value**: `void` (passes when the validator throws; fails otherwise).
- **When it throws**: Throws if the validator overlooks an invalid max‑alerts value.

### Validate_ShouldThrowException_WhenHistoryRetentionDaysIsInvalid
- **Purpose**: Validates that an invalid history retention period triggers an exception.
- **Parameters**: None.
- **Return Value**: `void` (passes when the validator throws; fails otherwise).
- **When it throws**: Throws if the validator does not reject an invalid retention value.

### Validate_ShouldThrowException_WhenDefaultPriceChangeThresholdIsNegative
- **Purpose**: Ensures that a negative default price‑change threshold is rejected by the validator.
- **Parameters**: None.
- **Return Value**: `void` (passes when the validator throws; fails otherwise).
- **When it throws**: Throws if the validator accepts a negative threshold.

### Validate_ShouldThrowException_WhenDefaultSpreadThresholdIsNegative
- **Purpose**: Confirms that a negative default spread threshold causes the validator to throw.
- **Parameters**: None.
- **Return Value**: `void` (passes when the validator throws; fails otherwise).
- **When it throws**: Throws if the validator fails to detect a negative spread threshold.

## Usage

The test class is intended to be executed by a unit‑test runner (e.g., xUnit, NUnit, or MSTest). Below are two typical ways to invoke the tests.

### Example 1: Running all tests with a test runner (MSTest syntax)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BinanceP2pMonitor.Tests; // namespace containing ConfigurationValidationTests

[TestClass]
public class ConfigurationValidationTestsRunner
{
    [TestMethod]
    public void RunAllValidationTests()
    {
        var testInstance = new ConfigurationValidationTests();

        // Each method is a separate test; calling them directly mirrors what the test runner does.
        testInstance.Validate_ShouldNotThrowException_WhenSettingsAreValid;
        testInstance.Validate_ShouldThrowException_WhenDatabaseConnectionStringIsInvalid;
        testInstance.Validate_ShouldThrowException_WhenMonitoringIntervalSecondsIsInvalid;
        testInstance.Validate_ShouldThrowException_WhenAlertCooldownMinutesIsInvalid;
        testInstance.Validate_ShouldThrowException_WhenMaxAlertsPerUserIsInvalid;
        testInstance.Validate_ShouldThrowException_WhenHistoryRetentionDaysIsInvalid;
        testInstance.Validate_ShouldThrowException_WhenDefaultPriceChangeThresholdIsNegative;
        testInstance.Validate_ShouldThrowException_WhenDefaultSpreadThresholdIsNegative;
    }
}
```

### Example 2: Executing a single test method manually (useful for debugging)

```csharp
using BinanceP2pMonitor.Tests;

public class DebugRunner
{
    public static void Main()
    {
        var tests = new ConfigurationValidationTests();

        try
        {
            tests.Validate_ShouldThrowException_WhenMonitoringIntervalSecondsIsInvalid;
            Console.WriteLine("Test passed: exception thrown as expected.");
        }
        catch (AssertFailedException ex)
        {
            Console.WriteLine($"Test failed: {ex.Message}");
        }
    }
}
```

## Notes

- The test methods contain no mutable state; they rely only on the static validation logic under test. Consequently, the class is thread‑safe and instances can be created and invoked concurrently without risk of interference.
- Each test expects a specific exception type from the validator (typically `ArgumentException` or a custom validation exception). If the validator’s exception contract changes, the corresponding test will begin to fail, providing an early signal of a breaking change.
- Passing tests do not guarantee that the validator works for every possible input combination; they only cover the individual validation rules listed. Additional edge‑case testing (e.g., boundary values, malformed strings) should be added as needed.
- The test class does not inherit from any framework‑specific base attribute in the provided signatures; however, in the actual repository it is likely decorated with a test class attribute (e.g., `[TestClass]` for MSTest) which enables discovery by test runners. This documentation does not modify or assume any such attributes beyond what is publicly visible.

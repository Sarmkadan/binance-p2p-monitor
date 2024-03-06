# PriceAlertTests

Unit test class for `PriceAlert` functionality, verifying alert triggering logic, statistics updates, and configuration states. Tests cover condition evaluation, cooldown periods, risk level calculations, and toggle behavior to ensure correct alert behavior in the Binance P2P monitoring system.

## API

### `ShouldTrigger_GreaterThanConditionAndValueExceedsThreshold_ReturnsTrue`
Verifies that an alert with a `GreaterThan` condition triggers when the current price exceeds the configured threshold.

### `ShouldTrigger_GreaterThanConditionButValueBelowThreshold_ReturnsFalse`
Ensures that an alert with a `GreaterThan` condition does not trigger when the current price is below the configured threshold.

### `ShouldTrigger_AlertIsDisabled_AlwaysReturnsFalse`
Confirms that a disabled alert never triggers regardless of price conditions.

### `ShouldTrigger_LessThanConditionAndValueBelowThreshold_ReturnsTrue`
Validates that an alert with a `LessThan` condition triggers when the current price is below the configured threshold.

### `ShouldTrigger_EqualsConditionWithinTolerance_ReturnsTrue`
Checks that an alert with an `Equals` condition triggers when the current price is within the specified tolerance of the target price.

### `RecordTrigger_IncrementsTriggerCountAndSetsTimestamp`
Ensures that recording a trigger increments the internal trigger count and updates the last triggered timestamp.

### `IsInCooldownPeriod_NeverTriggered_ReturnsFalse`
Verifies that an alert with no prior triggers is not in a cooldown period.

### `Toggle_EnabledAlert_BecomesDisabled`
Confirms that toggling an enabled alert disables it.

### `Toggle_DisabledAlert_BecomesEnabled`
Confirms that toggling a disabled alert enables it.

### `GetDescription_GreaterThanCondition_ContainsCorrectOperator`
Validates that the alert description includes the correct comparison operator for a `GreaterThan` condition.

### `IsValid_WellFormedAlert_ReturnsTrue`
Checks that a properly configured alert with valid parameters is considered valid.

### `IsValid_EmptyAsset_ReturnsFalse`
Ensures that an alert with an empty asset identifier is considered invalid.

### `GetRiskLevel_VariousSpreadValues_ReturnsCorrectLevel`
Verifies that the risk level is correctly determined based on the current spread value.

### `IsHighSpread_AboveDefaultThreshold_ReturnsTrue`
Confirms that a spread above the default high threshold is correctly identified as high.

### `IsLowSpread_BelowDefaultThreshold_ReturnsTrue`
Confirms that a spread below the default low threshold is correctly identified as low.

### `IsNormal_CurrentWithinMinMax_ReturnsTrue`
Validates that a spread within the configured minimum and maximum bounds is considered normal.

### `IsNormal_CurrentAboveMax_ReturnsFalse`
Ensures that a spread exceeding the maximum bound is not considered normal.

### `GetVarianceFromAverage_ZeroAverage_ReturnsZero`
Checks that when the average price is zero, the variance calculation returns zero to avoid division issues.

### `GetVarianceFromAverage_CurrentDoubleAverage_Returns100Percent`
Verifies that when the current price is double the average, the variance is correctly reported as 100%.

### `UpdateStatistics_NewSample_UpdatesCurrentAndSampleCount`
Ensures that adding a new price sample updates the current price and increments the sample count.

## Usage

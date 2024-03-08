# HistoricalSpreadAnalysisServiceTests

The `HistoricalSpreadAnalysisServiceTests` class contains unit tests for validating the functionality of the `HistoricalSpreadAnalysisService` in the `binance-p2p-monitor` project. These tests verify the correctness of methods responsible for analyzing historical spread data, detecting statistical anomalies, and calculating percentiles and rolling averages. The tests ensure edge cases are handled, such as empty history or invalid inputs, and validate the expected behavior under normal conditions.

## API

### `HistoricalSpreadAnalysisServiceTests`
Initializes a new instance of the test class. This constructor is implicitly called by the test framework to set up test dependencies.

---

### `AnalyzeHistoricalSpreadAsync_ShouldReturnNull_WhenNoHistory`
**Purpose**: Verifies that `AnalyzeHistoricalSpreadAsync` returns `null` when no historical data is available.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `AnalyzeHistoricalSpreadAsync_ShouldReturnReport_WhenHistoryExists`
**Purpose**: Validates that `AnalyzeHistoricalSpreadAsync` generates a valid report when historical data exists.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `DetectStatisticalAlertsAsync_ShouldReturnAnomalies_WhenZScoreExceedsThreshold`
**Purpose**: Ensures that `DetectStatisticalAlertsAsync` identifies anomalies when the Z-score exceeds the defined threshold.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `DetectStatisticalAlertsAsync_ShouldNotReturnAnomalies_WhenZScoreIsBelowThreshold`
**Purpose**: Confirms that `DetectStatisticalAlertsAsync` does not flag anomalies when the Z-score is below the threshold.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `GetSpreadPercentileAsync_ShouldReturnCorrectPercentile`
**Purpose**: Tests that `GetSpreadPercentileAsync` returns the correct percentile value for a given spread dataset.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

---

### `GetSpreadPercentileAsync_ShouldThrowArgumentOutOfRangeException_ForInvalidPercentile`
**Purpose**: Validates that `GetSpreadPercentileAsync` throws an `ArgumentOutOfRangeException` when an invalid percentile (e.g., < 0 or > 100) is provided.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**:
- `ArgumentOutOfRangeException`: If the percentile is outside the valid range (0-100).

---

### `GetRollingWindowAveragesAsync_ShouldReturnEmpty_WhenNoHistory`
**Purpose**: Ensures that `GetRollingWindowAveragesAsync` returns an empty result when no historical data is present.
**Parameters**: None.
**Return Value**: `Task` representing the asynchronous operation.
**Throws**: None.

## Usage

### Example 1: Testing Spread Analysis with Historical Data

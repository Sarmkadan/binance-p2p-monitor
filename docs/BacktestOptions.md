# BacktestOptions

The `BacktestOptions` class encapsulates the configuration parameters required to execute a historical simulation of a trading strategy within the `binance-p2p-monitor` project. It defines the initial capital constraints, entry and exit logic based on spread thresholds, risk management limits such as stop-loss and take-profit levels, and statistical settings for Monte Carlo analysis. This type serves as the primary input for backtesting engines to ensure consistent and reproducible simulation results across different market scenarios.

## API

### InitialEquity
*   **Type:** `decimal`
*   **Description:** Specifies the starting capital amount available for the simulation. This value serves as the baseline for calculating percentage-based returns and drawdowns.

### PositionSizeFraction
*   **Type:** `decimal`
*   **Description:** Determines the proportion of `InitialEquity` allocated to a single trade position. A value of `0.1m` indicates that 10% of the available equity is used for each entry.

### EntrySpreadThreshold
*   **Type:** `decimal`
*   **Description:** Defines the minimum spread difference required to trigger a buy order. The strategy will only enter a position when the market spread meets or exceeds this value.

### ExitSpreadThreshold
*   **Type:** `decimal`
*   **Description:** Defines the spread condition required to close an open position. The strategy will attempt to exit when the spread narrows or widens to meet this specific threshold, depending on the strategy logic.

### StopLossPercent
*   **Type:** `decimal`
*   **Description:** Represents the maximum allowable loss percentage relative to the entry price before a position is forcibly closed to limit downside risk.

### TakeProfitPercent
*   **Type:** `decimal`
*   **Description:** Represents the target gain percentage relative to the entry price at which a position is automatically closed to secure profits.

### TransactionCostPercent
*   **Type:** `decimal`
*   **Description:** Accounts for fees, slippage, or other transaction costs expressed as a percentage of the trade volume. This value is deducted from the profit/loss calculation of every executed trade.

### MaxConcurrentPositions
*   **Type:** `int`
*   **Description:** Sets the upper limit on the number of open positions allowed simultaneously. The backtester will not initiate new trades if this limit is reached.

### LookbackPeriod
*   **Type:** `int`
*   **Description:** Specifies the number of historical data points (e.g., candles or ticks) the strategy analyzes to make entry or exit decisions.

### MonteCarloIterations
*   **Type:** `int`
*   **Description:** Determines the number of random simulation runs performed during Monte Carlo analysis to assess strategy robustness and probability of ruin.

### ConfidenceLevel
*   **Type:** `decimal`
*   **Description:** Sets the statistical confidence interval (e.g., `0.95m` for 95%) used when calculating Value at Risk (VaR) or other probabilistic metrics from the Monte Carlo simulations.

### RandomSeed
*   **Type:** `int?`
*   **Description:** An optional seed value for the random number generator used in Monte Carlo simulations. Providing a specific value ensures deterministic and reproducible results; if `null`, a system-generated seed is used.

### VolatilityScaleFactor
*   **Type:** `decimal`
*   **Description:** A multiplier applied to historical volatility data during stress testing or scenario analysis to simulate market conditions with higher or lower variance than observed historically.

### Validate
*   **Signature:** `public void Validate()`
*   **Description:** Performs integrity checks on all configuration properties to ensure they contain logical and safe values before a backtest begins.
*   **Parameters:** None.
*   **Return Value:** None.
*   **Exceptions:** Throws an `InvalidOperationException` or `ArgumentOutOfRangeException` if any property contains an invalid state (e.g., negative equity, percentages outside 0-1 range, or contradictory thresholds).

## Usage

### Example 1: Standard Configuration
The following example demonstrates initializing a standard backtest configuration with fixed risk parameters and a deterministic random seed for reproducibility.

```csharp
var options = new BacktestOptions
{
    InitialEquity = 10000.00m,
    PositionSizeFraction = 0.20m,
    EntrySpreadThreshold = 0.005m,
    ExitSpreadThreshold = 0.002m,
    StopLossPercent = 0.02m,
    TakeProfitPercent = 0.05m,
    TransactionCostPercent = 0.001m,
    MaxConcurrentPositions = 3,
    LookbackPeriod = 50,
    MonteCarloIterations = 1000,
    ConfidenceLevel = 0.95m,
    RandomSeed = 42,
    VolatilityScaleFactor = 1.0m
};

// Validate configuration before passing to the engine
options.Validate();

// Proceed with backtest execution...
```

### Example 2: Stress Testing Scenario
This example configures the options for a high-volatility stress test, increasing the volatility scale factor and disabling the random seed to generate unique scenarios for each run.

```csharp
var stressTestOptions = new BacktestOptions
{
    InitialEquity = 50000.00m,
    PositionSizeFraction = 0.10m,
    EntrySpreadThreshold = 0.010m,
    ExitSpreadThreshold = 0.004m,
    StopLossPercent = 0.03m,
    TakeProfitPercent = 0.06m,
    TransactionCostPercent = 0.002m,
    MaxConcurrentPositions = 1,
    LookbackPeriod = 100,
    MonteCarloIterations = 5000,
    ConfidenceLevel = 0.99m,
    RandomSeed = null, // Non-deterministic for varied scenarios
    VolatilityScaleFactor = 2.5m // Simulate 2.5x normal volatility
};

stressTestOptions.Validate();
```

## Notes

*   **Validation Logic:** The `Validate` method must be called explicitly before passing the instance to any backtesting engine. Failure to do so may result in runtime errors during simulation if properties contain illogical values (e.g., `StopLossPercent` greater than `TakeProfitPercent`, or `PositionSizeFraction` greater than 1).
*   **Decimal Precision:** All monetary and percentage fields utilize the `decimal` type to prevent floating-point rounding errors common in financial calculations. Ensure literals are suffixed with `m` when assigning values.
*   **Thread Safety:** The `BacktestOptions` class is not thread-safe. While the properties are simple value types, the object state should not be modified concurrently by multiple threads. It is recommended to instantiate a separate `BacktestOptions` object for each parallel backtest run or to treat the instance as immutable after initialization and validation.
*   **Monte Carlo Determinism:** Setting `RandomSeed` to a specific integer guarantees that the sequence of random events generated during `MonteCarloIterations` is identical across runs. Leaving this property `null` is appropriate for production stress testing where diverse outcomes are desired, but it hinders debugging specific failure cases.
*   **Volatility Scaling:** The `VolatilityScaleFactor` does not alter the raw input data but acts as a multiplier within the simulation logic. A factor of `0m` may effectively neutralize price movement depending on the engine implementation, while values significantly greater than `1.0m` may trigger stop-losses more frequently.

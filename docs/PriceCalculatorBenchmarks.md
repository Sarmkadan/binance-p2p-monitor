# PriceCalculatorBenchmarks

The `PriceCalculatorBenchmarks` class serves as a dedicated harness for performance testing and validation of pricing algorithms within the `binance-p2p-monitor` project. It encapsulates a suite of methods designed to measure the execution time and accuracy of critical financial calculations, including spread determination, percentage changes, moving averages over varying periods and dataset sizes, standard deviation, and price formatting routines. This type is typically instantiated by benchmarking frameworks to isolate and stress-test specific computational paths without the overhead of external I/O or network dependencies.

## API

### `Setup`
Initializes the internal state required for subsequent benchmark executions. This method typically populates test datasets, configures decimal precision contexts, or prepares mock market data structures.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: May throw initialization exceptions if underlying data structures fail to allocate, though specific exception types depend on the internal implementation.

### `CalculateSpread`
Computes the difference between the highest bid and lowest ask prices (or equivalent buy/sell metrics) to determine the market spread.
*   **Parameters**: None (operates on prepared internal state).
*   **Returns**: `decimal` representing the calculated spread value.
*   **Throws**: May throw if the internal price data is uninitialized or invalid (e.g., negative prices).

### `PercentageChange`
Calculates the percentage change between two price points, typically representing the shift from an opening to a closing price or between consecutive ticks.
*   **Parameters**: None (operates on prepared internal state).
*   **Returns**: `decimal` representing the percentage change.
*   **Throws**: May throw `DivideByZeroException` if the base price used for calculation is zero.

### `MovingAverage_Period20_N1000`
Executes a moving average calculation with a window period of 20 over a dataset size of 1,000 entries.
*   **Parameters**: None.
*   **Returns**: `decimal` representing the resulting moving average.
*   **Throws**: May throw if the internal dataset contains fewer than 20 entries or is null.

### `MovingAverage_Period200_N1000`
Executes a moving average calculation with a window period of 200 over a dataset size of 1,000 entries.
*   **Parameters**: None.
*   **Returns**: `decimal` representing the resulting moving average.
*   **Throws**: May throw if the internal dataset contains fewer than 200 entries or is null.

### `MovingAverage_Period20_N50`
Executes a moving average calculation with a window period of 20 over a dataset size of 50 entries.
*   **Parameters**: None.
*   **Returns**: `decimal` representing the resulting moving average.
*   **Throws**: May throw if the internal dataset contains fewer than 20 entries or is null.

### `StandardDeviation_N1000`
Calculates the standard deviation for a dataset comprising 1,000 price entries to measure volatility.
*   **Parameters**: None.
*   **Returns**: `decimal` representing the standard deviation.
*   **Throws**: May throw if the dataset is insufficient for statistical calculation or contains invalid numeric values.

### `StandardDeviation_N50`
Calculates the standard deviation for a dataset comprising 50 price entries.
*   **Parameters**: None.
*   **Returns**: `decimal` representing the standard deviation.
*   **Throws**: May throw if the dataset is insufficient for statistical calculation.

### `FormatPrice_NoSymbol`
Formats a decimal price value into a string representation without appending any currency symbol.
*   **Parameters**: None (operates on a prepared internal price value).
*   **Returns**: `string` containing the formatted numeric value.
*   **Throws**: Unlikely to throw unless culture-specific formatting fails internally.

### `FormatPrice_WithSymbol`
Formats a decimal price value into a string representation including the appropriate currency symbol (e.g., USDT).
*   **Parameters**: None (operates on a prepared internal price value).
*   **Returns**: `string` containing the formatted numeric value and symbol.
*   **Throws**: Unlikely to throw unless culture-specific formatting fails internally.

## Usage

### Example 1: Initializing and Running a Spread Benchmark
This example demonstrates how to instantiate the benchmark class, initialize the test data, and execute the spread calculation method.

```csharp
using BinanceP2PMonitor.Benchmarks;

public class BenchmarkRunner
{
    public void RunSpreadTest()
    {
        var benchmarks = new PriceCalculatorBenchmarks();
        
        // Prepare the internal state with mock data
        benchmarks.Setup();
        
        // Execute the specific benchmark method
        decimal spread = benchmarks.CalculateSpread();
        
        Console.WriteLine($"Calculated Spread: {spread}");
    }
}
```

### Example 2: Comparing Moving Average Performance Across Datasets
This example illustrates running multiple moving average benchmarks to compare performance implications of different dataset sizes and periods.

```csharp
using BinanceP2PMonitor.Benchmarks;

public class PerformanceAnalysis
{
    public void AnalyzeMovingAverages()
    {
        var benchmarks = new PriceCalculatorBenchmarks();
        benchmarks.Setup();

        // Execute benchmarks for different configurations
        decimal ma20Small = benchmarks.MovingAverage_Period20_N50();
        decimal ma20Large = benchmarks.MovingAverage_Period20_N1000();
        decimal ma200Large = benchmarks.MovingAverage_Period200_N1000();

        Console.WriteLine($"MA(20, N=50): {ma20Small}");
        Console.WriteLine($"MA(20, N=1000): {ma20Large}");
        Console.WriteLine($"MA(200, N=1000): {ma200Large}");
    }
}
```

## Notes

*   **State Dependency**: All calculation methods (`CalculateSpread`, `PercentageChange`, `MovingAverage_*`, `StandardDeviation_*`, `FormatPrice_*`) rely on the internal state being correctly initialized. Calling these methods before invoking `Setup` will likely result in `NullReferenceException` or logical errors due to missing data.
*   **Thread Safety**: This class is not guaranteed to be thread-safe. The `Setup` method mutates internal state, and the calculation methods read from this shared state. If multiple threads access the same instance concurrently, external synchronization (e.g., `lock` statements) is required to prevent race conditions. It is recommended to create a new instance per thread or per test run.
*   **Edge Cases**: 
    *   Division by zero may occur in `PercentageChange` if the reference price in the prepared dataset is zero.
    *   Moving average and standard deviation methods may throw exceptions if the `Setup` method fails to populate the dataset with the minimum required number of elements (e.g., requesting a period of 200 on a dataset of 50).
*   **Precision**: Return types are strictly `decimal` to maintain financial precision, avoiding floating-point rounding errors common in `double` arithmetic.

# PriceCalculatorBenchmarksExtensions

The `PriceCalculatorBenchmarksExtensions` class provides a set of static utility methods designed to perform statistical analysis and formatting operations on decimal price data, specifically tailored for benchmarking scenarios within the Binance P2P monitoring system. It facilitates the calculation of moving average percentage changes, spread metrics relative to moving averages, formatted price output, and basic statistical distributions (mean and standard deviation) for spread data, enabling precise performance evaluation and data normalization without requiring external dependencies.

## API

### `CalculateMovingAveragePercentageChange`
Computes the percentage change between a current price value and a calculated moving average.
- **Parameters**: Accepts decimal values representing the current price and the moving average.
- **Returns**: A `decimal` representing the percentage change.
- **Throws**: Throws `DivideByZeroException` if the moving average provided is zero.

### `CalculateSpreadWithMovingAverage`
Determines the absolute spread (difference) between a specific price point and its corresponding moving average.
- **Parameters**: Accepts decimal values for the target price and the moving average.
- **Returns**: A `decimal` representing the calculated spread.
- **Throws**: No specific exceptions expected under normal arithmetic operations.

### `FormatPriceWithMovingAverage`
Generates a string representation of a price alongside its moving average context, typically used for logging or display in benchmark reports.
- **Parameters**: Accepts decimal values for the price and the moving average.
- **Returns**: A `string` containing the formatted output.
- **Throws**: No specific exceptions expected.

### `CalculateSpreadStatistics`
Analyzes a collection of spread values to determine central tendency and dispersion.
- **Parameters**: Accepts a collection (e.g., `IEnumerable<decimal>`) of spread values.
- **Returns**: A tuple `(decimal mean, decimal stdDev)` containing the arithmetic mean and the standard deviation of the provided dataset.
- **Throws**: May throw `ArgumentException` or similar collection-related exceptions if the input collection is null or empty, as statistical calculation requires data points.

## Usage

**Example 1: Calculating spread metrics and statistics for a dataset**
This example demonstrates how to compute the spread for individual data points and then aggregate those spreads to find the mean and standard deviation.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public class BenchmarkAnalysis
{
    public static void Run()
    {
        decimal currentPrice = 45000.50m;
        decimal movingAverage = 44800.00m;
        
        // Calculate individual spread
        decimal spread = PriceCalculatorBenchmarksExtensions.CalculateSpreadWithMovingAverage(
            currentPrice, 
            movingAverage
        );

        // Simulate a collection of historical spreads
        var historicalSpreads = new List<decimal> { 150.2m, 200.5m, 180.0m, 210.3m, 195.4m };

        // Calculate aggregate statistics
        var stats = PriceCalculatorBenchmarksExtensions.CalculateSpreadStatistics(historicalSpreads);
        
        Console.WriteLine($"Current Spread: {spread}");
        Console.WriteLine($"Mean Spread: {stats.mean}, StdDev: {stats.stdDev}");
    }
}
```

**Example 2: Formatting output and calculating percentage deviation**
This example shows how to format price data for reporting and calculate the percentage deviation from the moving average.

```csharp
using System;

public class ReportGenerator
{
    public static void GenerateSnapshot()
    {
        decimal price = 32500.75m;
        decimal ma = 32450.00m;

        // Format the display string
        string reportLine = PriceCalculatorBenchmarksExtensions.FormatPriceWithMovingAverage(price, ma);
        
        // Calculate percentage change
        decimal percentChange = PriceCalculatorBenchmarksExtensions.CalculateMovingAveragePercentageChange(price, ma);

        Console.WriteLine(reportLine);
        Console.WriteLine($"Deviation: {percentChange}%");
    }
}
```

## Notes

- **Division by Zero**: The `CalculateMovingAveragePercentageChange` method involves division by the moving average. Callers must ensure the moving average value is non-zero to prevent runtime `DivideByZeroException`.
- **Empty Collections**: The `CalculateSpreadStatistics` method requires a valid dataset to compute mean and standard deviation. Passing null or empty collections will likely result in an exception; ensure data validation prior to invocation.
- **Thread Safety**: As this class consists entirely of static methods that operate on primitive `decimal` types and input parameters without maintaining internal mutable state, it is inherently thread-safe. Multiple threads can safely invoke these methods concurrently without synchronization.
- **Precision**: All calculations utilize the C# `decimal` type to maintain high precision suitable for financial data, avoiding floating-point rounding errors common with `double`.

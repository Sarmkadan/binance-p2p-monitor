// entire file content ...
// ... goes in between

## PriceCalculatorBenchmarks

The `PriceCalculatorBenchmarks` class provides a set of benchmarking methods for evaluating the performance of the `PriceCalculator` class. It includes methods for calculating spread, percentage change, moving averages, and standard deviation.

### Usage

```csharp
using BinanceP2pMonitor.Benchmarks;

// Create a new instance of PriceCalculatorBenchmarks
var priceCalculatorBenchmarks = new PriceCalculatorBenchmarks();

// Setup the benchmark
priceCalculatorBenchmarks.Setup();

// Calculate the spread
var spread = priceCalculatorBenchmarks.CalculateSpread();

// Calculate the percentage change
var percentageChange = priceCalculatorBenchmarks.PercentageChange();

// Calculate the moving average for a period of 20 with 1000 samples
var movingAveragePeriod20N1000 = priceCalculatorBenchmarks.MovingAverage_Period20_N1000;

// Calculate the moving average for a period of 200 with 1000 samples
var movingAveragePeriod200N1000 = priceCalculatorBenchmarks.MovingAverage_Period200_N1000;

// Calculate the moving average for a period of 20 with 50 samples
var movingAveragePeriod20N50 = priceCalculatorBenchmarks.MovingAverage_Period20_N50;

// Calculate the standard deviation for 1000 samples
var standardDeviationN1000 = priceCalculatorBenchmarks.StandardDeviation_N1000;

// Calculate the standard deviation for 50 samples
var standardDeviationN50 = priceCalculatorBenchmarks.StandardDeviation_N50;

// Format a price without a symbol
var formattedPriceNoSymbol = priceCalculatorBenchmarks.FormatPrice_NoSymbol;

// Format a price with a symbol
var formattedPriceWithSymbol = priceCalculatorBenchmarks.FormatPrice_WithSymbol;
```

// ... rest of file content ...
```
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

## StringExtensionsBenchmarks

The `StringExtensionsBenchmarks` class provides performance benchmarks for various string extension methods that handle common text transformations and parsing operations. It includes methods for converting between different string cases, truncating text, parsing numeric values, and masking sensitive data.

### Usage

```csharp
using BinanceP2pMonitor.Benchmarks;
using BinanceP2pMonitor.Utilities;

// Create a class instance (no setup required)
var benchmarks = new StringExtensionsBenchmarks();

// Split camel case text into separate words
var camelCaseText = "BinancePriceMonitoringService";
var splitText = camelCaseText.SplitCamelCase(); // Returns "Binance Price Monitoring Service"

// Convert text to snake case
var pascalText = "BinancePriceMonitoringService";
var snakeText = pascalText.ToSnakeCase(); // Returns "binance_price_monitoring_service"

// Convert text to pascal case
var snakeText2 = "binance_price_monitoring_service";
var pascalText2 = snakeText2.ToPascalCase(); // Returns "BinancePriceMonitoringService"

// Truncate a long string to fit display constraints
var longText = "This is a very long text that needs to be shortened for display purposes";
var truncated = longText.Truncate(30); // Returns truncated string
var notTruncated = longText.Truncate(500); // Returns original string unchanged

// Parse a decimal value from a string
var decimalText = "42345.6789";
var decimalValue = decimalText.ToDecimalOrNull(); // Returns 42345.6789m
var invalidDecimal = "not-a-number".ToDecimalOrNull(); // Returns null

// Parse an integer value from a string
var intText = "98765";
var intValue = intText.ToIntOrNull(); // Returns 98765

// Mask sensitive data (shows first 4 characters)
var apiKey = "sk-live-abcdefghijklmnopqrstuvwxyz";
var maskedKey = apiKey.Mask(4); // Returns "sk-l-*******
```

// ... rest of file content ...
```
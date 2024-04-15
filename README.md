// entire file content ...
// ... goes in between

## PriceCalculatorTestsExtensions

The `PriceCalculatorTestsExtensions` class provides utility methods for generating test price sequences, calculating price changes, and analyzing spreads. It includes methods for generating linear and exponential price sequences, calculating cumulative percentage changes, and formatting price arrays.

### Usage

```csharp
using BinanceP2pMonitor.Tests;

// Generate a linear price sequence
var linearPrices = PriceCalculatorTestsExtensions.GenerateLinearPriceSequence(10, 100.0m, 10.0m);
Console.WriteLine($"Linear Price Sequence: [{string.Join(", ", linearPrices)}]");

// Generate an exponential price sequence
var exponentialPrices = PriceCalculatorTestsExtensions.GenerateExponentialPriceSequence(10, 100.0m, 1.1m);
Console.WriteLine($"Exponential Price Sequence: [{string.Join(", ", exponentialPrices)}]");

// Calculate cumulative percentage change
var cumulativeChange = PriceCalculatorTestsExtensions.CalculateCumulativePercentageChange(linearPrices);
Console.WriteLine($"Cumulative Percentage Change: {cumulativeChange:P}");

// Calculate average spread
var buyPrices = new decimal[] { 100.0m, 120.0m, 110.0m };
var sellPrices = new decimal[] { 90.0m, 100.0m, 95.0m };
var averageSpread = PriceCalculatorTestsExtensions.CalculateAverageSpread(buyPrices, sellPrices);
Console.WriteLine($"Average Spread: {averageSpread:P}");

// Generate a volatile price sequence
var volatilePrices = PriceCalculatorTestsExtensions.GenerateVolatilePriceSequence(10, 100.0m, 10.0m);
Console.WriteLine($"Volatile Price Sequence: [{string.Join(", ", volatilePrices)}]");

// Format a price array
var formattedPrices = PriceCalculatorTestsExtensions.FormatPriceArray(linearPrices);
Console.WriteLine($"Formatted Price Array: {formattedPrices}");

// Check if a value is within tolerance
var isWithinTolerance = PriceCalculatorTestsExtensions.ShouldBeWithinTolerance(100.0m, 105.0m, 5.0m);
Console.WriteLine($"Is Within Tolerance: {isWithinTolerance}");

// Generate spread test cases
var testCases = PriceCalculatorTestsExtensions.GenerateSpreadTestCases(10, 100.0m, 10.0m);
Console.WriteLine($"Spread Test Cases: [{string.Join(", ", testCases)}]");
```

// ... rest of file content ...

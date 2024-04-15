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

## PriceAlertTestsExtensions

`PriceAlertTestsExtensions` supplies helper methods for creating and manipulating `PriceAlert` and `Spread` objects in unit‑tests. It lets you quickly build test alerts, evaluate trigger conditions, toggle alert state, and inspect statistical properties such as variance, sample count and risk level.

### Usage

```csharp
using System;
using BinanceP2pMonitor.Tests;

// Create a test alert and a corresponding spread
var alert = PriceAlertTestsExtensions.CreateTestAlert();
var spread = PriceAlertTestsExtensions.CreateTestSpread();

// Inspect basic properties
Console.WriteLine($"Alert enabled: {PriceAlertTestsExtensions.IsEnabled(alert)}");
Console.WriteLine($"Alert description: {PriceAlertTestsExtensions.GetDescription(alert)}");
Console.WriteLine($"Spread risk level: {PriceAlertTestsExtensions.GetRiskLevel(spread)}");

// Determine whether the alert should fire for the current spread
if (PriceAlertTestsExtensions.ShouldTrigger(alert, spread))
{
    // Record the trigger and display updated statistics
    alert = PriceAlertTestsExtensions.RecordTrigger(alert);
    Console.WriteLine($"Alert triggered {PriceAlertTestsExtensions.TriggerCount(alert)} time(s).");
    Console.WriteLine($"Last triggered at: {PriceAlertTestsExtensions.LastTriggeredAt(alert)}");
}

// Example of toggling the alert state and checking cooldown
alert = PriceAlertTestsExtensions.Toggle(alert);
Console.WriteLine($"Alert now enabled: {PriceAlertTestsExtensions.IsEnabled(alert)}");
Console.WriteLine($"In cooldown period: {PriceAlertTestsExtensions.IsInCooldownPeriod(alert)}");

// Update spread statistics and query derived values
spread = PriceAlertTestsExtensions.UpdateStatistics(spread);
Console.WriteLine($"Spread is high: {PriceAlertTestsExtensions.IsHighSpread(spread)}");
Console.WriteLine($"Variance from average: {PriceAlertTestsExtensions.GetVarianceFromAverage(spread):P}");
Console.WriteLine($"Sample count: {PriceAlertTestsExtensions.SampleCount(spread)}");
```

## PriceRepositoryTestsExtensions

The `PriceRepositoryTestsExtensions` class provides utility methods for testing price repository functionality. It includes methods for getting a price repository, adding prices, and retrieving prices by ID or asset and fiat.

### Usage

```csharp
using BinanceP2pMonitor.Tests;

// Get a price repository
var priceRepository = PriceRepositoryTestsExtensions.GetPriceRepository();

// Add a price and get its ID
var addedPriceId = await PriceRepositoryTestsExtensions.AddAsync_ShouldReturnValidIdAndPersist(priceRepository, new Price());

// Get a price by ID
var price = await PriceRepositoryTestsExtensions.GetByIdAsync_ShouldReturnValidPrice(priceRepository, addedPriceId);

// Get the average price
var averagePrice = await PriceRepositoryTestsExtensions.GetAveragePriceAsync_ShouldReturnNull_WhenNoPricesInTimeRange(priceRepository, DateTime.Now, DateTime.Now.AddHours(-1));
```

// ... rest of file content ...

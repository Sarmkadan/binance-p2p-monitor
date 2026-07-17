# PriceCalculatorEdgeCaseTestsExtensions

Static helper class that provides reusable test data and assertion utilities for unit‑testing the price‑calculation logic in the Binance P2P monitor. The members are intended to be consumed by xUnit test classes to drive theory‑based tests and to verify specific edge‑case behaviours without duplicating test setup code.

## API

### `public static TheoryData<decimal, decimal, decimal> CreatePercentageChangeTestData`
- **Purpose**: Supplies a collection of test cases for the percentage‑change calculation.
- **Parameters**: None.
- **Return Value**: A `TheoryData<decimal, decimal, decimal>` where each row contains `(originalPrice, newPrice, expectedPercentageChange)`.
- **Throws**: Does not throw; returns a pre‑populated immutable collection.

### `public static TheoryData<decimal, decimal, decimal> CreateSpreadTestData`
- **Purpose**: Supplies a collection of test cases for the spread calculation (sell price minus buy price).
- **Parameters**: None.
- **Return Value**: A `TheoryData<decimal, decimal, decimal>` where each row contains `(buyPrice, sellPrice, expectedSpread)`.
- **Throws**: Does not throw.

### `public static TheoryData<int, decimal> CreateMovingAverageTestData`
- **Purpose**: Supplies a collection of test cases for the moving‑average calculation.
- **Parameters**: None.
- **Return Value**: A `TheoryData<int, decimal>` where each row contains `(sampleCount, expectedMovingAverage)` assuming a predefined input set.
- **Throws**: Does not throw.

### `public static TheoryData<string, decimal> CreateStandardDeviationTestData`
- **Purpose**: Supplies a collection of test cases for the standard‑deviation calculation.
- **Parameters**: None.
- **Return Value**: A `TheoryData<string, decimal>` where each row contains `(dataSetIdentifier, expectedStandardDeviation)`. The identifier maps to a fixed internal data set used by the method under test.
- **Throws**: Does not throw.

### `public static void ShouldThrowWhenPricesIsNull`
- **Purpose**: Asserts that the price‑calculation method under test throws an `ArgumentNullException` when the supplied price collection is `null`.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Fails the test if the method does not throw the expected exception.

### `public static void ShouldReturnZeroForEmptyCollection`
- **Purpose**: Asserts that the price‑calculation method under test returns zero when supplied with an empty price collection.
- **Parameters**: None.
- **Return Value**: None.
- **Throws**: Fails the test if the returned value is not zero.

### `public static IEnumerable<(decimal Original, decimal NewPrice, decimal Expected)> GetPercentageChangeEdgeCases`
- **Purpose**: Provides edge‑case tuples for percentage‑change scenarios, such as zero original price, maximal decimal values, and negative inputs.
- **Parameters**: None.
- **Return Value**: An enumerable of `(Original, NewPrice, Expected)` tuples.
- **Throws**: Does not throw.

### `public static IEnumerable<(decimal BuyPrice, decimal SellPrice, decimal Expected)> GetSpreadEdgeCases`
- **Purpose**: Provides edge‑case tuples for spread scenarios, including inverted prices (sell < buy), zero values, and extreme decimal limits.
- **Parameters**: None.
- **Return Value**: An enumerable of `(BuyPrice, SellPrice, Expected)` tuples.
- **Throws**: Does not throw.

## Usage

```csharp
using Xunit;
using BinanceP2PMonitor.Tests.Extensions; // namespace containing PriceCalculatorEdgeCaseTestsExtensions

public class PriceCalculatorTests
{
    [Theory]
    [MemberData(nameof(PriceCalculatorEdgeCaseTestsExtensions.CreatePercentageChangeTestData))]
    public void CalculatePercentageChange_ReturnsExpectedResult(decimal original, decimal newPrice, decimal expected)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act
        var result = calculator.CalculatePercentageChange(original, newPrice);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateSpread_ThrowsOnNullPrices()
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act & Assert
        PriceCalculatorEdgeCaseTestsExtensions.ShouldThrowWhenPricesIsNull();
        // The helper internally invokes the method under test with a null argument
        // and verifies that an ArgumentNullException is thrown.
    }
}
```

```csharp
using Xunit;
using System.Collections.Generic;

public class EdgeCaseValidationTests
{
    [Theory]
    [MemberData(nameof(PriceCalculatorEdgeCaseTestsExtensions.GetPercentageChangeEdgeCases))]
    public void CalculatePercentageChange_HandlesEdgeCases(decimal original, decimal newPrice, decimal expected)
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act
        var result = calculator.CalculatePercentageChange(original, newPrice);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateSpread_ReturnsZeroForEmptyCollection()
    {
        // Arrange
        var calculator = new PriceCalculator();

        // Act & Assert
        PriceCalculatorEdgeCaseTestsExtensions.ShouldReturnZeroForEmptyCollection();
        // The helper verifies that the method returns 0 when given an empty list.
    }
}
```

## Notes

- The `TheoryData` properties`TheoryData` members are initialized with static, immutable data; they are safe for concurrent read access across multiple test threads.
- Edge‑case enumerables include values such as `0`, `decimal.MinValue`, `decimal.MaxValue`, and combinations that produce overflow or underflow in intermediate calculations; the consuming tests should verify that the implementation handles these gracefully (e.g., by returning appropriate results or throwing documented exceptions).
- The void assertion helpers (`ShouldThrowWhenPricesIsNull` and `ShouldReturnZeroForEmptyCollection`) encapsulate the arrange‑act‑assert pattern for those specific scenarios; they rely on the test framework to record failures if the expected behaviour is not observed.
- No member modifies external state; the class is thread‑safe for use in parallel test execution.

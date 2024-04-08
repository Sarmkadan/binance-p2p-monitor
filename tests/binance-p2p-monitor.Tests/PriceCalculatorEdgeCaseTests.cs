#nullable enable
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains edge case unit tests for the <see cref="PriceCalculator"/> utility methods.
/// </summary>
public class PriceCalculatorEdgeCaseTests
{
    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculatePercentageChange"/> returns <c>0</c>
    /// when the original price is zero.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculatePercentageChange_ShouldReturnZero_WhenOriginalPriceIsZero()
    {
        // Act
        var result = PriceCalculator.CalculatePercentageChange(0m, 100m);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculatePercentageChange"/> returns the expected
    /// percentage change for various original and new price values.
    /// </summary>
    /// <param name="original">The original price.</param>
    /// <param name="new">The new price.</param>
    /// <param name="expected">The expected percentage change.</param>
    /// <returns>Nothing.</returns>
    [Theory]
    [InlineData(100, 110, 10)]
    [InlineData(100, 90, -10)]
    [InlineData(50, 100, 100)]
    [InlineData(100, 0, -100)]
    public void CalculatePercentageChange_ShouldReturnCorrectChange_ForVariousValues(decimal original, decimal @new, decimal expected)
    {
        // Act
        var result = PriceCalculator.CalculatePercentageChange(original, @new);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateSpread"/> returns <c>0</c>
    /// when the buy price is zero.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateSpread_ShouldReturnZero_WhenBuyPriceIsZero()
    {
        // Act
        var result = PriceCalculator.CalculateSpread(0m, 100m);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateSpread"/> returns the expected spread
    /// for various buy and sell price values.
    /// </summary>
    /// <param name="buy">The buy price.</param>
    /// <param name="sell">The sell price.</param>
    /// <param name="expected">The expected spread.</param>
    /// <returns>Nothing.</returns>
    [Theory]
    [InlineData(100, 102, 2)]
    [InlineData(100, 98, -2)]
    [InlineData(0, 0, 0)]
    public void CalculateSpread_ShouldReturnCorrectSpread_ForVariousPrices(decimal buy, decimal sell, decimal expected)
    {
        // Act
        var result = PriceCalculator.CalculateSpread(buy, sell);

        // Assert
        result.Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateMovingAverage"/> returns <c>0</c>
    /// when the input price collection is empty.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateMovingAverage_ShouldReturnZero_WhenPricesIsEmpty()
    {
        // Arrange
        var prices = new List<decimal>();

        // Act
        var result = PriceCalculator.CalculateMovingAverage(prices, 5);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateMovingAverage"/> throws an
    /// <see cref="ArgumentNullException"/> when the <c>prices</c> collection is <c>null</c>.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateMovingAverage_ShouldThrowArgumentNullException_WhenPricesIsNull()
    {
        // Arrange
        IEnumerable<decimal> prices = null!;

        // Act
        Action action = () => PriceCalculator.CalculateMovingAverage(prices, 5);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Prices collection cannot be null (Parameter 'prices')");
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateMovingAverage"/> throws an
    /// <see cref="ArgumentOutOfRangeException"/> when the <c>period</c> argument is zero or negative.
    /// </summary>
    /// <param name="period">The period value to test (zero or negative).</param>
    /// <returns>Nothing.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateMovingAverage_ShouldThrowArgumentOutOfRangeException_WhenPeriodIsZeroOrNegative(int period)
    {
        // Arrange
        var prices = new List<decimal> { 1, 2, 3, 4, 5 };

        // Act
        Action action = () => PriceCalculator.CalculateMovingAverage(prices, period);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("Period must be greater than zero*");
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateMovingAverage"/> returns the correct average
    /// when the requested period is greater than the number of price entries.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateMovingAverage_ShouldReturnCorrectAverage_WhenPeriodGreaterThanCount()
    {
        // Arrange
        var prices = new List<decimal> { 1, 2, 3 };

        // Act
        var result = PriceCalculator.CalculateMovingAverage(prices, 5);

        // Assert
        result.Should().Be(2m); // (1+2+3)/3
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateStandardDeviation"/> returns <c>0</c>
    /// for empty or single‑item price collections.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateStandardDeviation_ShouldReturnZero_WhenPricesIsEmptyOrSingleItem()
    {
        // Arrange
        var emptyPrices = new List<decimal>();
        var singlePrice = new List<decimal> { 100 };

        // Act & Assert
        PriceCalculator.CalculateStandardDeviation(emptyPrices).Should().Be(0m);
        PriceCalculator.CalculateStandardDeviation(singlePrice).Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateStandardDeviation"/> throws an
    /// <see cref="ArgumentNullException"/> when the <c>prices</c> collection is <c>null</c>.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateStandardDeviation_ShouldThrowArgumentNullException_WhenPricesIsNull()
    {
        // Arrange
        IEnumerable<decimal> prices = null!;

        // Act
        Action action = () => PriceCalculator.CalculateStandardDeviation(prices);

        // Assert
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Prices collection cannot be null (Parameter 'prices')");
    }

    /// <summary>
    /// Verifies that <see cref="PriceCalculator.CalculateStandardDeviation"/> returns the correct
    /// standard deviation for a known data set.
    /// </summary>
    /// <returns>Nothing.</returns>
    [Fact]
    public void CalculateStandardDeviation_ShouldReturnCorrectStandardDeviation()
    {
        // Arrange
        var prices = new List<decimal> { 2, 4, 4, 4, 5, 5, 7, 9 };

        // Act
        var result = PriceCalculator.CalculateStandardDeviation(prices);

        // Assert
        result.Should().BeApproximately(2m, 0.001m); // Expected approximately 2.0
    }
}

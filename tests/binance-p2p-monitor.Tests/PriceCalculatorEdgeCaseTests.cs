#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class PriceCalculatorEdgeCaseTests
{
    [Fact]
    public void CalculatePercentageChange_ShouldReturnZero_WhenOriginalPriceIsZero()
    {
        // Act
        var result = PriceCalculator.CalculatePercentageChange(0m, 100m);

        // Assert
        result.Should().Be(0m);
    }

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

    [Fact]
    public void CalculateSpread_ShouldReturnZero_WhenBuyPriceIsZero()
    {
        // Act
        var result = PriceCalculator.CalculateSpread(0m, 100m);

        // Assert
        result.Should().Be(0m);
    }

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

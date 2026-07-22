#nullable enable
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Comprehensive edge case unit tests for the <see cref="PriceCalculator"/> utility methods.
/// Tests cover: zero amounts, very large values, rounding behavior, negative inputs,
/// and boundary conditions.
/// </summary>
public class PriceCalculatorEdgeCasesTests
{
    #region CalculatePercentageChange Edge Cases

    /// <summary>
    /// Verifies that CalculatePercentageChange handles zero original price correctly.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_ZeroOriginalPrice_ReturnsZero()
    {
        // Act
        var result = PriceCalculator.CalculatePercentageChange(0m, 100m);
        var result2 = PriceCalculator.CalculatePercentageChange(0m, 0m);
        var result3 = PriceCalculator.CalculatePercentageChange(0m, -100m);

        // Assert
        result.Should().Be(0m);
        result2.Should().Be(0m);
        result3.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that CalculatePercentageChange handles zero new price correctly.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_ZeroNewPrice_ReturnsNegativeHundred()
    {
        // Act
        var result = PriceCalculator.CalculatePercentageChange(100m, 0m);
        var result2 = PriceCalculator.CalculatePercentageChange(50m, 0m);

        // Assert
        result.Should().Be(-100m);
        result2.Should().Be(-100m);
    }

    /// <summary>
    /// Verifies that CalculatePercentageChange handles large values correctly.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_LargeValues_ReturnsCorrectResult()
    {
        // Arrange
        var largeValue = 1000000m;
        var largerValue = largeValue * 2;

        // Act
        var result = PriceCalculator.CalculatePercentageChange(largeValue, largerValue);

        // Assert
        result.Should().Be(100m);
    }

    /// <summary>
    /// Verifies that CalculatePercentageChange handles very small decimal values correctly.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_VerySmallValues_ReturnsCorrectResult()
    {
        // Arrange
        var smallValue = 0.0001m;
        var slightlyLarger = 0.0002m;

        // Act
        var result = PriceCalculator.CalculatePercentageChange(smallValue, slightlyLarger);

        // Assert
        result.Should().BeApproximately(100m, 0.001m);
    }

    /// <summary>
    /// Verifies that CalculatePercentageChange handles negative price values correctly.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_NegativeValues_ReturnsCorrectResult()
    {
        // Act
        var result1 = PriceCalculator.CalculatePercentageChange(-100m, -50m);
        var result2 = PriceCalculator.CalculatePercentageChange(-50m, -100m);
        var result3 = PriceCalculator.CalculatePercentageChange(-100m, 0m);
        var result4 = PriceCalculator.CalculatePercentageChange(0m, -100m);

        // Assert
        result1.Should().Be(-50m);  // (-50 - (-100)) / (-100) * 100 = 50 / -100 * 100 = -50
        result2.Should().Be(100m);  // (-100 - (-50)) / (-50) * 100 = -50 / -50 * 100 = 100
        result3.Should().Be(-100m); // (0 - (-100)) / (-100) * 100 = 100 / -100 * 100 = -100
        result4.Should().Be(0m);    // 0 from 0
    }

    #endregion

    #region CalculateSpread Edge Cases

    /// <summary>
    /// Verifies that CalculateSpread handles zero buy price correctly.
    /// </summary>
    [Fact]
    public void CalculateSpread_ZeroBuyPrice_ReturnsZero()
    {
        // Act
        var result1 = PriceCalculator.CalculateSpread(0m, 0m);
        var result2 = PriceCalculator.CalculateSpread(0m, 100m);
        var result3 = PriceCalculator.CalculateSpread(0m, -100m);

        // Assert
        result1.Should().Be(0m);
        result2.Should().Be(0m);
        result3.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that CalculateSpread handles zero sell price correctly.
    /// </summary>
    [Fact]
    public void CalculateSpread_ZeroSellPrice_ReturnsNegativeHundred()
    {
        // Act
        var result1 = PriceCalculator.CalculateSpread(100m, 0m);
        var result2 = PriceCalculator.CalculateSpread(-100m, 0m);

        // Assert
        result1.Should().Be(-100m);
        result2.Should().Be(-100m);
    }

    /// <summary>
    /// Verifies that CalculateSpread handles large values correctly.
    /// </summary>
    [Fact]
    public void CalculateSpread_LargeValues_ReturnsCorrectResult()
    {
        // Arrange
        var largeBuy = 1000000m;
        var largeSell = 2000000m;

        // Act
        var result = PriceCalculator.CalculateSpread(largeBuy, largeSell);

        // Assert
        result.Should().Be(100m);
    }

    /// <summary>
    /// Verifies that CalculateSpread handles very small decimal differences correctly.
    /// </summary>
    [Fact]
    public void CalculateSpread_VerySmallDecimalDifferences_ReturnsCorrectResult()
    {
        // Arrange
        var basePrice = 100.0001m;
        var higherPrice = 100.0002m;

        // Act
        var result = PriceCalculator.CalculateSpread(basePrice, higherPrice);

        // Assert
        result.Should().BeApproximately(0.0001m, 0.00001m);
    }

    /// <summary>
    /// Verifies that CalculateSpread handles negative price values correctly.
    /// </summary>
    [Fact]
    public void CalculateSpread_NegativeValues_ReturnsCorrectResult()
    {
        // Act
        var result1 = PriceCalculator.CalculateSpread(-100m, -50m);
        var result2 = PriceCalculator.CalculateSpread(-50m, -100m);
        var result3 = PriceCalculator.CalculateSpread(-100m, 0m);

        // Assert
        result1.Should().Be(-50m);  // ((-50 - (-100)) / -100) * 100 = 50 / -100 * 100 = -50
        result2.Should().Be(100m);  // ((-100 - (-50)) / -50) * 100 = -50 / -50 * 100 = 100
        result3.Should().Be(-100m); // ((0 - (-100)) / -100) * 100 = 100 / -100 * 100 = -100
    }

    #endregion

    #region CalculateMidPrice Edge Cases

    /// <summary>
    /// Verifies that CalculateMidPrice handles zero values correctly.
    /// </summary>
    [Fact]
    public void CalculateMidPrice_WithZeroValues_ReturnsCorrectResult()
    {
        // Act
        var result1 = PriceCalculator.CalculateMidPrice(0m, 0m);
        var result2 = PriceCalculator.CalculateMidPrice(0m, 100m);
        var result3 = PriceCalculator.CalculateMidPrice(100m, 0m);
        var result4 = PriceCalculator.CalculateMidPrice(-100m, 100m);

        // Assert
        result1.Should().Be(0m);
        result2.Should().Be(50m);
        result3.Should().Be(50m);
        result4.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that CalculateMidPrice handles large values correctly.
    /// </summary>
    [Fact]
    public void CalculateMidPrice_LargeValues_ReturnsCorrectResult()
    {
        // Arrange
        var largeValue = 1000000m;

        // Act
        var result = PriceCalculator.CalculateMidPrice(largeValue, largeValue);

        // Assert
        result.Should().Be(largeValue);
    }

    #endregion

    #region RoundPrice Edge Cases

    /// <summary>
    /// Verifies that RoundPrice handles zero values correctly.
    /// </summary>
    [Fact]
    public void RoundPrice_ZeroValue_ReturnsZero()
    {
        // Act
        var result = PriceCalculator.RoundPrice(0m, 2);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that RoundPrice handles very large values correctly.
    /// </summary>
    [Fact]
    public void RoundPrice_VeryLargeValue_ReturnsCorrectResult()
    {
        // Arrange
        var largeValue = decimal.MaxValue;

        // Act
        var result = PriceCalculator.RoundPrice(largeValue, 0);

        // Assert
        result.Should().Be(largeValue);
    }

    /// <summary>
    /// Verifies that RoundPrice handles very small decimal values correctly.
    /// </summary>
    [Fact]
    public void RoundPrice_VerySmallDecimalValue_ReturnsCorrectResult()
    {
        // Arrange
        var smallValue = 0.00000001m;

        // Act
        var result = PriceCalculator.RoundPrice(smallValue, 8);

        // Assert
        result.Should().Be(smallValue);
    }

    /// <summary>
    /// Verifies that RoundPrice handles negative values correctly.
    /// </summary>
    [Fact]
    public void RoundPrice_NegativeValue_ReturnsCorrectResult()
    {
        // Arrange
        var negativeValue = -123.456789m;

        // Act
        var result = PriceCalculator.RoundPrice(negativeValue, 2);

        // Assert
        result.Should().Be(-123.46m);
    }

    /// <summary>
    /// Verifies that RoundPrice uses AwayFromZero rounding mode correctly.
    /// </summary>
    [Fact]
    public void RoundPrice_AwayFromZeroRounding_RoundsCorrectly()
    {
        // Arrange - values that round up at midpoint
        var value1 = 1.235m; // Should round to 1.24 with 2 decimal places
        var value2 = -1.235m; // Should round to -1.24 with 2 decimal places

        // Act
        var result1 = PriceCalculator.RoundPrice(value1, 2);
        var result2 = PriceCalculator.RoundPrice(value2, 2);

        // Assert
        result1.Should().Be(1.24m);
        result2.Should().Be(-1.24m);
    }

    #endregion

    #region FormatPrice Edge Cases

    /// <summary>
    /// Verifies that FormatPrice handles zero values correctly.
    /// </summary>
    [Fact]
    public void FormatPrice_ZeroValue_ReturnsCorrectFormat()
    {
        // Act
        var result1 = PriceCalculator.FormatPrice(0m);
        var result2 = PriceCalculator.FormatPrice(0m, "$", 2);
        var result3 = PriceCalculator.FormatPrice(0m, "$", 4);

        // Assert
        result1.Should().Be("0.00");
        result2.Should().Be("$0.00");
        result3.Should().Be("$0.0000");
    }

    /// <summary>
    /// Verifies that FormatPrice handles very large values correctly.
    /// </summary>
    [Fact]
    public void FormatPrice_VeryLargeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var largeValue = 999999999999.99m;

        // Act
        var result = PriceCalculator.FormatPrice(largeValue, "$", 2);

        // Assert
        result.Should().Be("$999999999999.99");
    }

    /// <summary>
    /// Verifies that FormatPrice handles very small decimal values correctly.
    /// </summary>
    [Fact]
    public void FormatPrice_VerySmallDecimalValue_ReturnsCorrectFormat()
    {
        // Arrange
        var smallValue = 0.0001m;

        // Act
        var result = PriceCalculator.FormatPrice(smallValue, "$", 4);

        // Assert
        result.Should().Be("$0.0001");
    }

    /// <summary>
    /// Verifies that FormatPrice handles negative values correctly.
    /// </summary>
    [Fact]
    public void FormatPrice_NegativeValue_ReturnsCorrectFormat()
    {
        // Arrange
        var negativeValue = -123.456m;

        // Act
        var result = PriceCalculator.FormatPrice(negativeValue, "$", 2);

        // Assert
        result.Should().Be("$-123.46");
    }

    #endregion

    #region IsAboveThreshold / IsBelowThreshold Edge Cases

    /// <summary>
    /// Verifies that IsAboveThreshold handles zero values correctly.
    /// </summary>
    [Fact]
    public void IsAboveThreshold_ZeroValues_ReturnsCorrectResult()
    {
        // Act & Assert
        PriceCalculator.IsAboveThreshold(1m, 0m).Should().BeTrue();
        PriceCalculator.IsAboveThreshold(0m, 0m).Should().BeFalse();
        PriceCalculator.IsAboveThreshold(-1m, 0m).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsAboveThreshold handles very large values correctly.
    /// </summary>
    [Fact]
    public void IsAboveThreshold_VeryLargeValues_ReturnsCorrectResult()
    {
        // Arrange
        var largeValue = decimal.MaxValue / 2;

        // Act & Assert
        PriceCalculator.IsAboveThreshold(largeValue, largeValue - 1).Should().BeTrue();
        PriceCalculator.IsAboveThreshold(largeValue, largeValue).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsAboveThreshold handles negative values correctly.
    /// </summary>
    [Fact]
    public void IsAboveThreshold_NegativeValues_ReturnsCorrectResult()
    {
        // Act & Assert
        PriceCalculator.IsAboveThreshold(-50m, -100m).Should().BeTrue();
        PriceCalculator.IsAboveThreshold(-100m, -50m).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsBelowThreshold handles zero values correctly.
    /// </summary>
    [Fact]
    public void IsBelowThreshold_ZeroValues_ReturnsCorrectResult()
    {
        // Act & Assert
        PriceCalculator.IsBelowThreshold(-1m, 0m).Should().BeTrue();
        PriceCalculator.IsBelowThreshold(0m, 0m).Should().BeFalse();
        PriceCalculator.IsBelowThreshold(1m, 0m).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsBelowThreshold handles very large values correctly.
    /// </summary>
    [Fact]
    public void IsBelowThreshold_VeryLargeValues_ReturnsCorrectResult()
    {
        // Arrange
        var largeValue = decimal.MaxValue / 2;

        // Act & Assert
        PriceCalculator.IsBelowThreshold(largeValue, largeValue + 1).Should().BeTrue();
        PriceCalculator.IsBelowThreshold(largeValue, largeValue).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsBelowThreshold handles negative values correctly.
    /// </summary>
    [Fact]
    public void IsBelowThreshold_NegativeValues_ReturnsCorrectResult()
    {
        // Act & Assert
        PriceCalculator.IsBelowThreshold(-100m, -50m).Should().BeTrue();
        PriceCalculator.IsBelowThreshold(-50m, -100m).Should().BeFalse();
    }

    #endregion

    #region CalculateMovingAverage Edge Cases

    /// <summary>
    /// Verifies that CalculateMovingAverage handles empty collections correctly.
    /// </summary>
    [Fact]
    public void CalculateMovingAverage_EmptyCollection_ReturnsZero()
    {
        // Arrange
        var prices = new List<decimal>();

        // Act
        var result = PriceCalculator.CalculateMovingAverage(prices, 5);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that CalculateMovingAverage handles very large period values correctly.
    /// </summary>
    [Fact]
    public void CalculateMovingAverage_VeryLargePeriod_ReturnsOverallAverage()
    {
        // Arrange
        var prices = new List<decimal> { 1m, 2m, 3m };

        // Act
        var result = PriceCalculator.CalculateMovingAverage(prices, 1000000);

        // Assert
        result.Should().Be(2m); // (1+2+3)/3 = 2
    }

    /// <summary>
    /// Verifies that CalculateMovingAverage handles very large price values correctly.
    /// </summary>
    [Fact]
    public void CalculateMovingAverage_VeryLargePriceValues_ReturnsCorrectResult()
    {
        // Arrange
        var prices = new List<decimal> { decimal.MaxValue / 3, decimal.MaxValue / 3, decimal.MaxValue / 3 };

        // Act
        var result = PriceCalculator.CalculateMovingAverage(prices, 3);

        // Assert
        result.Should().Be(decimal.MaxValue / 3);
    }

    #endregion

    #region CalculateStandardDeviation Edge Cases

    /// <summary>
    /// Verifies that CalculateStandardDeviation handles single-item collections correctly.
    /// </summary>
    [Fact]
    public void CalculateStandardDeviation_SingleItem_ReturnsZero()
    {
        // Arrange
        var prices = new List<decimal> { 42m };

        // Act
        var result = PriceCalculator.CalculateStandardDeviation(prices);

        // Assert
        result.Should().Be(0m);
    }

    /// <summary>
    /// Verifies that CalculateStandardDeviation handles very large values correctly.
    /// </summary>
    [Fact]
    public void CalculateStandardDeviation_VeryLargeValues_ReturnsCorrectResult()
    {
        // Arrange
        var prices = new List<decimal> { 1000000m, 2000000m, 3000000m };

        // Act
        var result = PriceCalculator.CalculateStandardDeviation(prices);

        // Assert
        result.Should().BeApproximately(816496.58m, 0.01m);
    }

    /// <summary>
    /// Verifies that CalculateStandardDeviation handles negative values correctly.
    /// </summary>
    [Fact]
    public void CalculateStandardDeviation_NegativeValues_ReturnsCorrectResult()
    {
        // Arrange
        var prices = new List<decimal> { -100m, -50m, 0m, 50m, 100m };

        // Act
        var result = PriceCalculator.CalculateStandardDeviation(prices);

        // Assert - should be same as positive version
        result.Should().BePositive();
    }

    #endregion
}

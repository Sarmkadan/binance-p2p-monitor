#nullable enable
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for <see cref="NumericExtensions"/> extension methods.
/// Tests cover happy-path scenarios, edge cases, boundary values, and error paths.
/// </summary>
public class NumericExtensionsUnitTests
{
    #region RoundTo Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.RoundTo"/> correctly rounds decimal values to specified decimal places.
    /// </summary>
    [Fact]
    public void RoundTo_WithPositiveDecimalPlaces_ShouldRoundCorrectly()
    {
        // Test basic rounding
        123.456m.RoundTo(2).Should().Be(123.46m);
        123.454m.RoundTo(2).Should().Be(123.45m);
        123.455m.RoundTo(2).Should().Be(123.46m);

        // Test rounding to 0 decimal places
        123.456m.RoundTo(0).Should().Be(123m);
        123.999m.RoundTo(0).Should().Be(124m);

        // Test rounding to 4 decimal places
        123.456789m.RoundTo(4).Should().Be(123.4568m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.RoundTo"/> handles zero correctly.
    /// </summary>
    [Fact]
    public void RoundTo_WithZeroValue_ShouldReturnZero()
    {
        0m.RoundTo(2).Should().Be(0m);
        0m.RoundTo(0).Should().Be(0m);
        0m.RoundTo(5).Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.RoundTo"/> throws when decimalPlaces is negative.
    /// </summary>
    [Fact]
    public void RoundTo_WithNegativeDecimalPlaces_ShouldThrowArgumentOutOfRangeException()
    {
        var action = () => 123.456m.RoundTo(-1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region IsWithinPercentage Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsWithinPercentage"/> correctly identifies values within percentage threshold.
    /// </summary>
    [Fact]
    public void IsWithinPercentage_WithValuesWithinThreshold_ShouldReturnTrue()
    {
        // Test within 5% threshold
        105m.IsWithinPercentage(100m, 5m).Should().BeTrue();
        95m.IsWithinPercentage(100m, 5m).Should().BeTrue();
        104.99m.IsWithinPercentage(100m, 5m).Should().BeTrue();

        // Test within 10% threshold
        110m.IsWithinPercentage(100m, 10m).Should().BeTrue();
        90m.IsWithinPercentage(100m, 10m).Should().BeTrue();

        // Test exact match
        100m.IsWithinPercentage(100m, 5m).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsWithinPercentage"/> correctly identifies values outside percentage threshold.
    /// </summary>
    [Fact]
    public void IsWithinPercentage_WithValuesOutsideThreshold_ShouldReturnFalse()
    {
        // Test outside 5% threshold
        105.01m.IsWithinPercentage(100m, 5m).Should().BeFalse();
        94.99m.IsWithinPercentage(100m, 5m).Should().BeFalse();

        // Test outside 10% threshold
        110.01m.IsWithinPercentage(100m, 10m).Should().BeFalse();
        89.99m.IsWithinPercentage(100m, 10m).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsWithinPercentage"/> handles zero target correctly.
    /// </summary>
    [Fact]
    public void IsWithinPercentage_WithZeroTarget_ShouldReturnTrueOnlyWhenBothZero()
    {
        // When target is 0, only 0 is considered within any threshold
        0m.IsWithinPercentage(0m, 5m).Should().BeTrue();
        1m.IsWithinPercentage(0m, 5m).Should().BeFalse();
        (-1m).IsWithinPercentage(0m, 5m).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsWithinPercentage"/> throws when percentageThreshold is negative.
    /// </summary>
    [Fact]
    public void IsWithinPercentage_WithNegativeThreshold_ShouldThrowArgumentOutOfRangeException()
    {
        var action = () => 100m.IsWithinPercentage(100m, -1m);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region CalculatePercentageChange Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.CalculatePercentageChange"/> correctly calculates percentage change for positive changes.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_WithPositiveChange_ShouldReturnPositiveValue()
    {
        110m.CalculatePercentageChange(100m).Should().Be(10m);
        200m.CalculatePercentageChange(100m).Should().Be(100m);
        150m.CalculatePercentageChange(100m).Should().Be(50m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.CalculatePercentageChange"/> correctly calculates percentage change for negative changes.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_WithNegativeChange_ShouldReturnNegativeValue()
    {
        90m.CalculatePercentageChange(100m).Should().Be(-10m);
        50m.CalculatePercentageChange(100m).Should().Be(-50m);
        1m.CalculatePercentageChange(100m).Should().Be(-99m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.CalculatePercentageChange"/> returns 0 when previousValue is 0.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_WithZeroPreviousValue_ShouldReturnZero()
    {
        100m.CalculatePercentageChange(0m).Should().Be(0m);
        0m.CalculatePercentageChange(0m).Should().Be(0m);
        1m.CalculatePercentageChange(0m).Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.CalculatePercentageChange"/> handles equal values correctly.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_WithEqualValues_ShouldReturnZero()
    {
        100m.CalculatePercentageChange(100m).Should().Be(0m);
        0m.CalculatePercentageChange(0m).Should().Be(0m);
        var negativeValue = -50m;
        negativeValue.CalculatePercentageChange(negativeValue).Should().Be(0m);
    }

    #endregion

    #region Clamp Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.Clamp"/> correctly clamps values within range.
    /// </summary>
    [Fact]
    public void Clamp_WithValueWithinRange_ShouldReturnSameValue()
    {
        50m.Clamp(0m, 100m).Should().Be(50m);
        0m.Clamp(0m, 100m).Should().Be(0m);
        100m.Clamp(0m, 100m).Should().Be(100m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.Clamp"/> clamps values below minimum.
    /// </summary>
    [Fact]
    public void Clamp_WithValueBelowMinimum_ShouldReturnMinimum()
    {
        (-10m).Clamp(0m, 100m).Should().Be(0m);
        (-100m).Clamp(0m, 100m).Should().Be(0m);
        (-1m).Clamp(0m, 100m).Should().Be(0m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.Clamp"/> clamps values above maximum.
    /// </summary>
    [Fact]
    public void Clamp_WithValueAboveMaximum_ShouldReturnMaximum()
    {
        110m.Clamp(0m, 100m).Should().Be(100m);
        200m.Clamp(0m, 100m).Should().Be(100m);
        150m.Clamp(0m, 100m).Should().Be(100m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.Clamp"/> throws when min is greater than max.
    /// </summary>
    [Fact]
    public void Clamp_WithMinGreaterThanMax_ShouldThrowArgumentException()
    {
        var action = () => 50m.Clamp(100m, 0m);
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region IsPositive Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsPositive"/> correctly identifies positive values.
    /// </summary>
    [Fact]
    public void IsPositive_WithPositiveValue_ShouldReturnTrue()
    {
        1m.IsPositive().Should().BeTrue();
        0.001m.IsPositive().Should().BeTrue();
        decimal.MaxValue.IsPositive().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsPositive"/> returns false for non-positive values.
    /// </summary>
    [Fact]
    public void IsPositive_WithNonPositiveValue_ShouldReturnFalse()
    {
        0m.IsPositive().Should().BeFalse();
        (-1m).IsPositive().Should().BeFalse();
        decimal.MinValue.IsPositive().Should().BeFalse();
    }

    #endregion

    #region IsNegative Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsNegative"/> correctly identifies negative values.
    /// </summary>
    [Fact]
    public void IsNegative_WithNegativeValue_ShouldReturnTrue()
    {
        (-1m).IsNegative().Should().BeTrue();
        (-0.001m).IsNegative().Should().BeTrue();
        decimal.MinValue.IsNegative().Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsNegative"/> returns false for non-negative values.
    /// </summary>
    [Fact]
    public void IsNegative_WithNonNegativeValue_ShouldReturnFalse()
    {
        0m.IsNegative().Should().BeFalse();
        1m.IsNegative().Should().BeFalse();
        decimal.MaxValue.IsNegative().Should().BeFalse();
    }

    #endregion

    #region IsBetween Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsBetween"/> correctly identifies values within range.
    /// </summary>
    [Fact]
    public void IsBetween_WithValueWithinRange_ShouldReturnTrue()
    {
        50m.IsBetween(0m, 100m).Should().BeTrue();
        0m.IsBetween(0m, 100m).Should().BeTrue();
        100m.IsBetween(0m, 100m).Should().BeTrue();
        50.5m.IsBetween(50m, 51m).Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsBetween"/> returns false for values outside range.
    /// </summary>
    [Fact]
    public void IsBetween_WithValueOutsideRange_ShouldReturnFalse()
    {
        (-1m).IsBetween(0m, 100m).Should().BeFalse();
        101m.IsBetween(0m, 100m).Should().BeFalse();
        99m.IsBetween(100m, 200m).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.IsBetween"/> throws when min is greater than max.
    /// </summary>
    [Fact]
    public void IsBetween_WithMinGreaterThanMax_ShouldThrowArgumentException()
    {
        var action = () => 50m.IsBetween(100m, 0m);
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region AbsolutePercentageDifference Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.AbsolutePercentageDifference"/> calculates correct difference for positive values.
    /// </summary>
    [Fact]
    public void AbsolutePercentageDifference_WithPositiveValues_ShouldReturnCorrectDifference()
    {
        100m.AbsolutePercentageDifference(100m).Should().BeApproximately(0m, 0.0001m);
        110m.AbsolutePercentageDifference(100m).Should().BeApproximately(9.5238m, 0.0001m);
        120m.AbsolutePercentageDifference(100m).Should().BeApproximately(18.1818m, 0.0001m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.AbsolutePercentageDifference"/> calculates correct difference for negative values.
    /// </summary>
    [Fact]
    public void AbsolutePercentageDifference_WithNegativeValues_ShouldReturnCorrectDifference()
    {
        (-100m).AbsolutePercentageDifference(-100m).Should().BeApproximately(0m, 0.0001m);
        (-110m).AbsolutePercentageDifference(-100m).Should().BeApproximately(9.5238m, 0.0001m);
        (-120m).AbsolutePercentageDifference(-100m).Should().BeApproximately(18.1818m, 0.0001m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.AbsolutePercentageDifference"/> handles mixed positive and negative values.
    /// </summary>
    [Fact]
    public void AbsolutePercentageDifference_WithMixedPositiveNegativeValues_ShouldReturnCorrectDifference()
    {
        100m.AbsolutePercentageDifference(-100m).Should().BeApproximately(200m, 0.0001m);
        (-100m).AbsolutePercentageDifference(100m).Should().BeApproximately(200m, 0.0001m);
        50m.AbsolutePercentageDifference(-50m).Should().BeApproximately(200m, 0.0001m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.AbsolutePercentageDifference"/> returns 0 when both values are 0.
    /// </summary>
    [Fact]
    public void AbsolutePercentageDifference_WithBothZero_ShouldReturnZero()
    {
        0m.AbsolutePercentageDifference(0m).Should().Be(0m);
    }

    #endregion

    #region ToCurrencyString Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.ToCurrencyString"/> formats decimal as currency string with default symbol.
    /// </summary>
    [Fact]
    public void ToCurrencyString_WithDefaultSymbol_ShouldFormatCorrectly()
    {
        123.456m.ToCurrencyString().Should().Be("$123.46");
        1000m.ToCurrencyString().Should().Be("$1,000.00");
        0.99m.ToCurrencyString().Should().Be("$0.99");
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.ToCurrencyString"/> formats decimal as currency string with custom symbol.
    /// </summary>
    [Fact]
    public void ToCurrencyString_WithCustomSymbol_ShouldFormatCorrectly()
    {
        123.456m.ToCurrencyString("€").Should().Be("€123.46");
        1000m.ToCurrencyString("£").Should().Be("£1,000.00");
        50.5m.ToCurrencyString("¥").Should().Be("¥50.50");
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.ToCurrencyString"/> throws when currencySymbol is null.
    /// </summary>
    [Fact]
    public void ToCurrencyString_WithNullSymbol_ShouldThrowArgumentNullException()
    {
        var action = () => 100m.ToCurrencyString(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region FormatPrecision Tests

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.FormatPrecision"/> formats decimal with specified precision.
    /// </summary>
    [Fact]
    public void FormatPrecision_WithValidPrecision_ShouldFormatCorrectly()
    {
        123.456789m.FormatPrecision(0).Should().Be("123");
        123.456789m.FormatPrecision(1).Should().Be("123.5");
        123.456789m.FormatPrecision(2).Should().Be("123.46");
        123.456789m.FormatPrecision(3).Should().Be("123.457");
        123.456789m.FormatPrecision(6).Should().Be("123.456789");
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.FormatPrecision"/> handles zero correctly.
    /// </summary>
    [Fact]
    public void FormatPrecision_WithZeroValue_ShouldReturnZeroString()
    {
        0m.FormatPrecision(2).Should().Be("0.00");
        0m.FormatPrecision(0).Should().Be("0");
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.FormatPrecision"/> throws when precision is negative.
    /// </summary>
    [Fact]
    public void FormatPrecision_WithNegativePrecision_ShouldThrowArgumentOutOfRangeException()
    {
        var action = () => 123.456m.FormatPrecision(-1);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}

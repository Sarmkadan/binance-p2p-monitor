using System;

#nullable enable

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for numeric operations
/// </summary>
public static class NumericExtensions
{
    /// <summary>
    /// Rounds decimal to specified decimal places
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="decimalPlaces">Number of decimal places to round to.</param>
    /// <returns>The rounded value.</returns>
    public static decimal RoundTo(this decimal value, int decimalPlaces)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Checks if decimal is within a percentage range of a target value
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="target">The target value to compare against.</param>
    /// <param name="percentageThreshold">Maximum allowed percentage difference (e.g., 5 for 5%).</param>
    /// <returns>True if the value is within the percentage threshold of the target.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when percentageThreshold is negative.</exception>
    public static bool IsWithinPercentage(this decimal value, decimal target, decimal percentageThreshold)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(percentageThreshold);

        if (target == 0)
        {
            return value == 0;
        }

        var difference = Math.Abs((value - target) / target * 100);
        return difference <= percentageThreshold;
    }

    /// <summary>
    /// Calculates percentage change between two values
    /// </summary>
    /// <param name="currentValue">The current value.</param>
    /// <param name="previousValue">The previous value.</param>
    /// <returns>The percentage change from previous to current value. Returns 0 when previousValue is 0 to avoid division by zero.</returns>
    public static decimal CalculatePercentageChange(this decimal currentValue, decimal previousValue)
    {
        if (previousValue == 0)
        {
            return 0;
        }

        return ((currentValue - previousValue) / previousValue) * 100;
    }

    /// <summary>
    /// Clamps value between min and max (inclusive)
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The clamped value.</returns>
    /// <exception cref="ArgumentException">Thrown when min is greater than max.</exception>
    public static decimal Clamp(this decimal value, decimal min, decimal max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);
        return Math.Min(Math.Max(value, min), max);
    }

    /// <summary>
    /// Checks if value is positive
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is greater than 0.</returns>
    public static bool IsPositive(this decimal value) => value > 0;

    /// <summary>
    /// Checks if value is negative
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is less than 0.</returns>
    public static bool IsNegative(this decimal value) => value < 0;

    /// <summary>
    /// Checks if value is between range (inclusive)
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <returns>True if the value is within the specified range.</returns>
    /// <exception cref="ArgumentException">Thrown when min is greater than max.</exception>
    public static bool IsBetween(this decimal value, decimal min, decimal max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);
        return value >= min && value <= max;
    }

    /// <summary>
    /// Gets absolute percentage difference between two values
    /// </summary>
    /// <param name="value1">The first value.</param>
    /// <param name="value2">The second value.</param>
    /// <returns>The absolute percentage difference between the two values. Returns 0 when both values are 0.</returns>
    public static decimal AbsolutePercentageDifference(this decimal value1, decimal value2)
    {
        if (value1 == 0 && value2 == 0)
        {
            return 0;
        }

        var average = (Math.Abs(value1) + Math.Abs(value2)) / 2;
        return average == 0 ? 0 : Math.Abs(value1 - value2) / average * 100;
    }

    /// <summary>
    /// Formats decimal as currency string
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="currencySymbol">The currency symbol to prefix (default: "$").</param>
    /// <returns>Formatted currency string with 2 decimal places.</returns>
    /// <exception cref="ArgumentNullException">Thrown when currencySymbol is null.</exception>
    public static string ToCurrencyString(this decimal value, string currencySymbol = "$")
    {
        ArgumentNullException.ThrowIfNull(currencySymbol);
        return $"{currencySymbol}{value:N2}";
    }

    /// <summary>
    /// Formats decimal with specific precision
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="precision">Number of decimal places to display.</param>
    /// <returns>Formatted string with specified precision.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when precision is negative.</exception>
    public static string FormatPrecision(this decimal value, int precision)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(precision);
        return value.ToString($"F{precision}");
    }
}
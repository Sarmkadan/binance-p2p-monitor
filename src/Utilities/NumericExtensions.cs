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
    public static decimal RoundTo(this decimal value, int decimalPlaces)
    {
        return Math.Round(value, decimalPlaces);
    }

    /// <summary>
    /// Checks if decimal is within a percentage range
    /// </summary>
    public static bool IsWithinPercentage(this decimal value, decimal target, decimal percentageThreshold)
    {
        var difference = Math.Abs((value - target) / target * 100);
        return difference <= percentageThreshold;
    }

    /// <summary>
    /// Calculates percentage change between two values
    /// </summary>
    public static decimal CalculatePercentageChange(this decimal currentValue, decimal previousValue)
    {
        if (previousValue == 0)
            return 0;

        return ((currentValue - previousValue) / previousValue) * 100;
    }

    /// <summary>
    /// Clamps value between min and max
    /// </summary>
    public static decimal Clamp(this decimal value, decimal min, decimal max)
    {
        return Math.Min(Math.Max(value, min), max);
    }

    /// <summary>
    /// Checks if value is positive
    /// </summary>
    public static bool IsPositive(this decimal value) => value > 0;

    /// <summary>
    /// Checks if value is negative
    /// </summary>
    public static bool IsNegative(this decimal value) => value < 0;

    /// <summary>
    /// Checks if value is between range (inclusive)
    /// </summary>
    public static bool IsBetween(this decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Gets absolute percentage difference between two values
    /// </summary>
    public static decimal AbsolutePercentageDifference(this decimal value1, decimal value2)
    {
        if (value1 == 0 && value2 == 0)
            return 0;

        var average = (Math.Abs(value1) + Math.Abs(value2)) / 2;
        return average == 0 ? 0 : Math.Abs(value1 - value2) / average * 100;
    }

    /// <summary>
    /// Formats decimal as currency string
    /// </summary>
    public static string ToCurrencyString(this decimal value, string currencySymbol = "$")
    {
        return $"{currencySymbol}{value:N2}";
    }

    /// <summary>
    /// Formats decimal with specific precision
    /// </summary>
    public static string FormatPrecision(this decimal value, int precision)
    {
        return value.ToString($"F{precision}");
    }
}

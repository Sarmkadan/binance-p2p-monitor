#nullable enable

using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Validation extension methods for decimal values using NumericExtensions operations
/// </summary>
public static class NumericExtensionsValidation
{
    /// <summary>
    /// Validates that the decimal value can be safely used with NumericExtensions operations
    /// </summary>
    /// <param name="value">The decimal value to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this decimal value)
    {
        var problems = new List<string>();

        // Test RoundTo with various decimal places
        try
        {
            _ = value.RoundTo(2);
            _ = value.RoundTo(4);
            _ = value.RoundTo(8);
        }
        catch (Exception ex)
        {
            problems.Add($"RoundTo operation failed: {ex.Message}");
        }

        // Test IsWithinPercentage with various thresholds
        try
        {
            _ = value.IsWithinPercentage(100, 5);
            _ = value.IsWithinPercentage(100, 0.1m);
            _ = value.IsWithinPercentage(100, 100);
        }
        catch (Exception ex)
        {
            problems.Add($"IsWithinPercentage operation failed: {ex.Message}");
        }

        // Test CalculatePercentageChange with zero previous value
        try
        {
            _ = value.CalculatePercentageChange(0);
        }
        catch (Exception ex)
        {
            problems.Add($"CalculatePercentageChange with zero previous value failed: {ex.Message}");
        }

        // Test Clamp with various ranges
        try
        {
            _ = value.Clamp(0, 100);
            _ = value.Clamp(-1000, 1000);
            _ = value.Clamp(decimal.MinValue, decimal.MaxValue);
        }
        catch (Exception ex)
        {
            problems.Add($"Clamp operation failed: {ex.Message}");
        }

        // Test IsPositive and IsNegative
        if (value.IsPositive() && value.IsNegative())
        {
            problems.Add("Value cannot be both positive and negative simultaneously");
        }

        // Test IsBetween with various ranges
        try
        {
            _ = value.IsBetween(0, 100);
            _ = value.IsBetween(decimal.MinValue, decimal.MaxValue);
        }
        catch (Exception ex)
        {
            problems.Add($"IsBetween operation failed: {ex.Message}");
        }

        // Test AbsolutePercentageDifference
        try
        {
            _ = value.AbsolutePercentageDifference(0);
            _ = value.AbsolutePercentageDifference(100);
            _ = value.AbsolutePercentageDifference(decimal.MaxValue);
        }
        catch (Exception ex)
        {
            problems.Add($"AbsolutePercentageDifference operation failed: {ex.Message}");
        }

        // Test edge cases that might cause issues
        if (value == decimal.MinValue || value == decimal.MaxValue)
        {
            try
            {
                _ = value.CalculatePercentageChange(1);
                _ = value.IsWithinPercentage(1, 1);
            }
            catch (Exception ex)
            {
                problems.Add($"Extreme value operations failed: {ex.Message}");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the decimal value is valid (has no validation problems)
    /// </summary>
    /// <param name="value">The decimal value to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this decimal value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the decimal value is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="value">The decimal value to validate</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with list of problems</exception>
    public static void EnsureValid(this decimal value)
    {
        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"Decimal value validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
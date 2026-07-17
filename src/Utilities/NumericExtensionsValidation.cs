#nullable enable

using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Validation extension methods for decimal values to ensure safe usage with NumericExtensions operations
/// </summary>
public static class NumericExtensionsValidation
{
    /// <summary>
    /// Validates that the decimal value is within safe bounds for financial calculations.
    /// </summary>
    /// <remarks>
    /// This method validates that the decimal value can be safely used with NumericExtensions operations
    /// by checking for extreme values that could cause overflow or precision issues in financial calculations.
    /// </remarks>
    /// <param name="value">The decimal value to validate.</param>
    /// <returns>List of validation problems (empty if valid).</returns>
    public static IReadOnlyList<string> Validate(this decimal value)
    {
        var problems = new List<string>();

        // Check for extreme values that could cause overflow in calculations
        if (value == decimal.MinValue)
        {
            problems.Add("Value is decimal.MinValue which may cause overflow in calculations");
        }
        else if (value == decimal.MaxValue)
        {
            problems.Add("Value is decimal.MaxValue which may cause overflow in calculations");
        }

        // Check for values that are too small to represent meaningful currency amounts
        if (value != 0 && Math.Abs(value) < 0.000001m)
        {
            problems.Add("Value is too small to represent meaningful currency amounts");
        }

        // Validate that basic operations won't overflow
        try
        {
            // Test multiplication scenarios that could overflow
            if (value != 0 && Math.Abs(value) > 1000000000)
            {
                _ = value * 1000;
            }
        }
        catch (OverflowException)
        {
            problems.Add("Value is too large and would cause overflow in multiplication operations");
        }
        catch (Exception ex)
        {
            problems.Add($"Overflow check failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the decimal value is valid (has no validation problems).
    /// </summary>
    /// <param name="value">The decimal value to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public static bool IsValid(this decimal value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures the decimal value is valid, throwing ArgumentException if not.
    /// </summary>
    /// <remarks>
    /// This method validates that the decimal value can be safely used in financial calculations.
    /// </remarks>
    /// <param name="value">The decimal value to validate.</param>
    /// <exception cref="ArgumentException">Thrown if validation fails with a list of problems.</exception>
    public static void EnsureValid(this decimal value)
    {
        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Decimal value validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}
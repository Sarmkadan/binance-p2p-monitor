#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides validation helpers for <see cref="Price"/> instances
/// </summary>
public static class PriceValidation
{
    /// <summary>
    /// Validates a Price instance and returns a list of human-readable validation problems
    /// </summary>
    /// <param name="value">The Price instance to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static IReadOnlyList<string> Validate(this Price value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Asset
        if (string.IsNullOrWhiteSpace(value.Asset))
        {
            errors.Add("Asset cannot be null or whitespace");
        }
        else if (value.Asset.Length > 20)
        {
            errors.Add("Asset cannot exceed 20 characters");
        }

        // Validate Fiat
        if (string.IsNullOrWhiteSpace(value.Fiat))
        {
            errors.Add("Fiat cannot be null or whitespace");
        }
        else if (value.Fiat.Length > 10)
        {
            errors.Add("Fiat cannot exceed 10 characters");
        }

        // Validate BuyPrice
        if (value.BuyPrice <= 0)
        {
            errors.Add("BuyPrice must be greater than 0");
        }

        // Validate SellPrice
        if (value.SellPrice <= 0)
        {
            errors.Add("SellPrice must be greater than 0");
        }
        else if (value.SellPrice < value.BuyPrice)
        {
            errors.Add("SellPrice cannot be less than BuyPrice");
        }

        // Validate BuyChangePercent
        if (value.BuyChangePercent < 0 || value.BuyChangePercent > 100)
        {
            errors.Add("BuyChangePercent must be between 0 and 100 inclusive");
        }

        // Validate SellChangePercent
        if (value.SellChangePercent < 0 || value.SellChangePercent > 100)
        {
            errors.Add("SellChangePercent must be between 0 and 100 inclusive");
        }

        // Validate Timestamp
        if (value.Timestamp == default)
        {
            errors.Add("Timestamp cannot be default(DateTime)");
        }
        else if (value.Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Timestamp cannot be in the future");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("CreatedAt cannot be default(DateTime)");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("CreatedAt cannot be in the future");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            errors.Add("UpdatedAt cannot be default(DateTime)");
        }
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("UpdatedAt cannot be in the future");
        }
        else if (value.UpdatedAt < value.CreatedAt)
        {
            errors.Add("UpdatedAt cannot be earlier than CreatedAt");
        }

        // Validate Metadata length
        if (value.Metadata?.Length > 1000)
        {
            errors.Add("Metadata cannot exceed 1000 characters");
        }

        // Validate History collection
        if (value.History is null)
        {
            errors.Add("History collection cannot be null");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified Price instance is valid
    /// </summary>
    /// <param name="value">The Price instance to check</param>
    /// <returns>True if the Price is valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static bool IsValid(this Price value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified Price instance is valid, throwing an exception if it is not
    /// </summary>
    /// <param name="value">The Price instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    /// <exception cref="ArgumentException">Thrown when value is not valid, containing the validation errors</exception>
    public static void EnsureValid(this Price value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Price validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)
                }");
        }
    }
}
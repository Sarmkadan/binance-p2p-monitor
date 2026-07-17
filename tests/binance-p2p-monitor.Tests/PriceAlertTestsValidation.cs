#nullable enable

using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Provides validation helpers for PriceAlert and Spread models
/// </summary>
public static class PriceAlertTestsValidation
{
    /// <summary>
    /// Validates a PriceAlert instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The PriceAlert to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this PriceAlert value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Asset
        if (string.IsNullOrWhiteSpace(value.Asset))
        {
            problems.Add("Asset cannot be null or whitespace");
        }
        else if (value.Asset.Length > 20)
        {
            problems.Add("Asset exceeds maximum length of 20 characters");
        }

        // Validate Fiat
        if (string.IsNullOrWhiteSpace(value.Fiat))
        {
            problems.Add("Fiat cannot be null or whitespace");
        }
        else if (value.Fiat.Length > 10)
        {
            problems.Add("Fiat exceeds maximum length of 10 characters");
        }

        // Validate AlertType - must not be Unknown (0)
        if (value.AlertType == AlertType.Unknown)
        {
            problems.Add("AlertType must be specified and cannot be Unknown");
        }

        // Validate Threshold
        if (value.Threshold < 0 || value.Threshold > 100)
        {
            problems.Add("Threshold must be between 0 and 100");
        }

        // Validate Condition - must not be Unknown (0)
        if (value.Condition == AlertCondition.Unknown)
        {
            problems.Add("Condition must be specified and cannot be Unknown");
        }

        // Validate UserId
        if (value.UserId <= 0)
        {
            problems.Add("UserId must be a positive integer");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid DateTime");
        }

        // Validate UpdatedAt
        if (value.UpdatedAt == default)
        {
            problems.Add("UpdatedAt must be set to a valid DateTime");
        }

        // Validate LastTriggeredAt - binary value cannot be zero
        if (value.LastTriggeredAt.HasValue && value.LastTriggeredAt.Value == 0)
        {
            problems.Add("LastTriggeredAt binary value cannot be zero");
        }

        // Validate TriggerCount
        if (value.TriggerCount < 0)
        {
            problems.Add("TriggerCount cannot be negative");
        }

        // Validate Notes length
        if (value.Notes?.Length > 500)
        {
            problems.Add("Notes exceed maximum length of 500 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a PriceAlert instance is valid
    /// </summary>
    /// <param name="value">The PriceAlert to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this PriceAlert value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a PriceAlert instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The PriceAlert to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid with detailed problems</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this PriceAlert value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PriceAlert is invalid: {string.Join("; ", problems)}");
        }
    }

    /// <summary>
    /// Validates a Spread instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The Spread to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this Spread value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Asset
        if (string.IsNullOrWhiteSpace(value.Asset))
        {
            problems.Add("Asset cannot be null or whitespace");
        }
        else if (value.Asset.Length > 20)
        {
            problems.Add("Asset exceeds maximum length of 20 characters");
        }

        // Validate Fiat
        if (string.IsNullOrWhiteSpace(value.Fiat))
        {
            problems.Add("Fiat cannot be null or whitespace");
        }
        else if (value.Fiat.Length > 10)
        {
            problems.Add("Fiat exceeds maximum length of 10 characters");
        }

        // Validate CurrentSpreadPercent
        if (value.CurrentSpreadPercent < 0)
        {
            problems.Add("CurrentSpreadPercent cannot be negative");
        }

        // Validate AverageSpreadPercent
        if (value.AverageSpreadPercent < 0)
        {
            problems.Add("AverageSpreadPercent cannot be negative");
        }

        // Validate MinSpreadPercent
        if (value.MinSpreadPercent < 0)
        {
            problems.Add("MinSpreadPercent cannot be negative");
        }

        // Validate MaxSpreadPercent
        if (value.MaxSpreadPercent < 0)
        {
            problems.Add("MaxSpreadPercent cannot be negative");
        }

        // Validate MinSpreadPercent <= MaxSpreadPercent
        if (value.MinSpreadPercent > value.MaxSpreadPercent)
        {
            problems.Add("MinSpreadPercent cannot be greater than MaxSpreadPercent");
        }

        // Validate SampleCount
        if (value.SampleCount < 1)
        {
            problems.Add("SampleCount must be at least 1");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            problems.Add("CreatedAt must be set to a valid DateTime");
        }

        // Validate LastUpdatedAt
        if (value.LastUpdatedAt == default)
        {
            problems.Add("LastUpdatedAt must be set to a valid DateTime");
        }

        // Validate StandardDeviation
        if (value.StandardDeviation < 0)
        {
            problems.Add("StandardDeviation cannot be negative");
        }

        // Validate PercentileRank
        if (value.PercentileRank < 0 || value.PercentileRank > 100)
        {
            problems.Add("PercentileRank must be between 0 and 100");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a Spread instance is valid
    /// </summary>
    /// <param name="value">The Spread to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this Spread value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a Spread instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The Spread to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid with detailed problems</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this Spread value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Spread is invalid: {string.Join("; ", problems)}");
        }
    }
}
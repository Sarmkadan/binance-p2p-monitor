#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides validation helpers for <see cref="SpreadStatisticsReport"/> instances
/// </summary>
public static class SpreadStatisticsReportValidation
{
    /// <summary>
    /// Validates a <see cref="SpreadStatisticsReport"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The report to validate.</param>
    /// <returns>A read-only list of validation error messages (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SpreadStatisticsReport? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate string properties
        if (string.IsNullOrWhiteSpace(value.Asset))
        {
            errors.Add("Asset cannot be null or whitespace.");
        }
        else if (value.Asset.Length > 20)
        {
            errors.Add("Asset exceeds maximum length of 20 characters.");
        }

        if (string.IsNullOrWhiteSpace(value.Fiat))
        {
            errors.Add("Fiat cannot be null or whitespace.");
        }
        else if (value.Fiat.Length > 10)
        {
            errors.Add("Fiat exceeds maximum length of 10 characters.");
        }

        // Validate time window
        if (value.TimeWindowHours < 1)
        {
            errors.Add("TimeWindowHours must be at least 1 hour.");
        }

        // Validate sample count
        if (value.SampleCount < 0)
        {
            errors.Add("SampleCount cannot be negative.");
        }

        // Validate statistical values (all should be non-negative)
        ValidateNonNegativeDecimal(errors, nameof(value.Mean), value.Mean);
        ValidateNonNegativeDecimal(errors, nameof(value.Median), value.Median);
        ValidateNonNegativeDecimal(errors, nameof(value.StandardDeviation), value.StandardDeviation);
        ValidateNonNegativeDecimal(errors, nameof(value.Variance), value.Variance);
        ValidateNonNegativeDecimal(errors, nameof(value.MinSpread), value.MinSpread);
        ValidateNonNegativeDecimal(errors, nameof(value.MaxSpread), value.MaxSpread);
        ValidateNonNegativeDecimal(errors, nameof(value.Percentile5), value.Percentile5);
        ValidateNonNegativeDecimal(errors, nameof(value.Percentile95), value.Percentile95);

        // CurrentSpread, ZScore, and TrendSlope can be any decimal value (positive or negative)
        // No specific validation needed beyond range consistency with MinSpread/MaxSpread

        // Validate AnalyzedAt
        if (value.AnalyzedAt == default)
        {
            errors.Add("AnalyzedAt cannot be the default DateTime value.");
        }
        else if (value.AnalyzedAt.Kind != DateTimeKind.Utc)
        {
            errors.Add("AnalyzedAt must be in UTC timezone.");
        }

        // Validate CurrentSpread should be within MinSpread and MaxSpread range
        if (value.CurrentSpread < value.MinSpread)
        {
            errors.Add("CurrentSpread cannot be less than MinSpread.");
        }

        if (value.CurrentSpread > value.MaxSpread)
        {
            errors.Add("CurrentSpread cannot be greater than MaxSpread.");
        }

        // Validate derived properties consistency using pattern matching
        if (value.Median is < 0 or > 100)
        {
            errors.Add("Median should be within reasonable bounds (0-100).");
        }

        if (value.Percentile5 > value.Percentile95)
        {
            errors.Add("Percentile5 cannot be greater than Percentile95.");
        }

        if (value.MinSpread > value.MaxSpread)
        {
            errors.Add("MinSpread cannot be greater than MaxSpread.");
        }

        // Validate statistical consistency: Percentile5 <= Median <= Percentile95
        if (value.Percentile5 > value.Median)
        {
            errors.Add("Percentile5 cannot be greater than Median.");
        }

        if (value.Median > value.Percentile95)
        {
            errors.Add("Median cannot be greater than Percentile95.");
        }

        // Validate that Percentile5 <= Mean <= Percentile95 (approximately)
        if (value.Percentile5 > value.Mean)
        {
            errors.Add("Percentile5 cannot be greater than Mean.");
        }

        if (value.Mean > value.Percentile95)
        {
            errors.Add("Mean cannot be greater than Percentile95.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SpreadStatisticsReport"/> instance is valid.
    /// </summary>
    /// <param name="value">The report to check.</param>
    /// <returns><see langword="true"/> if the report is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SpreadStatisticsReport? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SpreadStatisticsReport"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The report to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the report contains validation errors.</exception>
    public static void EnsureValid(this SpreadStatisticsReport? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SpreadStatisticsReport validation failed:{Environment.NewLine}- {
                    string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }

    private static void ValidateNonNegativeDecimal(IList<string> errors, string propertyName, decimal value)
    {
        if (value < 0)
        {
            errors.Add($"{propertyName} cannot be negative.");
        }
    }
}
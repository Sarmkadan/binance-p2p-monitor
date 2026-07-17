#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Provides validation helpers for PriceMonitoringServiceTests to ensure test data integrity
/// </summary>
public static class PriceMonitoringServiceTestsValidation
{
    /// <summary>
    /// Validates the test setup and configuration
    /// </summary>
    /// <param name="value">The PriceMonitoringServiceTests instance</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this PriceMonitoringServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if the PriceMonitoringServiceTests instance is valid
    /// </summary>
    /// <param name="value">The instance to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this PriceMonitoringServiceTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the PriceMonitoringServiceTests instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid</exception>
    public static void EnsureValid(this PriceMonitoringServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PriceMonitoringServiceTests is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Validates that price data used in tests is valid
    /// </summary>
    /// <param name="price">The price to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="price"/> is null</exception>
    public static IReadOnlyList<string> Validate(this Price price)
    {
        ArgumentNullException.ThrowIfNull(price);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(price.Asset))
        {
            problems.Add("Price.Asset is null or empty");
        }
        else if (price.Asset.Length > 20)
        {
            problems.Add("Price.Asset exceeds maximum length of 20 characters");
        }

        if (string.IsNullOrWhiteSpace(price.Fiat))
        {
            problems.Add("Price.Fiat is null or empty");
        }
        else if (price.Fiat.Length > 10)
        {
            problems.Add("Price.Fiat exceeds maximum length of 10 characters");
        }

        if (price.BuyPrice <= 0)
        {
            problems.Add("Price.BuyPrice must be greater than 0");
        }

        if (price.SellPrice <= 0)
        {
            problems.Add("Price.SellPrice must be greater than 0");
        }

        if (price.SellPrice < price.BuyPrice)
        {
            problems.Add("Price.SellPrice must be greater than or equal to Price.BuyPrice");
        }

        if (price.BuyChangePercent < 0 || price.BuyChangePercent > 100)
        {
            problems.Add("Price.BuyChangePercent must be between 0 and 100");
        }

        if (price.SellChangePercent < 0 || price.SellChangePercent > 100)
        {
            problems.Add("Price.SellChangePercent must be between 0 and 100");
        }

        if (price.Timestamp == default)
        {
            problems.Add("Price.Timestamp must be set to a valid DateTime");
        }

        if (price.CreatedAt == default)
        {
            problems.Add("Price.CreatedAt must be set to a valid DateTime");
        }

        if (price.UpdatedAt == default)
        {
            problems.Add("Price.UpdatedAt must be set to a valid DateTime");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates that AppSettings used in tests is valid
    /// </summary>
    /// <param name="settings">The AppSettings to validate</param>
    /// <returns>A list of validation problems (empty if valid)</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null</exception>
    public static IReadOnlyList<string> Validate(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(settings.DatabaseConnectionString))
        {
            problems.Add("AppSettings.DatabaseConnectionString is null or empty");
        }

        if (settings.MonitoringIntervalSeconds <= 0)
        {
            problems.Add("AppSettings.MonitoringIntervalSeconds must be greater than 0");
        }

        if (settings.AlertCooldownMinutes <= 0)
        {
            problems.Add("AppSettings.AlertCooldownMinutes must be greater than 0");
        }

        if (settings.MaxAlertsPerUser <= 0)
        {
            problems.Add("AppSettings.MaxAlertsPerUser must be greater than 0");
        }

        if (settings.HistoryRetentionDays <= 0)
        {
            problems.Add("AppSettings.HistoryRetentionDays must be greater than 0");
        }

        if (settings.SpreadAnalysisHistoryHours <= 0)
        {
            problems.Add("AppSettings.SpreadAnalysisHistoryHours must be greater than 0");
        }

        if (settings.DefaultPriceChangeThreshold < 0)
        {
            problems.Add("AppSettings.DefaultPriceChangeThreshold cannot be negative");
        }

        if (settings.DefaultSpreadThreshold < 0)
        {
            problems.Add("AppSettings.DefaultSpreadThreshold cannot be negative");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a Price instance is valid
    /// </summary>
    /// <param name="price">The price to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this Price price) => Validate(price).Count == 0;

    /// <summary>
    /// Checks if AppSettings is valid
    /// </summary>
    /// <param name="settings">The settings to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this AppSettings settings) => Validate(settings).Count == 0;

    /// <summary>
    /// Ensures that a Price instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="price">The price to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="price"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the price is invalid</exception>
    public static void EnsureValid(this Price price)
    {
        ArgumentNullException.ThrowIfNull(price);

        var problems = Validate(price);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Price is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that AppSettings is valid, throwing an exception if not
    /// </summary>
    /// <param name="settings">The settings to validate</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when the settings are invalid</exception>
    public static void EnsureValid(this AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var problems = Validate(settings);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AppSettings is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
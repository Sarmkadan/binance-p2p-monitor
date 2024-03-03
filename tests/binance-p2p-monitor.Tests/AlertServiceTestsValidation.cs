#nullable enable

using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Provides validation helpers for <see cref="AlertServiceTests"/> instances.
/// </summary>
public static class AlertServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="AlertServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AlertServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // AlertServiceTests is a test fixture class with only private fields and test methods
        // There are no public members to validate beyond the object itself
        // The class is properly initialized in its constructor

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AlertServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AlertServiceTests? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="AlertServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this AlertServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"AlertServiceTests instance is not valid. Errors: {string.Join("; ", errors)}");
        }
    }
}
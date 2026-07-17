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
    /// <remarks>
    /// AlertServiceTests is a test fixture class containing only private fields and test methods.
    /// As a test class, it has no public members that require validation beyond basic null checks.
    /// The class is properly initialized in its constructor and maintains its own internal state.
    /// </remarks>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AlertServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="AlertServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(this AlertServiceTests? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="AlertServiceTests"/> instance is valid.
    /// </summary>
    /// <remarks>
    /// Throws an <see cref="ArgumentException"/> if the instance is invalid, containing a list of validation problems.
    /// </remarks>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">The instance is invalid.</exception>
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
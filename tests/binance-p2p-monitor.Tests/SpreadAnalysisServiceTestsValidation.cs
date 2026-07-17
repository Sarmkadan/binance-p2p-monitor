using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SpreadAnalysisServiceTests"/> instances.
/// </summary>
public static class SpreadAnalysisServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="SpreadAnalysisServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SpreadAnalysisServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SpreadAnalysisServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SpreadAnalysisServiceTests value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="SpreadAnalysisServiceTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this SpreadAnalysisServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!IsValid(value))
        {
            throw new ArgumentException(
                "SpreadAnalysisServiceTests instance is not valid.");
        }
    }
}
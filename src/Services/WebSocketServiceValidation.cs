#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Provides validation helpers for <see cref="WebSocketService"/> instances.
/// </summary>
public static class WebSocketServiceValidation
{
    /// <summary>
    /// Validates a <see cref="WebSocketService"/> instance and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebSocketService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // WebSocketService doesn't have public properties that need validation
        // The service manages its own state internally

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="WebSocketService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this WebSocketService? value)
    {
        try
        {
            _ = Validate(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="WebSocketService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this WebSocketService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"WebSocketService is not valid. Problems: {string.Join(", ", problems)}");
        }
    }
}
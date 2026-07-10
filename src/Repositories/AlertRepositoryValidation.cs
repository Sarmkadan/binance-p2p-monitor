#nullable enable

using System.Globalization;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Provides validation helpers for AlertRepository operations
/// </summary>
public static class AlertRepositoryValidation
{
    /// <summary>
    /// Validates an AlertRepository instance and returns a list of human-readable problems
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AlertRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the repository's context is not null
        if (value.GetType().GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("AlertRepository context is null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified AlertRepository instance is valid
    /// </summary>
    /// <param name="value">The repository instance to check</param>
    /// <returns>True if valid; otherwise false</returns>
    public static bool IsValid(this AlertRepository value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified AlertRepository instance is valid, throwing an exception if not
    /// </summary>
    /// <param name="value">The repository instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if the repository is invalid, containing the list of problems</exception>
    public static void EnsureValid(this AlertRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AlertRepository is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}
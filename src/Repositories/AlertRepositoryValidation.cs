#nullable enable

using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="AlertRepository"/> instances.
/// </summary>
/// <remarks>
/// The <see cref="AlertRepository"/> constructor performs its own validation via <see cref="ArgumentNullException.ThrowIfNull"/>
/// on the injected <see cref="DatabaseContext"/>. This validation class is provided for consistency with the
/// project's validation pattern but is not strictly necessary for AlertRepository instances.
/// </remarks>
public static class AlertRepositoryValidation
{
    private static readonly Type _alertRepositoryType = typeof(AlertRepository);
    private static readonly System.Reflection.FieldInfo _contextField = _alertRepositoryType.GetField(
        "_context",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        ?? throw new InvalidOperationException("Cannot find _context field in AlertRepository");

    /// <summary>
    /// Validates an AlertRepository instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of validation error messages; empty if the repository is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the AlertRepository type structure is invalid.</exception>
    public static IReadOnlyList<string> Validate(this AlertRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the repository's context is not null
        // Note: AlertRepository constructor already validates this, but we check for consistency
        if (_contextField.GetValue(value) is null)
        {
            problems.Add("AlertRepository context (_context) cannot be null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified AlertRepository instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns>True if the repository is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AlertRepository? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified AlertRepository instance is valid, throwing an <see cref="ArgumentException"/> if validation fails.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the repository instance has validation problems.</exception>
    public static void EnsureValid(this AlertRepository? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AlertRepository validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}",
                nameof(value));
        }
    }
}
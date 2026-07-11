using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="PriceRepositoryTests"/> instances.
    /// </summary>
    public static class PriceRepositoryTestsValidation
    {
        /// <summary>
        /// Validates a <see cref="PriceRepositoryTests"/> instance and returns a list of human-readable problems.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this PriceRepositoryTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // PriceRepositoryTests is a test fixture class with only private fields and test methods
            // There are no public members to validate beyond the object itself
            // The class implements IDisposable which is properly implemented

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether a <see cref="PriceRepositoryTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this PriceRepositoryTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that a <see cref="PriceRepositoryTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of validation errors.</exception>
        public static void EnsureValid(this PriceRepositoryTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"PriceRepositoryTests instance is not valid. Errors: {string.Join("; ", errors)}");
            }
        }
    }
}

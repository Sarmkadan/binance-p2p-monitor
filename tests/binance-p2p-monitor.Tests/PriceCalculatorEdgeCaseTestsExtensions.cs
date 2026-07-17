#nullable enable

using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public static class PriceCalculatorEdgeCaseTestsExtensions
{
    /// <summary>
    /// Creates a test case for percentage change calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    public static TheoryData<decimal, decimal, decimal> CreatePercentageChangeTestData(
        this PriceCalculatorEdgeCaseTests _) =>
        new()
        {
            // Zero original price scenarios
            { 0m, 0m, 0m },
            { 0m, 100m, 0m },
            { 0m, -100m, 0m },

            // Zero new price scenarios
            { 100m, 0m, -100m },
            { 50m, 0m, -100m },

            // Large values
            { decimal.MaxValue, decimal.MaxValue, 0m },
            { 1000000m, 2000000m, 100m },

            // Very small values
            { 0.0001m, 0.0002m, 100m },
            { 0.0001m, 0.0001m, 0m },

            // Negative values
            { -100m, -50m, 50m },
            { -50m, -100m, -100m }
        };

    /// <summary>
    /// Creates a test case for spread calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    public static TheoryData<decimal, decimal, decimal> CreateSpreadTestData(
        this PriceCalculatorEdgeCaseTests _) =>
        new()
        {
            // Zero buy price
            { 0m, 0m, 0m },
            { 0m, 100m, 0m },
            { 0m, -100m, 0m },

            // Zero sell price
            { 100m, 0m, -100m },
            { -100m, 0m, 100m },

            // Equal prices
            { 100m, 100m, 0m },
            { -50m, -50m, 0m },

            // Large spreads
            { 100m, 200m, 100m },
            { 1000m, 500m, -50m },

            // Very small spreads
            { 100.0001m, 100.0002m, 0.0001m }
        };

    /// <summary>
    /// Creates a test case for moving average calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    public static TheoryData<int, decimal> CreateMovingAverageTestData(
        this PriceCalculatorEdgeCaseTests _) =>
        new()
        {
            // Zero period
            { 0, 0m },

            // Negative periods
            { -1, 0m },
            { -100, 0m },

            // Period larger than count
            { 10, 2m }, // (1+2+3)/3 = 2
            { 100, 2m }, // Same as above

            // Single item
            { 1, 1m },
            { 5, 1m }
        };

    /// <summary>
    /// Creates a test case for standard deviation calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input is null.</exception>
    public static TheoryData<string, decimal> CreateStandardDeviationTestData(
        this PriceCalculatorEdgeCaseTests _)
    {
        ArgumentNullException.ThrowIfNull(_);

        return new()
        {
            // Empty collection
            { string.Empty, 0m },

            // Single item
            { "100", 0m },
            { "50", 0m },

            // All same values
            { "100,100,100", 0m },
            { "0,0,0,0", 0m },

            // Two items
            { "1,3", 1m },

            // Negative values
            { "-1,-3,-5", 2m },

            // Large values
            { "1000,2000,3000", 816.496580927726m }
        };
    }

    /// <summary>
    /// Validates that the PriceCalculator methods handle null collections correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="action">The action that should throw.</param>
    /// <exception cref="ArgumentNullException">Thrown if test is null.</exception>
    public static void ShouldThrowWhenPricesIsNull(
        this PriceCalculatorEdgeCaseTests test,
        Action action) =>
        action.Should().Throw<ArgumentNullException>(
            because: "PriceCalculator methods should throw ArgumentNullException for null prices collections");

    /// <summary>
    /// Validates that the PriceCalculator methods handle empty collections correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="prices">The prices collection.</param>
    /// <param name="expectedResult">The expected result.</param>
    /// <exception cref="ArgumentNullException">Thrown if prices is null.</exception>
    public static void ShouldReturnZeroForEmptyCollection(
        this PriceCalculatorEdgeCaseTests test,
        IEnumerable<decimal> prices,
        decimal expectedResult) =>
        PriceCalculator.CalculateMovingAverage(prices, 5).Should().Be(expectedResult);

    /// <summary>
    /// Creates a comprehensive test suite for percentage change edge cases.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>An enumerable of test case tuples.</returns>
    public static IEnumerable<(decimal Original, decimal NewPrice, decimal Expected)> GetPercentageChangeEdgeCases(
        this PriceCalculatorEdgeCaseTests test)
    {
        yield return (0m, 0m, 0m);
        yield return (0m, 100m, 0m);
        yield return (0m, -100m, 0m);
        yield return (100m, 0m, -100m);
        yield return (decimal.MaxValue, decimal.MaxValue, 0m);
        yield return (-100m, -50m, 50m);
        yield return (0.0001m, 0.0002m, 100m);
    }

    /// <summary>
    /// Creates a comprehensive test suite for spread edge cases.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <returns>An enumerable of test case tuples.</returns>
    public static IEnumerable<(decimal BuyPrice, decimal SellPrice, decimal Expected)> GetSpreadEdgeCases(
        this PriceCalculatorEdgeCaseTests test)
    {
        yield return (0m, 0m, 0m);
        yield return (0m, 100m, 0m);
        yield return (100m, 0m, -100m);
        yield return (100m, 100m, 0m);
        yield return (100m, 200m, 100m);
        yield return (1000m, 500m, -50m);
    }
}
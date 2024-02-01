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
        this PriceCalculatorEdgeCaseTests _)
    {
        var data = new TheoryData<decimal, decimal, decimal>();

        // Zero original price scenarios
        data.Add(0m, 0m, 0m);
        data.Add(0m, 100m, 0m);
        data.Add(0m, -100m, 0m);

        // Zero new price scenarios
        data.Add(100m, 0m, -100m);
        data.Add(50m, 0m, -100m);

        // Large values
        data.Add(decimal.MaxValue, decimal.MaxValue, 0m);
        data.Add(1000000m, 2000000m, 100m);

        // Very small values
        data.Add(0.0001m, 0.0002m, 100m);
        data.Add(0.0001m, 0.0001m, 0m);

        // Negative values
        data.Add(-100m, -50m, 50m);
        data.Add(-50m, -100m, -100m);

        return data;
    }

    /// <summary>
    /// Creates a test case for spread calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    public static TheoryData<decimal, decimal, decimal> CreateSpreadTestData(
        this PriceCalculatorEdgeCaseTests _)
    {
        var data = new TheoryData<decimal, decimal, decimal>();

        // Zero buy price
        data.Add(0m, 0m, 0m);
        data.Add(0m, 100m, 0m);
        data.Add(0m, -100m, 0m);

        // Zero sell price
        data.Add(100m, 0m, -100m);
        data.Add(-100m, 0m, 100m);

        // Equal prices
        data.Add(100m, 100m, 0m);
        data.Add(-50m, -50m, 0m);

        // Large spreads
        data.Add(100m, 200m, 100m);
        data.Add(1000m, 500m, -50m);

        // Very small spreads
        data.Add(100.0001m, 100.0002m, 0.0001m);

        return data;
    }

    /// <summary>
    /// Creates a test case for moving average calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    public static TheoryData<int, decimal> CreateMovingAverageTestData(
        this PriceCalculatorEdgeCaseTests _)
    {
        var data = new TheoryData<int, decimal>();

        // Zero period
        data.Add(0, 0m);

        // Negative periods
        data.Add(-1, 0m);
        data.Add(-100, 0m);

        // Period larger than count
        data.Add(10, 2m); // (1+2+3)/3 = 2
        data.Add(100, 2m); // Same as above

        // Single item
        data.Add(1, 1m);
        data.Add(5, 1m);

        return data;
    }

    /// <summary>
    /// Creates a test case for standard deviation calculations with edge cases.
    /// </summary>
    /// <param name="_">The test instance (unused parameter for extension method).</param>
    /// <returns>A TheoryData entry for xUnit test parameterization.</returns>
    public static TheoryData<string, decimal> CreateStandardDeviationTestData(
        this PriceCalculatorEdgeCaseTests _)
    {
        var data = new TheoryData<string, decimal>();

        // Empty collection
        data.Add(string.Empty, 0m);

        // Single item
        data.Add("100", 0m);
        data.Add("50", 0m);

        // All same values
        data.Add("100,100,100", 0m);
        data.Add("0,0,0,0", 0m);

        // Two items
        data.Add("1,3", 1m);

        // Negative values
        data.Add("-1,-3,-5", 2m);

        // Large values
        data.Add("1000,2000,3000", 816.496580927726m);

        return data;
    }

    /// <summary>
    /// Validates that the PriceCalculator methods handle null collections correctly.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="action">The action that should throw.</param>
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
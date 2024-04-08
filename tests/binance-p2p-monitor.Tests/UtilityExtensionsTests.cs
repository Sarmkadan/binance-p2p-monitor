#nullable enable
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for utility extension methods, including date/time, enumerable,
/// numeric, string extensions, and validation helper methods.
/// </summary>
public class UtilityExtensionsTests
{
    // DateTimeExtensions Tests
    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.ToUnixTimestampMs"/> correctly converts a
    /// <see cref="DateTime"/> to a Unix timestamp in milliseconds.
    /// </summary>
    [Fact]
    public void ToUnixTimestampMs_ShouldReturnCorrectTimestamp()
    {
        var dateTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dateTime.ToUnixTimestampMs().Should().Be(1672531200000);
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.GetTimeAgoString"/> returns the correct
    /// "time ago" string representation.
    /// </summary>
    [Fact]
    public void GetTimeAgoString_ShouldReturnCorrectString_ForMinutes()
    {
        var dateTime = DateTime.UtcNow.AddMinutes(-5);
        dateTime.GetTimeAgoString().Should().Be("5m ago");
    }

    // EnumerableExtensions Tests
    /// <summary>
    /// Verifies that <see cref="Enumerable.Chunk{TSource}(IEnumerable{TSource}, int)"/> splits an
    /// <see cref="IEnumerable{T}"/> into chunks of the specified size.
    /// </summary>
    [Fact]
    public void Chunk_ShouldReturnCorrectChunks()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var chunks = Enumerable.Chunk(source, 2).ToList();
        chunks.Should().HaveCount(3);
        chunks[0].Should().ContainInOrder(1, 2);
        chunks[1].Should().ContainInOrder(3, 4);
        chunks[2].Should().ContainInOrder(5);
    }

    /// <summary>
    /// Verifies that <see cref="EnumerableExtensions.FirstOrNull{TSource}(IEnumerable{TSource})"/> returns the
    /// first element of an <see cref="IEnumerable{T}"/> or null if the collection is empty.
    /// </summary>
    [Fact]
    public void FirstOrNull_ShouldReturnFirstItemOrNull()
    {
        var source = new List<string> { "first", "second" };
        source.FirstOrNull().Should().Be("first");

        var emptySource = new List<string>();
        emptySource.FirstOrNull().Should().BeNull();
    }

    // NumericExtensions Tests
    /// <summary>
    /// Verifies that <see cref="NumericExtensions.RoundTo"/> correctly rounds a
    /// <see cref="decimal"/> value to the specified number of decimal places.
    /// </summary>
    [Fact]
    public void RoundTo_ShouldRoundDecimalCorrectly()
    {
        123.456m.RoundTo(2).Should().Be(123.46m);
        123.454m.RoundTo(2).Should().Be(123.45m);
    }

    /// <summary>
    /// Verifies that <see cref="NumericExtensions.CalculatePercentageChange"/> correctly
    /// calculates the percentage change between two values.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_ShouldReturnCorrectChange()
    {
        110m.CalculatePercentageChange(100m).Should().Be(10m);
        90m.CalculatePercentageChange(100m).Should().Be(-10m);
        100m.CalculatePercentageChange(0m).Should().Be(0m);
    }

    // StringExtensions Tests
    /// <summary>
    /// Verifies that <see cref="StringExtensions.Truncate"/> truncates a <see cref="string"/>
    /// to a maximum length and appends an ellipsis if necessary.
    /// </summary>
    /// <param name="input">The input string to truncate.</param>
    /// <param name="maxLength">The maximum allowed length of the string.</param>
    /// <param name="expected">The expected truncated result.</param>
    [Theory]
    [InlineData("LongStringExample", 5, "Lo...")]
    [InlineData("Short", 10, "Short")]
    [InlineData(null, 5, "")]
    public void Truncate_ShouldTruncateStringCorrectly(string? input, int maxLength, string expected)
    {
        input.Truncate(maxLength).Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="StringExtensions.ToSnakeCase"/> converts a string from
    /// PascalCase or camelCase to snake_case.
    /// </summary>
    [Fact]
    public void ToSnakeCase_ShouldConvertCorrectly()
    {
        "PascalCaseString".ToSnakeCase().Should().Be("pascal_case_string");
        "camelCaseString".ToSnakeCase().Should().Be("camel_case_string");
    }

    // ValidationHelper Tests
    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidEmail"/> correctly validates email addresses.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ShouldReturnCorrectResult(string email, bool expected)
    {
        ValidationHelper.IsValidEmail(email).Should().Be(expected);
    }

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidTicker"/> correctly validates ticker strings.
    /// </summary>
    /// <param name="ticker">The ticker symbol to validate.</param>
    /// <param name="expected">The expected validation result.</param>
    [Theory]
    [InlineData("USDT", true)]
    [InlineData("USDT-UAH", false)]
    [InlineData(null, false)]
    public void IsValidTicker_ShouldReturnCorrectResult(string ticker, bool expected)
    {
        ValidationHelper.IsValidTicker(ticker).Should().Be(expected);
    }
}

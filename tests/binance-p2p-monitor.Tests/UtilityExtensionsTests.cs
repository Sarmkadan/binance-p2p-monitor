#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class UtilityExtensionsTests
{
    // DateTimeExtensions Tests
    [Fact]
    public void ToUnixTimestampMs_ShouldReturnCorrectTimestamp()
    {
        var dateTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dateTime.ToUnixTimestampMs().Should().Be(1672531200000);
    }

    [Fact]
    public void GetTimeAgoString_ShouldReturnCorrectString_ForMinutes()
    {
        var dateTime = DateTime.UtcNow.AddMinutes(-5);
        dateTime.GetTimeAgoString().Should().Be("5m ago");
    }

    // EnumerableExtensions Tests
    [Fact]
    public void Chunk_ShouldReturnCorrectChunks()
    {
        var source = new List<int> { 1, 2, 3, 4, 5 };
        var chunks = source.Chunk(2).ToList();
        chunks.Should().HaveCount(3);
        chunks[0].Should().ContainInOrder(1, 2);
        chunks[1].Should().ContainInOrder(3, 4);
        chunks[2].Should().ContainInOrder(5);
    }

    [Fact]
    public void FirstOrNull_ShouldReturnFirstItemOrNull()
    {
        var source = new List<string> { "first", "second" };
        source.FirstOrNull().Should().Be("first");

        var emptySource = new List<string>();
        emptySource.FirstOrNull().Should().BeNull();
    }

    // NumericExtensions Tests
    [Fact]
    public void RoundTo_ShouldRoundDecimalCorrectly()
    {
        123.456m.RoundTo(2).Should().Be(123.46m);
        123.454m.RoundTo(2).Should().Be(123.45m);
    }

    [Fact]
    public void CalculatePercentageChange_ShouldReturnCorrectChange()
    {
        110m.CalculatePercentageChange(100m).Should().Be(10m);
        90m.CalculatePercentageChange(100m).Should().Be(-10m);
        100m.CalculatePercentageChange(0m).Should().Be(0m);
    }

    // StringExtensions Tests
    [Theory]
    [InlineData("LongStringExample", 5, "LongS...")]
    [InlineData("Short", 10, "Short")]
    [InlineData(null, 5, "")]
    public void Truncate_ShouldTruncateStringCorrectly(string? input, int maxLength, string expected)
    {
        input.Truncate(maxLength).Should().Be(expected);
    }

    [Fact]
    public void ToSnakeCase_ShouldConvertCorrectly()
    {
        "PascalCaseString".ToSnakeCase().Should().Be("pascal_case_string");
        "camelCaseString".ToSnakeCase().Should().Be("camel_case_string");
    }

    // ValidationHelper Tests
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ShouldReturnCorrectResult(string email, bool expected)
    {
        ValidationHelper.IsValidEmail(email).Should().Be(expected);
    }

    [Theory]
    [InlineData("USDT", true)]
    [InlineData("USDT-UAH", false)]
    [InlineData(null, false)]
    public void IsValidTicker_ShouldReturnCorrectResult(string ticker, bool expected)
    {
        ValidationHelper.IsValidTicker(ticker).Should().Be(expected);
    }
}

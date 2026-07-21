#nullable enable
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for DateTime extension methods.
/// </summary>
public class DateTimeExtensionsUnitTests
{
#region ToUnixTimestampMs Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.ToUnixTimestampMs"/> correctly converts a
    /// <see cref="DateTime"/> to a Unix timestamp in milliseconds.
    /// </summary>
    [Fact]
    public void ToUnixTimestampMs_ShouldReturnCorrectTimestamp()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        long result = dateTime.ToUnixTimestampMs();

        // Assert
        result.Should().Be(1672531200000);
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.ToUnixTimestampMs"/> correctly converts current time to Unix timestamp.
    /// </summary>
    [Fact]
    public void ToUnixTimestampMs_ShouldReturnValidTimestampForCurrentTime()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        long result = dateTime.ToUnixTimestampMs();

        // Assert
        result.Should().BeGreaterThan(0);
    }

#endregion

#region FromUnixTimestamp Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.FromUnixTimestamp"/> correctly converts a
    /// Unix timestamp to a <see cref="DateTime"/>.
    /// </summary>
    [Fact]
    public void FromUnixTimestamp_ShouldReturnCorrectDateTime()
    {
        // Arrange
        long timestamp = 1672531200; // 2023-01-01 00:00:00 UTC

        // Act
        DateTime result = DateTimeExtensions.FromUnixTimestamp(timestamp);

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.FromUnixTimestamp"/> throws <see cref="ArgumentOutOfRangeException"/>
    /// when the timestamp is negative.
    /// </summary>
    [Fact]
    public void FromUnixTimestamp_ShouldThrowArgumentOutOfRangeException_WhenTimestampIsNegative()
    {
        // Arrange
        long timestamp = -1;

        // Act
        Action act = () => DateTimeExtensions.FromUnixTimestamp(timestamp);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.FromUnixTimestamp"/> handles zero timestamp correctly.
    /// </summary>
    [Fact]
    public void FromUnixTimestamp_ShouldReturnEpochForZeroTimestamp()
    {
        // Arrange
        long timestamp = 0;

        // Act
        DateTime result = DateTimeExtensions.FromUnixTimestamp(timestamp);

        // Assert
        result.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

#endregion

#region FromUnixTimestampMs Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.FromUnixTimestampMs"/> correctly converts a
    /// Unix timestamp in milliseconds to a <see cref="DateTime"/>.
    /// </summary>
    [Fact]
    public void FromUnixTimestampMs_ShouldReturnCorrectDateTime()
    {
        // Arrange
        long timestampMs = 1672531200000; // 2023-01-01 00:00:00 UTC

        // Act
        DateTime result = DateTimeExtensions.FromUnixTimestampMs(timestampMs);

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.FromUnixTimestampMs"/> throws <see cref="ArgumentOutOfRangeException"/>
    /// when the timestamp is negative.
    /// </summary>
    [Fact]
    public void FromUnixTimestampMs_ShouldThrowArgumentOutOfRangeException_WhenTimestampIsNegative()
    {
        // Arrange
        long timestampMs = -1;

        // Act
        Action act = () => DateTimeExtensions.FromUnixTimestampMs(timestampMs);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.FromUnixTimestampMs"/> handles zero timestamp correctly.
    /// </summary>
    [Fact]
    public void FromUnixTimestampMs_ShouldReturnEpochForZeroTimestampMs()
    {
        // Arrange
        long timestampMs = 0;

        // Act
        DateTime result = DateTimeExtensions.FromUnixTimestampMs(timestampMs);

        // Assert
        result.Should().Be(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

#endregion

#region GetTimeAgoString Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.GetTimeAgoString"/> returns the correct
    /// "time ago" string representation for seconds.
    /// </summary>
    [Fact]
    public void GetTimeAgoString_ShouldReturnSecondsAgo_ForRecentTime()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddSeconds(-30);

        // Act
        string result = dateTime.GetTimeAgoString();

        // Assert
        result.Should().Be("30s ago");
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.GetTimeAgoString"/> returns the correct
    /// "time ago" string representation for minutes.
    /// </summary>
    [Fact]
    public void GetTimeAgoString_ShouldReturnMinutesAgo_ForMinutesAgo()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddMinutes(-5);

        // Act
        string result = dateTime.GetTimeAgoString();

        // Assert
        result.Should().Be("5m ago");
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.GetTimeAgoString"/> returns the correct
    /// "time ago" string representation for hours.
    /// </summary>
    [Fact]
    public void GetTimeAgoString_ShouldReturnHoursAgo_ForHoursAgo()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddHours(-2);

        // Act
        string result = dateTime.GetTimeAgoString();

        // Assert
        result.Should().Be("2h ago");
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.GetTimeAgoString"/> returns the correct
    /// "time ago" string representation for days.
    /// </summary>
    [Fact]
    public void GetTimeAgoString_ShouldReturnDaysAgo_ForDaysAgo()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddDays(-3);

        // Act
        string result = dateTime.GetTimeAgoString();

        // Assert
        result.Should().Be("3d ago");
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.GetTimeAgoString"/> returns formatted date for old dates.
    /// </summary>
    [Fact]
    public void GetTimeAgoString_ShouldReturnFormattedDate_ForOldDate()
    {
        // Arrange
        var dateTime = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        string result = dateTime.GetTimeAgoString();

        // Assert
        result.Should().Be("2020-01-01 12:00:00");
    }

#endregion

#region RoundTo Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.RoundTo"/> correctly rounds a DateTime to the nearest interval.
    /// </summary>
    [Fact]
    public void RoundTo_ShouldRoundToNearestInterval()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 12, 7, 30, DateTimeKind.Utc); // 12:07:30
        var interval = TimeSpan.FromMinutes(5); // 5-minute intervals

        // Act
        DateTime result = dateTime.RoundTo(interval);

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 12, 10, 0, DateTimeKind.Utc)); // Should round to 12:10:00
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.RoundTo"/> rounds up when closer to upper bound.
    /// </summary>
    [Fact]
    public void RoundTo_ShouldRoundUp_WhenCloserToUpperBound()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 12, 2, 30, DateTimeKind.Utc); // 12:02:30
        var interval = TimeSpan.FromMinutes(5); // 5-minute intervals

        // Act
        DateTime result = dateTime.RoundTo(interval);

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 12, 5, 0, DateTimeKind.Utc)); // Should round to 12:05:00
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.RoundTo"/> throws <see cref="ArgumentOutOfRangeException"/>
    /// when the interval has zero or negative ticks.
    /// </summary>
    [Fact]
    public void RoundTo_ShouldThrowArgumentOutOfRangeException_WhenIntervalHasZeroOrNegativeTicks()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;
        var interval = TimeSpan.FromTicks(-1);

        // Act
        Action act = () => dateTime.RoundTo(interval);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

#endregion

#region StartOfDay Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.StartOfDay"/> correctly returns the start of the day.
    /// </summary>
    [Fact]
    public void StartOfDay_ShouldReturnStartOfDay()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 15, 30, 45, DateTimeKind.Utc);

        // Act
        DateTime result = dateTime.StartOfDay();

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

#endregion

#region EndOfDay Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.EndOfDay"/> correctly returns the end of the day.
    /// </summary>
    [Fact]
    public void EndOfDay_ShouldReturnEndOfDay()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 1, 15, 30, 45, DateTimeKind.Utc);

        // Act
        DateTime result = dateTime.EndOfDay();

        // Assert
        result.Year.Should().Be(2023);
        result.Month.Should().Be(1);
        result.Day.Should().Be(1);
        result.Hour.Should().Be(23);
        result.Minute.Should().Be(59);
        result.Second.Should().Be(59);
        result.Millisecond.Should().Be(999);
    }

#endregion

#region StartOfWeek Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.StartOfWeek"/> correctly returns the start of the week (Monday).
    /// </summary>
    [Fact]
    public void StartOfWeek_ShouldReturnStartOfWeek_Monday()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 5, 15, 30, 45, DateTimeKind.Utc); // Thursday

        // Act
        DateTime result = dateTime.StartOfWeek();

        // Assert
        result.Should().Be(new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc)); // Monday
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.StartOfWeek"/> correctly returns the start of the week (Monday)
    /// when the date is already a Monday.
    /// </summary>
    [Fact]
    public void StartOfWeek_ShouldReturnSameDate_WhenAlreadyMonday()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 2, 15, 30, 45, DateTimeKind.Utc); // Monday

        // Act
        DateTime result = dateTime.StartOfWeek();

        // Assert
        result.Should().Be(new DateTime(2023, 1, 2, 0, 0, 0, DateTimeKind.Utc));
    }

#endregion

#region StartOfMonth Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.StartOfMonth"/> correctly returns the start of the month.
    /// </summary>
    [Fact]
    public void StartOfMonth_ShouldReturnStartOfMonth()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 15, 15, 30, 45, DateTimeKind.Utc);

        // Act
        DateTime result = dateTime.StartOfMonth();

        // Assert
        result.Should().Be(new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

#endregion

#region EndOfMonth Tests

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.EndOfMonth"/> correctly returns the end of the month.
    /// </summary>
    [Fact]
    public void EndOfMonth_ShouldReturnEndOfMonth()
    {
        // Arrange
        var dateTime = new DateTime(2023, 1, 15, 15, 30, 45, DateTimeKind.Utc);

        // Act
        DateTime result = dateTime.EndOfMonth();

        // Assert
        result.Year.Should().Be(2023);
        result.Month.Should().Be(1);
        result.Day.Should().Be(31);
        result.Hour.Should().Be(23);
        result.Minute.Should().Be(59);
        result.Second.Should().Be(59);
        result.Millisecond.Should().Be(999);
    }

    /// <summary>
    /// Verifies that <see cref="DateTimeExtensions.EndOfMonth"/> correctly handles February in a leap year.
    /// </summary>
    [Fact]
    public void EndOfMonth_ShouldReturnCorrectEndOfFebruaryInLeapYear()
    {
        // Arrange
        var dateTime = new DateTime(2024, 2, 15, 15, 30, 45, DateTimeKind.Utc); // 2024 is a leap year

        // Act
        DateTime result = dateTime.EndOfMonth();

        // Assert
        result.Year.Should().Be(2024);
        result.Month.Should().Be(2);
        result.Day.Should().Be(29);
        result.Hour.Should().Be(23);
        result.Minute.Should().Be(59);
        result.Second.Should().Be(59);
        result.Millisecond.Should().Be(999);
    }

#endregion
}
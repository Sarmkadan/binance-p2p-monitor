#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp in milliseconds
    /// </summary>
    public static long ToUnixTimestampMs(this DateTime dateTime)
    {
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Gets a human-readable time difference string
    /// </summary>
    public static string GetTimeAgoString(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        return timeSpan.TotalSeconds < 60
            ? $"{(int)timeSpan.TotalSeconds}s ago"
            : timeSpan.TotalMinutes < 60
            ? $"{(int)timeSpan.TotalMinutes}m ago"
            : timeSpan.TotalHours < 24
            ? $"{(int)timeSpan.TotalHours}h ago"
            : timeSpan.TotalDays < 30
            ? $"{(int)timeSpan.TotalDays}d ago"
            : dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// Rounds DateTime to nearest interval
    /// </summary>
    public static DateTime RoundTo(this DateTime dateTime, TimeSpan interval)
    {
        var offset = dateTime.Ticks % interval.Ticks;
        return offset < interval.Ticks / 2
            ? dateTime.AddTicks(-offset)
            : dateTime.AddTicks(interval.Ticks - offset);
    }

    /// <summary>
    /// Gets the start of day
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime) => dateTime.Date;

    /// <summary>
    /// Gets the end of day
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime) => dateTime.Date.AddDays(1).AddTicks(-1);

    /// <summary>
    /// Gets the start of week (Monday)
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var days = (int)dateTime.DayOfWeek - 1;
        if (days < 0) days = 6;
        return dateTime.AddDays(-days).Date;
    }

    /// <summary>
    /// Gets the start of month
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime) => new(dateTime.Year, dateTime.Month, 1);

    /// <summary>
    /// Gets the end of month
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        var nextMonth = dateTime.AddMonths(1);
        return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).EndOfDay();
    }
}

#nullable enable

using System;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp in milliseconds
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>Unix timestamp in milliseconds</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static long ToUnixTimestampMs(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return (long)(dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    /// <param name="timestamp">Unix timestamp in seconds</param>
    /// <returns>DateTime representing the timestamp</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timestamp"/> is negative</exception>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        if (timestamp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timestamp), "Timestamp cannot be negative");
        }
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Converts Unix timestamp in milliseconds to DateTime
    /// </summary>
    /// <param name="timestampMs">Unix timestamp in milliseconds</param>
    /// <returns>DateTime representing the timestamp</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timestampMs"/> is negative</exception>
    public static DateTime FromUnixTimestampMs(long timestampMs)
    {
        if (timestampMs < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timestampMs), "Timestamp cannot be negative");
        }
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampMs);
    }

    /// <summary>
    /// Gets a human-readable time difference string
    /// </summary>
    /// <param name="dateTime">The DateTime to convert to relative string</param>
    /// <returns>Human-readable time difference string</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static string GetTimeAgoString(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        return timeSpan.TotalSeconds switch
        {
            < 60 => $"{(int)timeSpan.TotalSeconds}s ago",
            < 3600 => $"{(int)timeSpan.TotalMinutes}m ago",
            < 86400 => $"{(int)timeSpan.TotalHours}h ago",
            < 2592000 => $"{(int)timeSpan.TotalDays}d ago",
            _ => dateTime.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }

    /// <summary>
    /// Rounds DateTime to nearest interval
    /// </summary>
    /// <param name="dateTime">The DateTime to round</param>
    /// <param name="interval">The rounding interval</param>
    /// <returns>Rounded DateTime</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> or <paramref name="interval"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> has zero or negative ticks</exception>
    public static DateTime RoundTo(this DateTime dateTime, TimeSpan interval)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        ArgumentNullException.ThrowIfNull(interval);

        if (interval.Ticks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must have positive ticks");
        }

        var offset = dateTime.Ticks % interval.Ticks;
        return offset < interval.Ticks / 2
            ? dateTime.AddTicks(-offset)
            : dateTime.AddTicks(interval.Ticks - offset);
    }

    /// <summary>
    /// Gets the start of day
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of day (00:00:00)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of day
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at end of day (23:59:59.9999999)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of week (Monday)
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of week (Monday 00:00:00)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        var days = (int)dateTime.DayOfWeek - 1;
        return days < 0
            ? dateTime.AddDays(-6).Date
            : dateTime.AddDays(-days).Date;
    }

    /// <summary>
    /// Gets the start of month
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at start of month (1st day 00:00:00)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of month
    /// </summary>
    /// <param name="dateTime">The DateTime</param>
    /// <returns>DateTime at end of month (last day 23:59:59.9999999)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dateTime"/> is null</exception>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        ArgumentNullException.ThrowIfNull(dateTime);
        var nextMonth = dateTime.AddMonths(1);
        return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).EndOfDay();
    }
}
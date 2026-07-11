# DateTimeExtensions

The `DateTimeExtensions` class provides a set of static utility methods designed to simplify common date and time operations within the `binance-p2p-monitor` project. It facilitates seamless conversion between .NET `DateTime` objects and Unix timestamps, generates human-readable relative time strings, and offers precise rounding and boundary calculation methods for days, weeks, and months, ensuring consistent time handling across the monitoring logic.

## API

### ToUnixTimestampMs
Converts a `DateTime` instance to a Unix timestamp represented in milliseconds.
*   **Parameters**: `this DateTime dateTime` (The date and time to convert).
*   **Returns**: `long` representing the number of milliseconds elapsed since the Unix epoch (January 1, 1970, 00:00:00 UTC).
*   **Throws**: None.

### FromUnixTimestamp
Converts a Unix timestamp in milliseconds to a `DateTime` instance.
*   **Parameters**: `long unixTimestampMs` (The Unix timestamp in milliseconds).
*   **Returns**: `DateTime` representing the corresponding point in time.
*   **Throws**: None. Note: Extremely large or negative values may result in `DateTime` overflow exceptions depending on the .NET runtime implementation, though standard Binance API timestamps fall within safe ranges.

### GetTimeAgoString
Generates a human-readable string describing the time elapsed between a specific date and the current moment (e.g., "5 minutes ago", "2 days ago").
*   **Parameters**: `this DateTime dateTime` (The past date to evaluate).
*   **Returns**: `string` containing the relative time description.
*   **Throws**: None.

### RoundTo
Rounds a `DateTime` instance to the nearest specified time interval.
*   **Parameters**: `this DateTime dateTime`, `TimeSpan interval` (The time span to round to, e.g., 1 minute, 15 minutes).
*   **Returns**: `DateTime` rounded to the nearest multiple of the provided interval.
*   **Throws**: `ArgumentOutOfRangeException` if the provided `interval` is zero or negative.

### StartOfDay
Returns the beginning of the day (00:00:00) for the specified date.
*   **Parameters**: `this DateTime dateTime`.
*   **Returns**: `DateTime` representing midnight of the same day as the input.
*   **Throws**: None.

### EndOfDay
Returns the end of the day (23:59:59.999) for the specified date.
*   **Parameters**: `this DateTime dateTime`.
*   **Returns**: `DateTime` representing the last millisecond of the same day as the input.
*   **Throws**: None.

### StartOfWeek
Returns the first day of the week containing the specified date.
*   **Parameters**: `this DateTime dateTime`.
*   **Returns**: `DateTime` representing midnight of the first day of the week (typically Monday, depending on culture settings if not explicitly forced).
*   **Throws**: None.

### StartOfMonth
Returns the first day of the month containing the specified date.
*   **Parameters**: `this DateTime dateTime`.
*   **Returns**: `DateTime` representing midnight of the first day of the month.
*   **Throws**: None.

### EndOfMonth
Returns the last day of the month containing the specified date.
*   **Parameters**: `this DateTime dateTime`.
*   **Returns**: `DateTime` representing the last millisecond of the last day of the month.
*   **Throws**: None.

## Usage

### Converting Binance API Timestamps
Binance APIs typically return timestamps as long integers representing milliseconds since the Unix epoch. These extensions simplify parsing and formatting those values for local processing.

```csharp
using BinanceP2PMonitor.Extensions;

// Simulate a timestamp received from the Binance P2P API
long apiTimestamp = 1715623456789;

// Convert to DateTime for local logic
DateTime tradeTime = DateTimeExtensions.FromUnixTimestamp(apiTimestamp);

// Perform operations
DateTime tradeStartOfDay = tradeTime.StartOfDay();

// Convert back to milliseconds for storage or re-transmission
long storedTimestamp = tradeStartOfDay.ToUnixTimestampMs();
```

### Generating Time-Ago Reports and Rounding
When displaying trade history or aggregating data into time buckets, relative time strings and rounded intervals are often required.

```csharp
using BinanceP2PMonitor.Extensions;

DateTime lastTradeTime = DateTime.UtcNow.AddMinutes(-15);

// Generate a user-friendly string for the UI
string relativeTime = lastTradeTime.GetTimeAgoString(); 
// Output example: "15 minutes ago"

// Round the trade time to the nearest 15-minute candle interval
TimeSpan candleInterval = TimeSpan.FromMinutes(15);
DateTime roundedCandleTime = lastTradeTime.RoundTo(candleInterval);

// Calculate the full range for a daily aggregation
DateTime dayStart = lastTradeTime.StartOfDay();
DateTime dayEnd = lastTradeTime.EndOfDay();
```

## Notes

*   **Kind Preservation**: These methods generally preserve the `Kind` property (UTC, Local, or Unspecified) of the input `DateTime`. When converting from Unix timestamps via `FromUnixTimestamp`, the resulting `DateTime` is typically returned as UTC.
*   **Thread Safety**: As this class consists entirely of static methods that operate on immutable `DateTime` structs and passed-in parameters without maintaining internal state, it is fully thread-safe.
*   **Edge Cases**:
    *   `RoundTo` will throw an exception if the interval is non-positive.
    *   `EndOfDay` and `EndOfMonth` set the time to the maximum millisecond precision (`.999`) rather than the theoretical tick limit, ensuring compatibility with most database systems and serialization formats that expect millisecond precision.
    *   `StartOfWeek` behavior regarding the first day of the week (Monday vs. Sunday) depends on the current thread's culture settings unless the implementation explicitly forces a specific calendar week rule.

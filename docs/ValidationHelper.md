# ValidationHelper

`ValidationHelper` is a static utility class that provides a centralized set of validation methods for the binance-p2p-monitor application. It ensures that user inputs, configuration values, and runtime parameters conform to the expected formats and constraints before they are processed by the core monitoring logic. All methods are deterministic and stateless.

## API

### `IsValidEmail`

```csharp
public static bool IsValidEmail(string email)
```

Validates that a string represents a properly formatted email address.

- **Parameters:** `email` — the candidate string to evaluate.
- **Return Value:** `true` if the string matches a standard email pattern (local-part@domain.tld); otherwise `false`.
- **Exceptions:** Does not throw. A `null` argument returns `false`.

### `IsValidTicker`

```csharp
public static bool IsValidTicker(string ticker)
```

Determines whether a string qualifies as a valid cryptocurrency trading ticker symbol (e.g., `BTC`, `ETH`).

- **Parameters:** `ticker` — the candidate ticker string.
- **Return Value:** `true` if the string consists of an accepted length of uppercase alphanumeric characters; otherwise `false`.
- **Exceptions:** Does not throw. A `null` or empty argument returns `false`.

### `IsValidFiatCode`

```csharp
public static bool IsValidFiatCode(string fiatCode)
```

Checks whether a string is a recognized ISO 4217 fiat currency code (e.g., `USD`, `EUR`, `TRY`).

- **Parameters:** `fiatCode` — the candidate currency code.
- **Return Value:** `true` if the code is present in the internal allow-list of supported fiat currencies; otherwise `false`.
- **Exceptions:** Does not throw. A `null` argument returns `false`.

### `IsValidPrice`

```csharp
public static bool IsValidPrice(string price)
```

Validates that a string can be interpreted as a positive decimal price value.

- **Parameters:** `price` — the string representation of a price.
- **Return Value:** `true` if the string parses to a decimal greater than zero; otherwise `false`.
- **Exceptions:** Does not throw. A `null`, empty, or non-numeric argument returns `false`.

### `IsValidThreshold`

```csharp
public static bool IsValidThreshold(string threshold)
```

Validates that a string represents a valid percentage threshold for alerting purposes.

- **Parameters:** `threshold` — the string representation of a threshold value.
- **Return Value:** `true` if the string parses to a decimal within the configured allowable range (typically 0 to 100); otherwise `false`.
- **Exceptions:** Does not throw. A `null` or out-of-range argument returns `false`.

### `IsValidTelegramChatId`

```csharp
public static bool IsValidTelegramChatId(string chatId)
```

Checks whether a string is a valid Telegram chat identifier.

- **Parameters:** `chatId` — the candidate chat ID string.
- **Return Value:** `true` if the string matches the expected format for Telegram chat IDs (numeric or negative numeric string); otherwise `false`.
- **Exceptions:** Does not throw. A `null` or empty argument returns `false`.

### `IsValidDateRange`

```csharp
public static bool IsValidDateRange(DateTime start, DateTime end)
```

Validates that two `DateTime` values form a logical date range.

- **Parameters:**
  - `start` — the beginning of the range.
  - `end` — the end of the range.
- **Return Value:** `true` if `start` is strictly earlier than `end`; otherwise `false`.
- **Exceptions:** Does not throw. Equal timestamps return `false`.

### `IsValidCollection<T>`

```csharp
public static bool IsValidCollection<T>(IEnumerable<T> collection)
```

Verifies that a collection is not null and contains at least one element.

- **Type Parameters:** `T` — the element type of the collection.
- **Parameters:** `collection` — the enumerable to inspect.
- **Return Value:** `true` if the collection is non-null and has a count greater than zero; otherwise `false`.
- **Exceptions:** Does not throw.

### `IsValidPrecision`

```csharp
public static bool IsValidPrecision(decimal value, int decimalPlaces)
```

Checks whether a decimal value conforms to a specified maximum number of decimal places.

- **Parameters:**
  - `value` — the decimal number to check.
  - `decimalPlaces` — the maximum allowed number of digits after the decimal point.
- **Return Value:** `true` if the value’s fractional digit count does not exceed `decimalPlaces`; otherwise `false`.
- **Exceptions:** Does not throw. A negative `decimalPlaces` argument is treated as zero.

### `MatchesPattern`

```csharp
public static bool MatchesPattern(string input, string regexPattern)
```

Tests whether a string fully matches a given regular expression pattern.

- **Parameters:**
  - `input` — the string to evaluate.
  - `regexPattern` — the regular expression pattern to apply.
- **Return Value:** `true` if the entire input string matches the pattern; otherwise `false`.
- **Exceptions:** Does not throw. A `null` input or `null` pattern returns `false`. An invalid pattern may propagate a `RegexParseException` from the underlying regex engine.

## Usage

### Example 1: Validating User Configuration Before Starting the Monitor

```csharp
public bool TryStartMonitor(string email, string ticker, string fiat, string thresholdStr)
{
    if (!ValidationHelper.IsValidEmail(email))
    {
        Console.WriteLine("Invalid email address provided.");
        return false;
    }

    if (!ValidationHelper.IsValidTicker(ticker))
    {
        Console.WriteLine($"Ticker '{ticker}' is not recognized.");
        return false;
    }

    if (!ValidationHelper.IsValidFiatCode(fiat))
    {
        Console.WriteLine($"Fiat code '{fiat}' is not supported.");
        return false;
    }

    if (!ValidationHelper.IsValidThreshold(thresholdStr))
    {
        Console.WriteLine("Threshold must be a number between 0 and 100.");
        return false;
    }

    // Configuration is valid; proceed with starting the monitor.
    StartMonitoring(email, ticker, fiat, decimal.Parse(thresholdStr));
    return true;
}
```

### Example 2: Filtering a Batch of P2P Advertisements

```csharp
public List<Advertisement> FilterValidAds(IEnumerable<Advertisement> ads)
{
    if (!ValidationHelper.IsValidCollection(ads))
    {
        return new List<Advertisement>();
    }

    return ads
        .Where(ad =>
            ValidationHelper.IsValidPrice(ad.Price) &&
            ValidationHelper.IsValidFiatCode(ad.FiatCurrency) &&
            ValidationHelper.IsValidTicker(ad.CryptoAsset) &&
            ValidationHelper.IsValidPrecision(ad.Quantity, 8))
        .ToList();
}
```

## Notes

- **Null Handling:** All string-accepting methods treat `null` as invalid and return `false` without throwing. `IsValidCollection<T>` similarly handles `null` collections gracefully.
- **Thread Safety:** Every method is static and operates solely on its arguments without accessing shared mutable state. The class is safe to call concurrently from multiple threads without synchronization.
- **Edge Cases:**
  - `IsValidDateRange` requires a strictly chronological order; equal `DateTime` values are considered invalid to prevent zero-length intervals.
  - `IsValidPrecision` treats a negative `decimalPlaces` as zero, meaning only integer values will pass validation.
  - `MatchesPattern` performs a full-string match (anchored); partial matches return `false`. An invalid regex pattern string will result in an exception from the regex engine, which is the only scenario where this class may throw.
  - `IsValidPrice` and `IsValidThreshold` are culture-sensitive if the parsing logic relies on the current culture’s decimal separator. Callers should ensure consistent formatting or invariant culture usage upstream.

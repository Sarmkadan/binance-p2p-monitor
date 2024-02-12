# CurrencyExtensions

Utility class providing formatting and comparison helpers for currency representations in the Binance P2P monitor context.

## API

### `public static string FormatCurrencyValue(decimal value, string currencyCode)`

Formats a numeric currency value according to the specified currency's display conventions.

- **Parameters**
  - `value`: The numeric value to format.
  - `currencyCode`: The ISO currency code (e.g., "USDT", "BUSD").
- **Return value**: A string representation of the formatted value.
- **Exceptions**: Throws `ArgumentNullException` if `currencyCode` is null.

### `public static string GetCurrencyDisplay(string currencyCode)`

Returns a human-readable display name for the given currency code.

- **Parameters**
  - `currencyCode`: The ISO currency code.
- **Return value**: A localized or standardized display name (e.g., "Tether USD" for "USDT").
- **Exceptions**: Throws `ArgumentNullException` if `currencyCode` is null.

### `public static bool IsMorePopularThan(string currencyCode, string otherCurrencyCode)`

Determines whether one currency is considered more popular than another based on internal ranking criteria.

- **Parameters**
  - `currencyCode`: The primary currency code.
  - `otherCurrencyCode`: The secondary currency code for comparison.
- **Return value**: `true` if `currencyCode` is ranked higher in popularity; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if either parameter is null.

### `public static string GetPopularityCssClass(string currencyCode)`

Returns a CSS class name indicating the popularity level of a currency.

- **Parameters**
  - `currencyCode`: The currency code to evaluate.
- **Return value**: A string representing a CSS class (e.g., "popularity-high", "popularity-medium").
- **Exceptions**: Throws `ArgumentNullException` if `currencyCode` is null.

### `public static string GetShortDisplayName(string currencyCode)`

Returns a shortened display name for the given currency code.

- **Parameters**
  - `currencyCode`: The ISO currency code.
- **Return value**: A concise display name (e.g., "USDT" for "Tether USD").
- **Exceptions**: Throws `ArgumentNullException` if `currencyCode` is null.

### `public static bool ShouldHighlight(string currencyCode)`

Determines whether a currency should be visually highlighted in the UI based on specific criteria.

- **Parameters**
  - `currencyCode`: The currency code to check.
- **Return value**: `true` if the currency meets highlight criteria; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `currencyCode` is null.

## Usage

# ValidationResult
The `ValidationResult` type is a record used to validate various data types and formats, providing a set of static methods to check the validity of different inputs, such as email addresses, ticker symbols, fiat currency codes, prices, thresholds, Telegram chat IDs, date ranges, collections, and precision values, as well as matching patterns.

## API
The `ValidationResult` type provides the following static methods:
* `IsValidEmail`: Checks if a given string is a valid email address. Returns `true` if the email is valid, `false` otherwise.
* `IsValidTicker`: Verifies if a given string is a valid ticker symbol. Returns `true` if the ticker is valid, `false` otherwise.
* `IsValidFiatCode`: Checks if a given string is a valid fiat currency code. Returns `true` if the code is valid, `false` otherwise.
* `IsValidPrice`: Validates if a given value is a valid price. Returns `true` if the price is valid, `false` otherwise.
* `IsValidThreshold`: Checks if a given value is a valid threshold. Returns `true` if the threshold is valid, `false` otherwise.
* `IsValidTelegramChatId`: Verifies if a given value is a valid Telegram chat ID. Returns `true` if the ID is valid, `false` otherwise.
* `IsValidDateRange`: Checks if a given date range is valid. Returns `true` if the range is valid, `false` otherwise.
* `IsValidCollection<T>`: Validates if a given collection of type `T` is valid. Returns `true` if the collection is valid, `false` otherwise.
* `IsValidPrecision`: Checks if a given precision value is valid. Returns `true` if the precision is valid, `false` otherwise.
* `MatchesPattern`: Verifies if a given string matches a specified pattern. Returns `true` if the string matches the pattern, `false` otherwise.

## Usage
The following examples demonstrate how to use the `ValidationResult` type:
```csharp
// Example 1: Validating an email address
string email = "example@example.com";
if (ValidationResult.IsValidEmail(email))
{
    Console.WriteLine("Email is valid");
}
else
{
    Console.WriteLine("Email is invalid");
}

// Example 2: Validating a collection of prices
List<decimal> prices = new List<decimal> { 10.99m, 5.49m, 7.99m };
if (ValidationResult.IsValidCollection(prices))
{
    Console.WriteLine("Prices are valid");
}
else
{
    Console.WriteLine("Prices are invalid");
}
```

## Notes
When using the `ValidationResult` type, consider the following edge cases:
* Null or empty inputs may return `false` for most validation methods.
* The `IsValidCollection<T>` method may throw an exception if the collection is null.
* The `MatchesPattern` method may throw an exception if the pattern is invalid.
* The `ValidationResult` type is designed to be thread-safe, as all methods are static and do not rely on instance state. However, the validity of the input data is not guaranteed to be consistent across threads.

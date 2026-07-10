# Currency
The `Currency` type represents a monetary unit in the binance-p2p-monitor project, encapsulating its properties and behaviors. It provides a structured way to work with currencies, including their identification, formatting, and comparison.

## API
The `Currency` type exposes the following public members:
* `Id`: A unique identifier for the currency, represented as an integer.
* `Code`: A string representing the currency code.
* `Name`: A string representing the currency name.
* `Symbol`: A nullable string representing the currency symbol.
* `IsActive`: A boolean indicating whether the currency is active.
* `DecimalPlaces`: An integer representing the number of decimal places for the currency.
* `CreatedAt` and `UpdatedAt`: DateTime values representing the creation and last update timestamps.
* `PopularityScore`: A decimal value representing the currency's popularity score.
* `DisplayOrder`: An integer representing the display order of the currency.
* `Notes`: A nullable string for additional notes about the currency.
* `FormatValue`: A string representing the formatted value of the currency.
* `GetDisplayFormat`: A string representing the display format of the currency.
* `IsValid`: A boolean indicating whether the currency is valid.
* `IsPopular`: A boolean indicating whether the currency is popular.
* `GetPopularityTier`: A string representing the popularity tier of the currency.
* `RoundValue`: A decimal value representing the rounded value of the currency.
* `GetFullName`: A string representing the full name of the currency.
* `ComparePopularity`: An integer representing the comparison of the currency's popularity.

## Usage
Here are two examples of using the `Currency` type in C#:
```csharp
// Example 1: Creating a new Currency instance
var currency = new Currency
{
    Id = 1,
    Code = "USD",
    Name = "United States Dollar",
    Symbol = "$",
    IsActive = true,
    DecimalPlaces = 2,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    PopularityScore = 0.8m,
    DisplayOrder = 1,
    Notes = "The US dollar is the official currency of the United States."
};

Console.WriteLine(currency.GetFullName); // Output: United States Dollar

// Example 2: Comparing the popularity of two currencies
var currency1 = new Currency { PopularityScore = 0.8m };
var currency2 = new Currency { PopularityScore = 0.9m };

var comparison = currency1.ComparePopularity(currency2);
if (comparison < 0)
{
    Console.WriteLine("Currency 1 is less popular than Currency 2");
}
else if (comparison > 0)
{
    Console.WriteLine("Currency 1 is more popular than Currency 2");
}
else
{
    Console.WriteLine("Currency 1 and Currency 2 have the same popularity");
}
```

## Notes
When working with the `Currency` type, consider the following edge cases and thread-safety remarks:
* The `IsValid` property may return false if the currency's properties are not properly set.
* The `IsPopular` property may return false if the popularity score is below a certain threshold.
* The `GetPopularityTier` method may return a string indicating the popularity tier, which can be used for sorting or filtering purposes.
* The `RoundValue` property may return a decimal value rounded to the nearest decimal place, which can be used for display purposes.
* The `ComparePopularity` method may return an integer value indicating the comparison of the popularity scores, which can be used for sorting or filtering purposes.
* The `Currency` type is not thread-safe, and its properties and methods should not be accessed concurrently from multiple threads. If concurrent access is necessary, consider using synchronization mechanisms such as locks or concurrent collections.

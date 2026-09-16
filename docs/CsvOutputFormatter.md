# CsvOutputFormatter

`CsvOutputFormatter` implements `IOutputFormatter` and converts objects into comma-separated text. It is declared in the `BinanceP2pMonitor.Formatters` namespace.

## Public API

### `FormatType`

```csharp
public string FormatType { get; }
```

Returns `"csv"`.

### `Format(object? data)`

```csharp
public string Format(object? data)
```

Formats one object as a header row followed by one data row. The header names are the names of the object's public properties.

If `data` is `null`, the method returns an empty string.

### `Format(IEnumerable<object> data)`

```csharp
public string Format(IEnumerable<object> data)
```

Materializes the sequence and formats each object as one row. The public property names of the first object become the headers and determine the columns for every row. Each row is inspected using its own runtime type, so a property absent from a later object produces an empty field.

If the sequence is empty, the method returns an empty string.

### `Format(IEnumerable<object> data, IEnumerable<string> headers)`

```csharp
public string Format(IEnumerable<object> data, IEnumerable<string> headers)
```

Materializes both sequences and emits the supplied headers in their given order. For each object, a header is matched to a public property by an exact, case-sensitive name comparison. A header with no matching property produces an empty field. Duplicate headers produce duplicate columns.

Unlike the overload that infers headers, this overload emits a header row even when `data` is empty. An empty `headers` sequence produces an empty header line and, when data is present, one empty line per object.

## Output format

- Fields are separated with commas; no spaces are added around delimiters.
- The first row contains the inferred or supplied headers.
- Each object produces one data row after the header.
- Every header and data row ends with `Environment.NewLine`, including the final row.
- Property values that implement `IFormattable` are converted with `CultureInfo.InvariantCulture` and no explicit format string. Other non-null values use `ToString()`.
- A null property value, a missing property, an empty string, or a `ToString()` result of `null` is written as `""`.
- A field containing a comma, double quote, or line-feed character (`\n`) is enclosed in double quotes. Double quotes inside the field are doubled.
- Other non-empty fields are written without quotes.

For example:

```csharp
var formatter = new CsvOutputFormatter();
var csv = formatter.Format(new object[]
{
    new { Asset = "USDT", Merchant = "ACME, Inc.", Price = 42.50m },
    new { Asset = "BTC", Merchant = "Alice \"Fast Pay\"", Price = 1234.75m }
});
```

The value of `csv` is:

```csv
Asset,Merchant,Price
USDT,"ACME, Inc.",42.50
BTC,"Alice ""Fast Pay""",1234.75
```

The displayed block uses line feeds for readability; the actual row terminator is the current platform's `Environment.NewLine`.

Custom headers can select and reorder columns:

```csharp
var csv = formatter.Format(
    new object[] { new { Asset = "USDT", Price = 42.50m } },
    new[] { "Price", "Unknown", "Asset" });
```

This produces:

```csv
Price,Unknown,Asset
42.50,"",USDT
```

## Operational notes

- Both collection overloads enumerate their inputs immediately by calling `ToList()`; they do not stream rows.
- Passing `null` for either enumerable is not specially handled and results in an exception while the input is materialized.
- Property getters are evaluated during formatting. Exceptions thrown by a getter propagate to the caller.
- The formatter performs no validation that all objects share the same type or property set.
- Reflection determines the inferred property order. Supply explicit headers when a specific column order is required.

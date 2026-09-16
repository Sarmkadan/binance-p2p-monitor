# TableOutputFormatter

`TableOutputFormatter` implements `IOutputFormatter` and renders objects as a bordered ASCII table. It is declared in the `BinanceP2pMonitor.Formatters` namespace.

## Public API

### `FormatType`

```csharp
public string FormatType { get; }
```

Returns `"table"`.

### `Format(object? data)`

```csharp
public string Format(object? data)
```

Formats one object as a table with a header row and one data row. The object's public property names are used as the headers.

If `data` is `null`, the method returns `"(empty)"`.

### `Format(IEnumerable<object> data)`

```csharp
public string Format(IEnumerable<object> data)
```

Materializes the sequence and uses the public property names of the first object as the headers. Each object produces one data row. Objects are inspected using their own runtime types, so a property that is absent from a later object is rendered as `"(null)"`.

If the sequence is empty, the method returns `"(no data)"`.

### `Format(IEnumerable<object> data, IEnumerable<string> headers)`

```csharp
public string Format(IEnumerable<object> data, IEnumerable<string> headers)
```

Materializes both sequences and emits the supplied headers in their given order. For each object, a header is matched to a public property by an exact, case-sensitive name comparison. A missing property or null property value is rendered as `"(null)"`. Duplicate headers produce duplicate columns.

If `data` is empty, the method returns `"(no data)"` without emitting the supplied headers. If `headers` is empty and data is present, the result is a zero-column table: the border lines are `++`, while the header and each data row are `|`.

## Output format

- A border line appears before the header, after the header, and after all data rows.
- Each column is surrounded by `|` characters and one space of padding on each side.
- Border intersections use `+`, and each horizontal segment contains the column width plus two hyphens for the cell padding.
- Column widths are the length of the widest header or rendered value in that column. Cells are padded on the right to that width.
- Every generated line ends with `Environment.NewLine`, including the final border.
- Values are converted by calling `ToString()` without an explicit format provider.
- Data values longer than 50 characters are shortened to 50 characters by replacing the final three characters with `...`. Headers are not truncated.
- Null values, properties not found on an object's runtime type, and `ToString()` results of `null` appear as `(null)`.
- Cell contents are inserted verbatim. Pipes, line breaks, and other characters with structural significance are not escaped.

For example:

```csharp
var formatter = new TableOutputFormatter();
var table = formatter.Format(new object[]
{
    new { Asset = "USDT", Price = 42.50m },
    new { Asset = "BTC", Price = 1234.75m }
});
```

The value of `table` is:

```text
+-------+---------+
| Asset | Price   |
+-------+---------+
| USDT  | 42.50   |
| BTC   | 1234.75 |
+-------+---------+
```

The displayed block omits the final line terminator for readability. The actual string ends with `Environment.NewLine`.

Custom headers can select and reorder columns:

```csharp
var table = formatter.Format(
    new object[] { new { Asset = "USDT", Price = 42.50m } },
    new[] { "Price", "Unknown", "Asset" });
```

This produces:

```text
+-------+---------+-------+
| Price | Unknown | Asset |
+-------+---------+-------+
| 42.50 | (null)  | USDT  |
+-------+---------+-------+
```

## Operational notes

- Both collection overloads enumerate their inputs immediately by calling `ToList()`; they do not stream rows.
- Passing `null` for either enumerable is not specially handled and results in an exception while the input is materialized.
- A null element in `data` is not specially handled and results in an exception when its runtime type is inspected.
- Property getters are evaluated during formatting. Exceptions thrown by a getter propagate to the caller.
- The formatter performs no validation that all objects share the same type or property set.
- Reflection determines the inferred property order. Supply explicit headers when a specific column order is required.

# MarkdownOutputFormatter

`MarkdownOutputFormatter` implements `IOutputFormatter` and renders objects as a Markdown table. It is declared in the `BinanceP2pMonitor.Formatters` namespace.

## Public API

### `FormatType`

```csharp
public string FormatType { get; }
```

Returns `"markdown"`.

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

If `data` is empty, the method returns `"(no data)"` without emitting the supplied headers. If `headers` is empty and data is present, the result consists of a `|` header line, a `|-` separator line, and a `|` line for each object.

## Output format

- The first line is the header row, the second is the Markdown separator row, and each object adds one data row.
- Every generated line ends with `Environment.NewLine`, including the final data row.
- Cells are padded on the right to the widest header or value in their column.
- Values are converted by calling `ToString()` without an explicit format provider.
- Data values longer than 50 characters are shortened to 50 characters by replacing the final three characters with `...`. Headers are not truncated.
- Null values and properties not found on an object's runtime type appear as `(null)`.
- Cell contents are inserted verbatim. Pipes, line breaks, and other Markdown-sensitive characters are not escaped.

For example:

```csharp
var formatter = new MarkdownOutputFormatter();
var markdown = formatter.Format(new object[]
{
    new { Asset = "USDT", Price = 42.50m },
    new { Asset = "BTC", Price = 1234.75m }
});
```

The value of `markdown` is:

```text
| Asset | Price   |
|-------|--------|
| USDT  | 42.50   |
| BTC   | 1234.75 |
```

The displayed block omits the final line terminator for readability. The actual string ends with `Environment.NewLine`.

Custom headers can select and reorder columns:

```csharp
var markdown = formatter.Format(
    new object[] { new { Asset = "USDT", Price = 42.50m } },
    new[] { "Price", "Unknown", "Asset" });
```

This produces:

```text
| Price | Unknown | Asset |
|-------|--------|------|
| 42.50 | (null)  | USDT  |
```

## Operational notes

- Both collection overloads enumerate their inputs immediately by calling `ToList()`; they do not stream rows.
- Passing `null` for either enumerable is not specially handled and results in an exception while the input is materialized.
- Property getters are evaluated during formatting. Exceptions thrown by a getter propagate to the caller.
- The formatter performs no validation that all objects share the same type or property set.
- Reflection determines the inferred property order. Supply explicit headers when a specific column order is required.

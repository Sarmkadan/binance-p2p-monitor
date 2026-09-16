# JsonOutputFormatter

`JsonOutputFormatter` implements `IOutputFormatter` and serializes values with `System.Text.Json`. It is declared in the `BinanceP2pMonitor.Formatters` namespace.

## Public API

### `FormatType`

```csharp
public string FormatType { get; }
```

Returns `"json"`.

### `Format(object? data)`

```csharp
public string Format(object? data)
```

Serializes one value as indented JSON. Passing `null` returns the JSON literal `null` directly.

The value can itself be a collection; in that case, `System.Text.Json` serializes it according to its runtime type.

### `Format(IEnumerable<object> data)`

```csharp
public string Format(IEnumerable<object> data)
```

Materializes `data` with `ToList()` and serializes the resulting list as an indented JSON array. An empty sequence produces `[]`.

### `Format(IEnumerable<object> data, IEnumerable<string> headers)`

```csharp
public string Format(IEnumerable<object> data, IEnumerable<string> headers)
```

Materializes `data` with `ToList()` and serializes an object with two properties:

- `headers` contains the supplied header sequence.
- `data` contains the materialized data list.

Headers are metadata only. They do not select, rename, filter, or reorder properties within the objects in `data`. An empty header sequence is serialized as an empty array, while a null `headers` value is serialized as `null`.

## Output format

Successful output uses the default `JsonSerializerOptions` behavior with `WriteIndented` enabled. Consequently:

- CLR property names retain their declared casing.
- Public properties are serialized; fields are not included by default.
- Null properties are included.
- Nested objects and collections are serialized recursively.
- Non-empty objects and arrays use indented, multi-line JSON. Empty objects and arrays remain `{}` and `[]`.

For example:

```csharp
var formatter = new JsonOutputFormatter();
var json = formatter.Format(new object[]
{
    new { Asset = "USDT", Price = 42.50m },
    new { Asset = "BTC", Price = 1234.75m }
});
```

The value of `json` is:

```json
[
  {
    "Asset": "USDT",
    "Price": 42.50
  },
  {
    "Asset": "BTC",
    "Price": 1234.75
  }
]
```

Using the header overload:

```csharp
var json = formatter.Format(
    new object[] { new { Asset = "USDT", Price = 42.50m } },
    new[] { "Price", "Asset" });
```

produces:

```json
{
  "headers": [
    "Price",
    "Asset"
  ],
  "data": [
    {
      "Asset": "USDT",
      "Price": 42.50
    }
  ]
}
```

The lowercase `headers` and `data` names come from the anonymous wrapper object's property names, not from a global camel-case naming policy.

## Error handling and operational notes

- Each overload catches exceptions raised inside its serialization path and returns a compact JSON object of the form `{"error":"message"}`. The message is the caught exception's `Message` value.
- Error output does not use the indented serializer options used for successful output.
- The collection overloads materialize `data` before serialization, so they enumerate it immediately and do not stream the resulting JSON.
- A null `data` enumerable causes `ToList()` to throw; that exception is caught and returned as an error object.
- The header overload leaves `headers` as an enumerable. `System.Text.Json` enumerates it while serializing the wrapper object.
- Serialization uses the standard default handling for unsupported values and object cycles. Any resulting exception is converted to the error object described above.

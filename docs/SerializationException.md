# SerializationException

Represents an error that occurs during JSON serialization or deserialization operations within the binance-p2p-monitor application. This exception type captures contextual information about the data type being processed and the raw JSON content involved, aiding in debugging malformed or unexpected API responses.

## API

### Constructors

#### `SerializationException()`
Initializes a new instance of the `SerializationException` class with default error information.

- **Parameters:** None.
- **Remarks:** Use this constructor when the specific data type and JSON content are not available or not relevant to the failure.

#### `SerializationException(string? dataType, string? jsonContent, string message, Exception? innerException)`
Initializes a new instance of the `SerializationException` class with detailed contextual information.

- **Parameters:**
  - `dataType` (`string?`): The name of the target type that was being deserialized or serialized. May be `null` if the type is unknown.
  - `jsonContent` (`string?`): The raw JSON string that caused the failure. May be `null` if the content is unavailable.
  - `message` (`string`): The error message that describes the exception.
  - `innerException` (`Exception?`): The exception that is the cause of the current exception, or `null` if no inner exception is specified.
- **Throws:** Nothing directly (constructor).

### Properties

#### `DataType`
Gets the name of the data type involved in the serialization operation.

- **Type:** `string?`
- **Value:** The type name as a string, or `null` if not specified during construction.
- **Remarks:** Useful for identifying which model or DTO failed to process.

#### `JsonContent`
Gets the raw JSON content that triggered the exception.

- **Type:** `string?`
- **Value:** The JSON string, or `null` if not provided during construction.
- **Remarks:** May contain truncated or partial content depending on where the failure occurred in the processing pipeline.

### Methods

#### `override string ToString()`
Returns a string representation of the current exception, including the message, data type, JSON content, and inner exception details.

- **Return Value:** `string` — A formatted string containing all available diagnostic information.
- **Throws:** Nothing.

### Derived Type: `JsonSerializationException`

#### `JsonSerializationException()`
Initializes a new instance of the `JsonSerializationException` class with default error information.

- **Parameters:** None.

#### `JsonSerializationException(string? dataType, string? jsonContent, string message, Exception? innerException)`
Initializes a new instance of the `JsonSerializationException` class with detailed contextual information.

- **Parameters:**
  - `dataType` (`string?`): The name of the target type.
  - `jsonContent` (`string?`): The raw JSON string.
  - `message` (`string`): The error message.
  - `innerException` (`Exception?`): The causing exception, if any.
- **Throws:** Nothing directly (constructor).

#### `override string ToString()`
Returns a string representation of the current `JsonSerializationException`, including all contextual fields.

- **Return Value:** `string` — A formatted diagnostic string.
- **Throws:** Nothing.

## Usage

### Example 1: Catching and Logging a Deserialization Failure

```csharp
try
{
    var jsonResponse = await httpClient.GetStringAsync("/api/v3/ticker/price");
    var ticker = JsonConvert.DeserializeObject<TickerPrice>(jsonResponse);
}
catch (JsonException ex)
{
    var serializationEx = new SerializationException(
        dataType: typeof(TickerPrice).FullName,
        jsonContent: jsonResponse,
        message: "Failed to deserialize ticker price response.",
        innerException: ex);

    logger.LogError(serializationEx.ToString());
    throw serializationEx;
}
```

### Example 2: Throwing a Specific `JsonSerializationException` for Malformed Data

```csharp
public OrderBookEntry ParseOrderBookEntry(string rawJson)
{
    if (string.IsNullOrWhiteSpace(rawJson))
    {
        throw new JsonSerializationException(
            dataType: nameof(OrderBookEntry),
            jsonContent: rawJson,
            message: "Received empty or null JSON for order book entry.",
            innerException: null);
    }

    try
    {
        return JsonConvert.DeserializeObject<OrderBookEntry>(rawJson);
    }
    catch (JsonException ex)
    {
        throw new JsonSerializationException(
            dataType: nameof(OrderBookEntry),
            jsonContent: rawJson,
            message: "Order book entry JSON is malformed.",
            innerException: ex);
    }
}
```

## Notes

- **Null Handling:** Both `DataType` and `JsonContent` are nullable strings. Always check for `null` before using them in string formatting or comparisons to avoid unintended `null` coalescing behavior.
- **`ToString()` Output:** The `ToString()` override includes all available fields. If `JsonContent` contains large payloads, the resulting string may be extremely long. Consider truncating or redacting sensitive data before logging in production environments.
- **Exception Chaining:** The `innerException` parameter preserves the original stack trace and root cause. When re-throwing, pass the caught exception as the inner exception rather than discarding it.
- **Thread Safety:** Instances of `SerializationException` and `JsonSerializationException` are immutable after construction. They are safe to read from multiple threads concurrently without synchronization.
- **Inheritance:** `JsonSerializationException` is a more specific subtype of `SerializationException`. Catch `JsonSerializationException` first when you need to handle JSON-specific errors distinctly from other serialization failures.
- **Serialization:** These exception types are designed to be serializable for cross-domain propagation and logging persistence. Ensure that `DataType` and `JsonContent` do not contain sensitive information if exceptions are serialized to external systems.

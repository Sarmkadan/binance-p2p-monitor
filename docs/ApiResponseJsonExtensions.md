# ApiResponseJsonExtensions

Provides JSON serialization and deserialization functionality for `ApiResponse` and `ApiResponse<T>` types, enabling conversion between these objects and their JSON string representations. This static class facilitates structured data exchange in scenarios where API responses need to be logged, transmitted, or reconstructed from external sources.

## API

### `ToJson`
Serializes an `ApiResponse` instance to its JSON string representation.  
**Parameters:**  
- `response` (`ApiResponse`): The object to serialize.  
**Returns:**  
- `string`: The JSON representation of the input object.  
**Exceptions:**  
- `JsonException`: Thrown if the object cannot be serialized due to invalid structure or unsupported types.

### `FromJson`
Deserializes a JSON string into an `ApiResponse` instance.  
**Parameters:**  
- `json` (`string`): The JSON string to deserialize.  
**Returns:**  
- `ApiResponse?`: The deserialized object, or `null` if deserialization fails.  
**Exceptions:**  
- `JsonException`: Thrown if the JSON is malformed or does not conform to the expected structure.

### `TryFromJson`
Attempts to deserialize a JSON string into an `ApiResponse` instance without throwing exceptions.  
**Parameters:**  
- `json` (`string`): The JSON string to deserialize.  
- `result` (`out ApiResponse`): The deserialized object if successful.  
**Returns:**  
- `bool`: `true` if deserialization succeeded; `false` otherwise.  

### `ToJson<T>`
Serializes an `ApiResponse<T>` instance to its JSON string representation.  
**Parameters:**  
- `response` (`ApiResponse<T>`): The object to serialize.  
**Returns:**  
- `string`: The JSON representation of the input object.  
**Exceptions:**  
- `JsonException`: Thrown if the object cannot be serialized.

### `FromJson<T>`
Deserializes a JSON string into an `ApiResponse<T>` instance.  
**Parameters:**  
- `json` (`string`): The JSON string to deserialize.  
**Returns:**  
- `ApiResponse<T>?`: The deserialized object, or `null` if deserialization fails.  
**Exceptions:**  
- `JsonException`: Thrown if the JSON is malformed or incompatible with the target type.

### `TryFromJson<T>`
Attempts to deserialize a JSON string into an `ApiResponse<T>` instance without throwing exceptions.  
**Parameters:**  
- `json` (`string`): The JSON string to deserialize.  
- `result` (`out ApiResponse<T>`): The deserialized object if successful.  
**Returns:**  
- `bool`: `true` if deserialization succeeded; `false` otherwise.  

## Usage

```csharp
// Serialize an ApiResponse to JSON
ApiResponse response = new ApiResponse { Success = true, Data = "example" };
string json = response.ToJson();
Console.WriteLine(json); // {"Success":true,"Data":"example"}

// Deserialize JSON into ApiResponse<int>
string inputJson = "{\"Success\":true,\"Data\":42}";
ApiResponse<int>? parsed = ApiResponseJsonExtensions.FromJson<int>(inputJson);
if (parsed?.Success == true)
{
    Console.WriteLine($"Value: {parsed.Data}"); // Value: 42
}
```

```csharp
// Safely deserialize JSON with TryFromJson
string invalidJson = "not valid json";
if (ApiResponseJsonExtensions.TryFromJson(invalidJson, out ApiResponse? result))
{
    Console.WriteLine($"Parsed: {result?.Success}");
}
else
{
    Console.WriteLine("Deserialization failed."); // Deserialization failed.
}
```

## Notes

- All methods are thread-safe if the underlying JSON serializer (e.g., `System.Text.Json`) is configured with immutable options. Shared mutable state in serialization settings may introduce race conditions.
- `FromJson` and `FromJson<T>` return `null` for invalid input rather than throwing, but malformed JSON will still trigger exceptions. Use `TryFromJson` variants for graceful error handling.
- Generic type parameter `T` must be compatible with the JSON structure; mismatched types will result in `null` returns or exceptions depending on the method used.
- Empty or whitespace-only JSON strings are treated as invalid input and will cause `FromJson` to return `null` or throw.

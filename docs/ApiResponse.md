# ApiResponse

The `ApiResponse<T>` class (and its non-generic base `ApiResponse`) is the standard response envelope used throughout the `binance-p2p-monitor` project. It encapsulates the outcome of any operation, providing a consistent structure for success/failure status, returned data, human-readable messages, detailed error lists, a timestamp, and a request identifier. The generic variant carries the actual payload in `Data`, while the non-generic base is used for operations that do not return a value. Static factory methods on both types simplify the creation of common response patterns.

## API

### Instance Members

#### `public bool Success`

Indicates whether the operation completed successfully. When `true`, `Data` typically contains the expected result and `Errors` is empty. When `false`, `Message` and `Errors` provide details about the failure.

#### `public T? Data`

The payload returned by the operation. For a successful response, this holds the result of type `T`. For a failed response, it is `null` (or `default`). The generic type parameter `T` is constrained only by the caller; no class constraint is applied.

#### `public string? Message`

A human-readable summary of the operation outcome. May be `null` if no message is provided. Typically set to a success description (e.g., "Operation completed.") or an error description.

#### `public List<string> Errors`

A list of detailed error messages. Empty for successful responses. Each string represents a distinct error, such as validation failures or exception messages. The list is never `null`; it is initialized to an empty list upon construction.

#### `public DateTime Timestamp`

The UTC timestamp when the response was created. Set automatically by the factory methods or constructor. Useful for logging and correlation.

#### `public string RequestId`

A unique identifier for the request that produced this response. Set by the caller or automatically generated. Helps trace operations across distributed components.

### Static Factory Members (on `ApiResponse<T>`)

#### `public static ApiResponse<T> SuccessResult`

Returns a new `ApiResponse<T>` instance with `Success = true`, `Data` set to `default(T?)`, `Message` set to a default success message, and `Errors` empty. The `Timestamp` is set to the current UTC time, and `RequestId` is generated automatically.  
*Parameters:* None.  
*Returns:* A pre-configured success response.  
*Throws:* Nothing.

#### `public static ApiResponse<T> ErrorResult`

Returns a new `ApiResponse<T>` instance with `Success = false`, `Data = default`, `Message` set to a default error message, and `Errors` containing a single generic error string. The `Timestamp` and `RequestId` are set as in `SuccessResult`.  
*Parameters:* None.  
*Returns:* A pre-configured error response.  
*Throws:* Nothing.

*Note:* The list shows two identical signatures for `ErrorResult`; in the actual implementation one overload may accept a custom message or error list. Only the parameterless version is documented here.

### Static Factory Members (on non-generic `ApiResponse`, hidden by `new`)

#### `public static new ApiResponse SuccessResult`

Hides the base class member. Returns a new non-generic `ApiResponse` with `Success = true`, `Message` set to a default success message, and `Errors` empty. `Timestamp` and `RequestId` are set automatically.  
*Parameters:* None.  
*Returns:* A pre-configured success response.  
*Throws:* Nothing.

#### `public static new ApiResponse ErrorResult`

Hides the base class member. Returns a new non-generic `ApiResponse` with `Success = false`, `Message` set to a default error message, and `Errors` containing a single generic error string. `Timestamp` and `RequestId` are set automatically.  
*Parameters:* None.  
*Returns:* A pre-configured error response.  
*Throws:* Nothing.

*Note:* Two identical signatures appear in the member list; only the parameterless version is documented. Overloads accepting custom messages may exist.

## Usage

### Example 1: Returning a successful response with data

```csharp
public ApiResponse<Order> GetOrder(long orderId)
{
    try
    {
        var order = _orderRepository.Find(orderId);
        if (order == null)
        {
            return ApiResponse<Order>.ErrorResult; // or a custom error
        }

        return new ApiResponse<Order>
        {
            Success = true,
            Data = order,
            Message = "Order retrieved successfully.",
            Errors = new List<string>(),
            Timestamp = DateTime.UtcNow,
            RequestId = Guid.NewGuid().ToString()
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<Order>
        {
            Success = false,
            Data = default,
            Message = "An unexpected error occurred.",
            Errors = new List<string> { ex.Message },
            Timestamp = DateTime.UtcNow,
            RequestId = Guid.NewGuid().ToString()
        };
    }
}
```

### Example 2: Using static factory methods for a void operation

```csharp
public ApiResponse CancelOrder(long orderId)
{
    if (!_orderRepository.Exists(orderId))
    {
        // Return a pre-built error response (non-generic)
        return ApiResponse.ErrorResult;
    }

    _orderRepository.Cancel(orderId);
    // Return a pre-built success response (non-generic)
    return ApiResponse.SuccessResult;
}
```

## Notes

- **Thread safety:** Instances of `ApiResponse<T>` and `ApiResponse` are immutable after construction (all properties are read/write but are typically set once). The static factory methods create new instances each time they are called and are safe to invoke concurrently. However, the `Errors` list is a mutable `List<string>`; if the same instance is shared across threads, modifications to the list are not synchronized. In practice, responses are created and consumed within a single logical operation, so this is rarely an issue.
- **Nullability:** `Data` is nullable (`T?`) to accommodate failure cases. `Message` is also nullable. `Errors` is never null; it is always an initialized list, even if empty.
- **Inheritance:** `ApiResponse<T>` inherits from the non-generic `ApiResponse` base class. The `new` keyword on the static factory members of `ApiResponse<T>` hides the base class members. When calling `ApiResponse<T>.SuccessResult`, the generic version is used; when calling `ApiResponse.SuccessResult`, the non-generic version is used. This design allows both typed and untyped responses to be created with the same naming convention.
- **Edge cases:** If `T` is a reference type, `Data` will be `null` on error. If `T` is a value type, `Data` will be `default(T)` (e.g., `0` for integers). Consumers should always check `Success` before accessing `Data`. The `Timestamp` is set to `DateTime.UtcNow` at the moment of creation; no time zone conversion is applied. The `RequestId` is generated using `Guid.NewGuid().ToString()` when not explicitly provided; it is not guaranteed to be globally unique but is sufficient for tracing within a single process lifetime.

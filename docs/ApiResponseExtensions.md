# ApiResponseExtensions

The `ApiResponseExtensions` class provides a set of static extension methods designed to simplify the creation, validation, and inspection of `ApiResponse` and `ApiResponse<T>` objects within the `binance-p2p-monitor` project. These utilities centralize logic for determining request success status, appending error details to existing responses, constructing new successful responses with payload data, and generating human-readable summaries of the response state, thereby reducing boilerplate code in API consumption layers.

## API

### `IsSuccessful`
Determines whether a given `ApiResponse` instance represents a successful operation.
*   **Parameters**: `this ApiResponse response` – The response instance to evaluate.
*   **Return Value**: `bool` – Returns `true` if the response indicates success; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `response` is `null`.

### `IsSuccessful<T>`
Determines whether a given `ApiResponse<T>` instance represents a successful operation.
*   **Parameters**: `this ApiResponse<T> response` – The generic response instance to evaluate.
*   **Return Value**: `bool` – Returns `true` if the response indicates success; otherwise, `false`.
*   **Throws**: `ArgumentNullException` if `response` is `null`.

### `AddError`
Appends an error message to an existing `ApiResponse` object, marking it as unsuccessful if not already flagged.
*   **Parameters**: 
    *   `this ApiResponse response` – The target response instance.
    *   `string message` – The error message to append.
*   **Return Value**: `ApiResponse` – The same instance passed in, modified to include the new error.
*   **Throws**: `ArgumentNullException` if `response` or `message` is `null`.

### `AddError<T>`
Appends an error message to an existing `ApiResponse<T>` object, marking it as unsuccessful.
*   **Parameters**: 
    *   `this ApiResponse<T> response` – The target generic response instance.
    *   `string message` – The error message to append.
*   **Return Value**: `ApiResponse<T>` – The same instance passed in, modified to include the new error.
*   **Throws**: `ArgumentNullException` if `response` or `message` is `null`.

### `WithData<T>`
Creates a new `ApiResponse<T>` instance populated with specific data, indicating a successful operation.
*   **Parameters**: 
    *   `this ApiResponse<T> response` – This parameter serves as a type anchor for the extension; the method typically returns a new instance based on the generic type `T`.
    *   `T data` – The payload data to embed in the new response.
*   **Return Value**: `ApiResponse<T>` – A new response instance containing the provided data and a success status.
*   **Throws**: `ArgumentNullException` if `response` is `null` (required for extension method resolution).

### `Summary`
Generates a concise string representation of the `ApiResponse` status and any associated errors.
*   **Parameters**: `this ApiResponse response` – The response instance to summarize.
*   **Return Value**: `string` – A formatted string detailing success/failure and error counts or messages.
*   **Throws**: `ArgumentNullException` if `response` is `null`.

### `Summary<T>`
Generates a concise string representation of the `ApiResponse<T>` status, including data presence and errors.
*   **Parameters**: `this ApiResponse<T> response` – The generic response instance to summarize.
*   **Return Value**: `string` – A formatted string detailing success/failure, data status, and error messages.
*   **Throws**: `ArgumentNullException` if `response` is `null`.

## Usage

The following examples demonstrate how to utilize these extensions when handling Binance P2P API results.

**Example 1: Validating and Summarizing a Response**
This example shows how to check the success status of a raw response and generate a log-friendly summary if errors occur.

```csharp
public async Task ProcessP2POrderAsync(ApiResponse response)
{
    if (!response.IsSuccessful())
    {
        // Append a contextual error before logging
        response.AddError("Failed to process order due to upstream validation.");
        
        // Generate a detailed summary for logging
        string logMessage = response.Summary();
        Console.WriteLine($"Order Processing Failed: {logMessage}");
        return;
    }

    Console.WriteLine("Order processed successfully.");
}
```

**Example 2: Constructing a Typed Response with Data**
This example illustrates creating a successful typed response containing P2P advertisement data, or adding errors to a failed attempt.

```csharp
public ApiResponse<List<P2PAdvertisement>> FetchAdvertisements()
{
    try
    {
        var ads = _service.GetActiveAds();
        
        // Use a dummy instance or default to anchor the extension if required by specific implementation,
        // or call directly if the extension creates a new instance internally based on type inference.
        // Assuming standard pattern where we might start with a base or use the extension to wrap data:
        var emptyResponse = new ApiResponse<List<P2PAdvertisement>>();
        return emptyResponse.WithData(ads);
    }
    catch (Exception ex)
    {
        var failedResponse = new ApiResponse<List<P2PAdvertisement>>();
        return failedResponse.AddError(ex.Message);
    }
}
```

## Notes

*   **Null Safety**: All extension methods in this class assume the source object (`this` parameter) is not `null`. Passing a `null` reference will result in a standard `ArgumentNullException` thrown by the runtime before the method logic executes.
*   **Mutability**: The `AddError` and `AddError<T>` methods operate mutably on the provided instance, modifying its internal error collection and returning the same reference. Callers should be aware that the original object state is altered. Conversely, `WithData<T>` typically implies the creation of a new instance to ensure immutability of the source, though implementation details should be verified if chaining mutable operations.
*   **Thread Safety**: As these methods primarily manipulate local state or create new instances without utilizing shared static mutable fields, they are generally thread-safe regarding the method logic itself. However, if the underlying `ApiResponse` instance is shared across threads, external synchronization is required when calling `AddError` to prevent race conditions during error list modification.
*   **Generic Type Inference**: The `WithData<T>` and `Summary<T>` methods rely on the generic type `T` being inferred from the `this` parameter or explicitly specified. Ensure the input response object is strongly typed to avoid inference failures.

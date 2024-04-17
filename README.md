// entire file content ...
// ... goes in between

## ApiResponse

The `ApiResponse` class provides a standardized way to return data from API operations, including success and error responses. It allows for consistent output formatting and error handling.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

// Create a successful response with data
var successfulResponse = ApiResponse.SuccessResult(new { id = 1, name = "John" }, "User retrieved successfully");

// Create a failed response with a single error
var failedResponse = ApiResponse.ErrorResult("Invalid input", "Operation failed");

// Create a failed response with multiple errors
var failedResponseWithMultipleErrors = ApiResponse.ErrorResult(new List<string> { "Invalid input", "Another error" }, "Operation failed");

// Access response properties
Console.WriteLine($"Success: {successfulResponse.Success}");
Console.WriteLine($"Data: {successfulResponse.Data}");
Console.WriteLine($"Message: {successfulResponse.Message}");
Console.WriteLine($"Errors: {string.Join(", ", successfulResponse.Errors)}");
Console.WriteLine($"Timestamp: {successfulResponse.Timestamp}");
Console.WriteLine($"RequestId: {successfulResponse.RequestId}");
```
// ... rest of file content ...

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

## RateLimiter

The `RateLimiter` class implements a token bucket rate limiting algorithm to control API request frequency. It maintains separate token buckets for different keys, allowing fine-grained rate limiting per endpoint, user, or IP address.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

// Create a rate limiter with 10 requests per 1 second window
var rateLimiter = new RateLimiter(maxRequests: 10, timeWindow: TimeSpan.FromSeconds(1));

// Check if a request is allowed for a specific key
bool isAllowed = rateLimiter.IsAllowed("api-endpoint-1");

// Get remaining tokens for a key
int remaining = rateLimiter.GetRemainingTokens("api-endpoint-1");

// Get time until next token is available (returns null if tokens are available)
TimeSpan? timeUntilNext = rateLimiter.GetTimeUntilNextToken("api-endpoint-1");

// Reset the bucket for a specific key
rateLimiter.Reset("api-endpoint-1");

// Clear all rate limiting buckets
rateLimiter.ClearAll();
```
// ... rest of file content ...

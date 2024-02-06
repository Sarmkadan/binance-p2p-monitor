# HttpClientFactory

A lightweight factory for creating and reusing `HttpClient` instances specifically tailored for interacting with Binance P2P API endpoints. It centralizes HTTP client creation and provides typed convenience methods for common GET and POST operations, ensuring consistent configuration and resource management across the `binance-p2p-monitor` project.

## API

### `public HttpClientFactory`

Initializes a new factory instance. The factory manages the lifecycle of underlying `HttpClient` instances, ensuring proper disposal and reuse where applicable. No parameters are required for instantiation.

### `public HttpClient CreateApiClient()`

Creates and returns a new `HttpClient` instance preconfigured for Binance P2P API communication. The client includes default headers and timeout settings suitable for API interactions. The returned client is owned by the caller and must be disposed when no longer needed.

**Returns:** A new `HttpClient` instance ready for use with Binance P2P endpoints.

### `public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)`

Performs an HTTP GET request to the specified URL and deserializes the JSON response into an instance of type `T`.

**Parameters:**
- `url` (string): The absolute or relative endpoint URL to request.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Returns:** A task that represents the asynchronous operation. The result is the deserialized object of type `T` if the request succeeds and the response contains valid JSON; otherwise, `null`.

**Exceptions:**
- Throws `HttpRequestException` if the HTTP request fails.
- Throws `JsonException` if the response body cannot be deserialized.
- Throws `OperationCanceledException` if the operation is canceled via the `cancellationToken`.

### `public async Task<T?> PostAsync<T>(string url, HttpContent content, CancellationToken cancellationToken = default)`

Performs an HTTP POST request to the specified URL with the provided content and deserializes the JSON response into an instance of type `T`.

**Parameters:**
- `url` (string): The absolute or relative endpoint URL to request.
- `content` (HttpContent): The content to send in the request body.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Returns:** A task that represents the asynchronous operation. The result is the deserialized object of type `T` if the request succeeds and the response contains valid JSON; otherwise, `null`.

**Exceptions:**
- Throws `HttpRequestException` if the HTTP request fails.
- Throws `JsonException` if the response body cannot be deserialized.
- Throws `OperationCanceledException` if the operation is canceled via the `cancellationToken`.

### `public async Task<string> GetStringAsync(string url, CancellationToken cancellationToken = default)`

Performs an HTTP GET request to the specified URL and returns the raw response body as a string.

**Parameters:**
- `url` (string): The absolute or relative endpoint URL to request.
- `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.

**Returns:** A task that represents the asynchronous operation. The result is the response body as a string if the request succeeds; otherwise, `null`.

**Exceptions:**
- Throws `HttpRequestException` if the HTTP request fails.
- Throws `OperationCanceledException` if the operation is canceled via the `cancellationToken`.

## Usage

### Example 1: Fetching P2P order data

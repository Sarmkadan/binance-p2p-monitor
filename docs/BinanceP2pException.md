# BinanceP2pException

`BinanceP2pException` is a custom exception type designed to encapsulate errors encountered during interactions with the Binance P2P (Peer-to-Peer) trading API. It extends standard exception handling by including contextual data such as error codes, HTTP status codes, and additional metadata relevant to P2P operations, enabling more granular error analysis and recovery.

## API

### `public string? ErrorCode`
A string identifier representing the specific error type or category. This field is optional and may be `null` if the error does not map to a known Binance P2P error code. It is typically populated from API responses or internal validation logic.

### `public Dictionary<string, object>? Context`
A dictionary containing additional contextual information about the error. Keys are descriptive strings (e.g., `"orderId"`, `"symbol"`, `"timestamp"`), and values are arbitrary objects representing relevant data at the time of the error. This field is optional and may be `null` if no context is available.

### `public BinanceP2pException(string? message)`
Constructs a new `BinanceP2pException` with a custom error message. The message provides a human-readable explanation of the failure. This constructor is typically used when wrapping lower-level exceptions or generating custom error messages.

### `public BinanceP2pException(string? message, Exception? innerException)`
Constructs a new `BinanceP2pException` with a custom error message and an inner exception. This is useful for preserving the exception stack trace and cause when rethrowing or wrapping exceptions from underlying APIs or services.

### `public override string ToString()`
Returns a string representation of the exception, including the error message, error code (if present), HTTP status code (if present), and the full context dictionary (if present). The output is formatted for readability and debugging purposes.

### `public InvalidPriceException(string? message)`
Constructs a specialized `BinanceP2pException` indicating that an invalid price was provided or encountered during a P2P trade operation. This exception type is typically thrown during price validation or order submission.

### `public InvalidAlertException(string? message)`
Constructs a specialized `BinanceP2pException` indicating that an invalid alert configuration or payload was encountered. This may occur when parsing or validating alert rules for P2P trade monitoring.

### `public DataAccessException(string? message)`
Constructs a specialized `BinanceP2pException` indicating a failure in data access operations, such as reading from or writing to a local cache, database, or persistent storage used by the P2P monitor.

### `public int? HttpStatusCode`
An optional integer representing the HTTP status code returned by the Binance P2P API in response to a failed request. This field is `null` if the exception was not triggered by an HTTP response or if the status code is unavailable.

### `public ApiException(string? message)`
Constructs a specialized `BinanceP2pException` indicating a failure in communication with the Binance P2P API. This may include network issues, timeouts, or malformed responses.

### `public ApiException(string? message, Exception? innerException)`
Constructs a specialized `BinanceP2pException` indicating an API communication failure with an inner exception. This preserves the original exception chain for debugging.

### `public ConfigurationException(string? message)`
Constructs a specialized `BinanceP2pException` indicating an invalid or missing configuration value required for P2P operations. This may include API keys, endpoints, or alert thresholds.

### `public ResourceNotFoundException(string? message)`
Constructs a specialized `BinanceP2pException` indicating that a required resource (e.g., order, user, symbol) was not found during a P2P operation.

### `public List<string> ValidationErrors`
A list of human-readable validation error messages. This field is populated when input validation fails, such as invalid order parameters or alert rules. The list may be empty if no validation errors occurred.

### `public ValidationException(string? message)`
Constructs a specialized `BinanceP2pException` indicating that input validation failed. This exception type is typically thrown after collecting multiple validation errors into a single exception, with the `ValidationErrors` list containing detailed messages.

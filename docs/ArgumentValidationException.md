# ArgumentValidationException

A specialized exception type used to aggregate and report validation errors for method arguments in the `binance-p2p-monitor` project. It collects multiple validation failure messages in a dictionary and provides a structured way to inspect and format these errors.

## API

### `public Dictionary<string, string> ValidationErrors`
A read-only dictionary mapping argument names to their corresponding validation error messages. Populated during construction and immutable thereafter.

### `public ArgumentValidationException()`
Constructs an empty `ArgumentValidationException` with no validation errors.

### `public ArgumentValidationException(string message)`
Constructs an `ArgumentValidationException` with a single top-level error message. The `ValidationErrors` dictionary will be empty.

**Parameters**
- `message` – A string describing the overall validation failure.

### `public ArgumentValidationException(string message, Exception innerException)`
Constructs an `ArgumentValidationException` with a top-level error message and an inner exception. The `ValidationErrors` dictionary will be empty.

**Parameters**
- `message` – A string describing the overall validation failure.
- `innerException` – The exception that caused this validation failure.

### `public override string ToString()`
Returns a formatted string representation of the exception, including all validation errors and any inner exception.

**Returns**
- A string containing the exception type, top-level message (if present), all validation errors, and the inner exception (if present).

## Usage

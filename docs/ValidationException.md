# ValidationException

A custom exception type used to aggregate and report multiple validation errors in a structured way, typically within the Binance P2P monitor project for input validation scenarios.

## API

### `public List<string> Errors`

Gets the collection of validation error messages. This list is mutable and can be modified after the exception is created.

### `public ValidationException(string message) : base(message)`

Constructs a new `ValidationException` with a single error message. The provided message is added to the `Errors` list.

- **Parameters**
  - `message` (string): The validation error message to include in the exception.

### `public ValidationException(List<string> errors) : base(string.Join(Environment.NewLine, errors))`

Constructs a new `ValidationException` from a collection of error messages. Each message is joined into a single string separated by newlines to form the exception message.

- **Parameters**
  - `errors` (List<string>): The list of validation error messages to include in the exception.

### `public ValidationException(string message, List<string> errors) : base(message)`

Constructs a new `ValidationException` with a primary message and additional error messages. The primary message is used as the exception message, while all messages (including the primary) are added to the `Errors` list.

- **Parameters**
  - `message` (string): The primary validation error message.
  - `errors` (List<string>): Additional validation error messages to include.

### `public void AddError(string error)`

Adds a new validation error message to the `Errors` list. This method allows dynamic accumulation of errors during validation logic.

- **Parameters**
  - `error` (string): The validation error message to add.
- **Exceptions**
  - `ArgumentNullException`: Thrown if `error` is `null`.

## Usage

```csharp
// Example 1: Creating an exception with a single error
try
{
    throw new ValidationException("Price must be positive.");
}
catch (ValidationException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Errors: {string.Join(", ", ex.Errors)}");
}

// Example 2: Accumulating multiple errors during validation
var validationErrors = new List<string>();
if (price <= 0)
    validationErrors.Add("Price must be positive.");
if (quantity <= 0)
    validationErrors.Add("Quantity must be positive.");

if (validationErrors.Count > 0)
{
    throw new ValidationException("Validation failed.", validationErrors);
}
```

## Notes

- The `Errors` list is not thread-safe. If multiple threads may modify it concurrently, external synchronization is required.
- When constructing the exception with a list of errors, the `Errors` property will contain all messages passed, including duplicates or empty strings if present in the input list.
- The base exception message is derived differently depending on the constructor used: either the single `message` parameter or a newline-joined string of all errors. This may affect logging or display logic that relies on `Message`.
- The `AddError` method does not validate for empty strings; such values will be added to the `Errors` list without filtering.

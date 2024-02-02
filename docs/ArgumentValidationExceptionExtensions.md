# ArgumentValidationExceptionExtensions

Provides extension methods for the `ArgumentValidationException` type, enabling the addition of individual or multiple error messages to an existing exception instance, as well as querying the stored errors. This class is designed to simplify the construction and inspection of validation error collections within the `binance-p2p-monitor` project.

## API

### `WithAdditionalErrors`

```csharp
public static ArgumentValidationException WithAdditionalErrors(
    this ArgumentValidationException exception,
    IEnumerable<string> additionalErrors)
```

Adds a collection of error messages to the exception and returns a new `ArgumentValidationException` instance that contains both the original errors and the newly supplied ones.

- **Parameters**  
  - `exception`: The `ArgumentValidationException` to which errors will be added. Must not be `null`.  
  - `additionalErrors`: A sequence of error message strings to append. May be empty; `null` is treated as an empty collection.

- **Returns**  
  A new `ArgumentValidationException` that combines the original errors with the provided `additionalErrors`. The original exception is not modified.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

### `WithError`

```csharp
public static ArgumentValidationException WithError(
    this ArgumentValidationException exception,
    string error)
```

Adds a single error message to the exception and returns a new `ArgumentValidationException` instance that contains both the original errors and the new message.

- **Parameters**  
  - `exception`: The `ArgumentValidationException` to which the error will be added. Must not be `null`.  
  - `error`: The error message string to append. May be `null` or empty; such values are stored as-is.

- **Returns**  
  A new `ArgumentValidationException` that combines the original errors with the provided `error`. The original exception is not modified.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

### `GetAllErrorMessages`

```csharp
public static string GetAllErrorMessages(
    this ArgumentValidationException exception)
```

Returns a single string that concatenates all error messages stored in the exception, separated by a newline character.

- **Parameters**  
  - `exception`: The `ArgumentValidationException` whose errors are to be retrieved. Must not be `null`.

- **Returns**  
  A string containing all error messages, each on a new line. If the exception contains no errors, an empty string is returned.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.

### `HasErrorFor`

```csharp
public static bool HasErrorFor(
    this ArgumentValidationException exception,
    string key)
```

Determines whether the exception contains an error associated with the specified key (e.g., a property name or field identifier).

- **Parameters**  
  - `exception`: The `ArgumentValidationException` to inspect. Must not be `null`.  
  - `key`: The identifier to look up. Must not be `null` or empty.

- **Returns**  
  `true` if an error with the given key exists; otherwise `false`.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.  
  `ArgumentException` if `key` is `null` or empty.

### `GetErrorMessage`

```csharp
public static string? GetErrorMessage(
    this ArgumentValidationException exception,
    string key)
```

Retrieves the error message associated with the specified key, or `null` if no such error exists.

- **Parameters**  
  - `exception`: The `ArgumentValidationException` to query. Must not be `null`.  
  - `key`: The identifier for the error message. Must not be `null` or empty.

- **Returns**  
  The error message string if the key is found; otherwise `null`.

- **Throws**  
  `ArgumentNullException` if `exception` is `null`.  
  `ArgumentException` if `key` is `null` or empty.

## Usage

### Example 1: Building an exception with multiple errors

```csharp
var validationEx = new ArgumentValidationException("Initial error")
    .WithError("Field 'Amount' must be positive")
    .WithAdditionalErrors(new[] { "Field 'Symbol' is required", "Field 'Price' is out of range" });

string allMessages = validationEx.GetAllErrorMessages();
Console.WriteLine(allMessages);
// Output:
// Initial error
// Field 'Amount' must be positive
// Field 'Symbol' is required
// Field 'Price' is out of range
```

### Example 2: Checking for specific errors

```csharp
var ex = new ArgumentValidationException("General validation failure")
    .WithError("Amount: must be greater than zero")
    .WithError("Symbol: cannot be empty");

bool hasAmountError = ex.HasErrorFor("Amount");       // true
string? amountMsg = ex.GetErrorMessage("Amount");     // "must be greater than zero"
bool hasPriceError = ex.HasErrorFor("Price");         // false
string? priceMsg = ex.GetErrorMessage("Price");       // null
```

## Notes

- All methods are extension methods on `ArgumentValidationException` and are defined in a static class. They do not modify the original exception; each `With*` method returns a new instance, making the operations safe for concurrent reads on the original object.
- If the `exception` argument is `null`, every method throws `ArgumentNullException`. Always ensure the exception instance is non-null before calling these extensions.
- The `additionalErrors` parameter in `WithAdditionalErrors` accepts `null` without throwing; it is treated as an empty collection. Individual `null` entries within the collection are stored as-is.
- `HasErrorFor` and `GetErrorMessage` consider keys case-sensitively. The exact comparison behavior depends on the internal storage of the `ArgumentValidationException`; by default, keys are compared using ordinal (case-sensitive) rules.
- Thread-safety: Because the `With*` methods produce new instances, and the query methods (`GetAllErrorMessages`, `HasErrorFor`, `GetErrorMessage`) only read from the exception, these extensions are thread-safe when the underlying `ArgumentValidationException` implementation is immutable or behaves as such. No shared mutable state is introduced by these extension methods.

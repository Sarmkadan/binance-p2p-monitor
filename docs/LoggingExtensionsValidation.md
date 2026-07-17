# LoggingExtensionsValidation

Provides static helper methods for validating logging configuration used throughout the `binance-p2p-monitor` application. The members allow callers to check whether logging settings are correct, retrieve detailed validation messages, or enforce validity by throwing when problems are detected.

## API

### Validate overloads
```csharp
public static IReadOnlyList<string> Validate()
```
* **Purpose** – Performs validation of the logging configuration and returns a read‑only list of error messages. An empty list indicates success.  
* **Parameters** – Varies by overload; see the specific call site for the exact arguments (e.g., logger instance, configuration object, file paths).  
* **Return value** – `IReadOnlyList<string>` containing zero or more validation error descriptions.  
* **Exceptions** – Does not throw; validation failures are reported via the returned list.

```csharp
public static IReadOnlyList<string> Validate()
```
* (Identical description as above; each overload validates a different aspect of logging such as console output, file rolling, or external provider settings.)

```csharp
public static IReadOnlyList<string> Validate()
```
* (Same purpose; distinct parameter set.)

```csharp
public static IReadOnlyList<string> Validate()
```
* (Same purpose; distinct parameter set.)

```csharp
public static IReadOnlyList<string> Validate()
```
* (Same purpose; distinct parameter set.)

```csharp
public static IReadOnlyList<string> Validate()
```
* (Same purpose; distinct parameter set.)

```csharp
public static IReadOnlyList<string> Validate()
```
* (Same purpose; distinct parameter set.)

### IsValid overloads
```csharp
public static bool IsValid()
```
* **Purpose** – Determines whether the logging configuration passes validation without providing detailed messages.  
* **Parameters** – Varies by overload; corresponds to the parameters of the matching `Validate` overload.  
* **Return value** – `true` if the configuration is valid; otherwise `false`.  
* **Exceptions** – None.

```csharp
public static bool IsValid()
```
* (Same purpose; different parameter set.)

```csharp
public static bool IsValid()
```
* (Same purpose; different parameter set.)

```csharp
public static bool IsValid()
```
* (Same purpose; different parameter set.)

```csharp
public static bool IsValid()
```
* (Same purpose; different parameter set.)

```csharp
public static bool IsValid()
```
* (Same purpose; different parameter set.)

```csharp
public static bool IsValid()
```
* (Same purpose; different parameter set.)

### EnsureValid overloads
```csharp
public static void EnsureValid()
```
* **Purpose** – Validates the logging configuration and throws an exception if any problems are found, guaranteeing that the caller can proceed only when the configuration is correct.  
* **Parameters** – Varies by overload; matches the parameters of the corresponding `Validate` overload.  
* **Return value** – None.  
* **Exceptions** – Throws `InvalidOperationException` (or a derived type) containing a concatenated message of all validation errors when the configuration is invalid.

```csharp
public static void EnsureValid()
```
* (Same purpose; different parameter set.)

```csharp
public static void EnsureValid()
```
* (Same purpose; different parameter set.)

```csharp
public static void EnsureValid()
```
* (Same purpose; different parameter set.)

```csharp
public static void EnsureValid()
```
* (Same purpose; different parameter set.)

```csharp
public static void EnsureValid()
```
* (Same purpose; different parameter set.)

```csharp
public static void EnsureValid()
```
* (Same purpose; different parameter set.)

## Usage

### Example 1: Simple validation check
```csharp
using BinanceP2pMonitor.Logging;

var loggerConfig = LoadLoggerConfiguration(); // application‑specific method
if (!LoggingExtensionsValidation.IsValid(loggerConfig))
{
    var errors = LoggingExtensionsValidation.Validate(loggerConfig);
    foreach var err in errors
    {
        Console.WriteLine($"Logging error: {err}");
    }
    // handle misconfiguration, e.g., fallback to default logger
}
else
{
    // configuration is safe to use
    InitializeLogger(loggerConfig);
}
```

### Example 2: Enforcing validity with exception handling
```csharp
using BinanceP2pMonitor.Logging;

try
{
    LoggingExtensionsValidation.EnsureValid(loggerConfig);
    // If we reach this point, the logger configuration is guaranteed valid.
    StartMonitoringWithLogger(loggerConfig);
}
catch (InvalidOperationException ex)
{
    // Detailed validation messages are included in the exception.
    Logger.Error(ex, "Failed to start monitoring due to invalid logging configuration.");
    Environment.Exit(1);
}
```

## Notes

* All members are **static** and thread‑safe; they rely only on their input parameters and contain no mutable state.  
* The overloads differ only in the types and number of parameters they accept (e.g., `ILoggerFactory`, `LoggingSettings`, file paths, or provider‑specific objects). Callers should select the overload that matches the data they have available.  
* `Validate` never throws; it accumulates errors in the returned list, making it suitable for UI or logging scenarios where you want to report all problems at once.  
* `IsValid` is a convenience wrapper that returns a single Boolean; it does not provide the underlying error details.  
* `EnsureValid` throws on the first detection of any validation failure; the exception’s message aggregates all errors returned by the corresponding `Validate` overload, so no information is lost.  
* Passing `null` for any argument that is not explicitly allowed results in an `ArgumentNullException` thrown by the underlying validation logic (not by the wrapper itself).  
* Because the methods do not store state, they can be invoked concurrently from multiple threads without additional synchronization.

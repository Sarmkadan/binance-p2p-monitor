# ExceptionHandlingMiddleware

Middleware for centralized exception handling and error reporting.

## Overview

The `ExceptionHandlingMiddleware` is responsible for catching and handling exceptions that occur during command execution in the Binance P2P Monitor application. It provides centralized error logging, user-friendly error output, and appropriate exit codes.

## Constructor

```csharp
public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, Func<CommandContext, Task<int>> next)
```

### Parameters

- `logger`: An instance of `ILogger<ExceptionHandlingMiddleware>` for logging exceptions.
- `next`: A delegate representing the next middleware in the pipeline, which takes a `CommandContext` and returns a `Task<int>` (exit code).

## Method: InvokeAsync

```csharp
public async Task<int> InvokeAsync(CommandContext context)
```

### Parameters

- `context`: The command execution context, which may contain flags (e.g., "verbose") that affect error output behavior.

### Returns

An integer exit code indicating the result of the command execution:
- `0`: Success (if no exception occurs and the next middleware returns 0).
- `1`: General error (for most exception types).
- `130`: Operation cancelled by user (for `OperationCanceledException`).

### Exception Handling

The middleware catches specific exception types and handles them as follows:

#### BinanceP2pException
- Logs the error using `ILogger.LogError` with the message "Binance P2P exception: {Message}".
- Writes the error message to `Console.Error` in the format: `Error: {ex.Message}`.
- Returns exit code `1`.

#### Utilities.ValidationException
- Logs the warning using `ILogger.LogWarning` with the message "Validation error: {Message}".
- Writes the validation error message to `Console.Error` in the format: `Validation error: {ex.Message}`.
- Iterates through `ex.Errors` (a collection of validation error strings) and writes each to `Console.Error` prefixed with `  - `.
- Returns exit code `1`.

#### InvalidOperationException
- Logs the error using `ILogger.LogError` with the message "Invalid operation: {Message}".
- Writes the error message to `Console.Error` in the format: `Invalid operation: {ex.Message}`.
- Returns exit code `1`.

#### OperationCanceledException
- Logs the information using `ILogger.LogInformation` with the message "Operation cancelled by user".
- Returns exit code `130` (standard exit code for SIGINT/SIGTERM).

#### Exception (all other exceptions)
- Logs the error using `ILogger.LogError` with the message "Unexpected error occurred: {Message}".
- Writes the error message to `Console.Error` in the format: `Unexpected error: {ex.Message}`.
- If the context has the "verbose" flag set (`context.HasFlag("verbose")`), also writes the exception's stack trace to `Console.Error`.
- Returns exit code `1`.

## Usage

This middleware is typically used in the application's command execution pipeline. It wraps the next middleware (or command handler) and ensures that any exceptions are caught, logged, and presented to the user in a consistent manner.

## Notes

- The middleware does not swallow exceptions; it handles them by logging and returning an appropriate exit code.
- The "verbose" flag in the `CommandContext` controls whether stack traces are printed for unexpected errors.
- All handled exceptions result in a non-zero exit code, signaling failure to the calling process.
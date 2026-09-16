# LoggingMiddleware

Middleware for comprehensive command execution logging.

## Overview

The `LoggingMiddleware` class logs command execution details including the command name, arguments, options, flags, execution time, and result (success, cancellation, or failure).

## Constructor

```csharp
public LoggingMiddleware(ILogger<LoggingMiddleware> logger, Func<CommandContext, Task<int>> next)
```

### Parameters

- `logger`: An instance of `ILogger<LoggingMiddleware>` used for logging.
- `next`: A delegate representing the next middleware in the pipeline, which takes a `CommandContext` and returns a `Task<int>` (exit code).

## Method: InvokeAsync

```csharp
public async Task<int> InvokeAsync(CommandContext context)
```

### Parameters

- `context`: The command execution context containing:
  - `CommandName`: The name of the command being executed.
  - `Arguments`: A list of command arguments.
  - `Options`: A dictionary of command options (key-value pairs).
  - `Flags`: A set of command flags (keys only).

### Behavior

1. Starts a stopwatch to measure execution time.
2. Constructs a command info string from the command name and arguments.
3. Logs the command execution at the `Information` level.
4. If options are present, logs them at the `Debug` level.
5. If flags are present, logs them at the `Debug` level.
6. Executes the next middleware via `_next(context)`.
7. On successful completion:
   - Stops the stopwatch.
   - Logs the command completion, elapsed time in milliseconds, and exit code at the `Information` level.
   - Returns the exit code.
8. On `OperationCanceledException`:
   - Stops the stopwatch.
   - Logs a warning about command cancellation, elapsed time, and returns `-1`.
9. On any other exception:
   - Stops the stopwatch.
   - Logs the error (including exception details) at the `Error` level.
   - Re-throws the exception.

### Return Value

- The exit code from the command execution (returned by `_next(context)`).
- `-1` if the command was cancelled.
- If an exception occurs, the exception is re-thrown after logging.

## Example Log Output

```
Information: Executing command: update-config --verbose
Debug: Options: verbose=True
Information: Command completed: update-config --verbose in 125ms with exit code 0
```

```
Warning: Command cancelled: long-running-task after 5000ms
```

```
Error: System.InvalidOperationException: Configuration not found
   at BinanceP2pMonitor.Commands.UpdateConfigHandler.HandleAsync(CommandContext context)
   ...
Information: Command failed: update-config in 300ms with exit code 1
```

## Notes

- The middleware ensures that all command executions are logged with timing information.
- Cancellation is handled gracefully and logged as a warning.
- Unexpected errors are logged with full exception details and then re-thrown to allow higher-level error handling.
- The middleware does not modify the command execution; it only observes and logs.
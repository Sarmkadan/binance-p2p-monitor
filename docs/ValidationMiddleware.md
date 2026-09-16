# ValidationMiddleware

Middleware for validating command arguments before execution.

## Overview

The `ValidationMiddleware` class is responsible for validating command arguments and options before command execution. It ensures that commands are properly formed and have all required parameters before allowing them to proceed to execution.

## Responsibilities

- Validates that the requested command exists
- Validates command arguments using the command's built-in validation logic
- Provides clear error messages to users when validation fails
- Logs validation activities for debugging purposes
- Prevents execution of invalid commands

## Implementation Details

### Constructor

```csharp
public ValidationMiddleware(ILogger<ValidationMiddleware> logger, Func<CommandContext, ICommand?, Task<int>> next)
```

Parameters:
- `logger`: Logger instance for logging validation activities
- `next`: Delegate representing the next middleware in the pipeline

### InvokeAsync Method

```csharp
public async Task<int> InvokeAsync(CommandContext context, ICommand? command)
```

Process:
1. **Command Existence Check**: Verifies that the command is not null
   - If command is null, logs warning and returns error code 1
   - Displays user-friendly error message indicating unknown command

2. **Argument Validation**: Calls `command.ValidateArguments(context)` to validate command-specific arguments
   - If validation errors exist, logs warning and displays each error to user
   - Provides hint to use `--help` for usage information
   - Returns error code 1

3. **Successful Validation**: If validation passes
   - Logs debug message indicating successful validation
   - Proceeds to execute the next middleware in the pipeline

### Error Handling

The middleware returns error code `1` for all validation failures:
- Unknown command: Command not found in command registry
- Validation failures: Command-specific argument validation errors

Error messages are written to `Console.Error` for proper separation from standard output.

### Logging

Uses `ILogger<ValidationMiddleware>` for structured logging:
- Warning level: Command not found or validation failures
- Debug level: Successful validation passage

## Usage

This middleware is automatically registered in the middleware pipeline and processes every command invocation before the command's actual execution logic runs.

## Related Components

- `CommandContext`: Contains execution context including command name and arguments
- `ICommand`: Interface that commands implement, providing the `ValidateArguments` method
- Middleware pipeline: Part of the command processing chain that includes logging, execution, and other concerns

## Conventions

- Follows standard middleware pattern with `InvokeAsync` method
- Uses dependency injection for logger and next middleware delegate
- Returns integer exit codes consistent with console application conventions
- Separates error output (`Console.Error`) from regular output
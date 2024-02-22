# HistoryCommand

The `HistoryCommand` class represents a command within the `binance-p2p-monitor` application that retrieves and displays historical P2P trade or order data. It follows a common command pattern, exposing members for obtaining help text, validating its configuration, and executing the retrieval operation asynchronously. The command is designed to be instantiated, validated, and then executed, returning an exit code that indicates success or failure.

## API

### `HistoryCommand()`

Initializes a new instance of the `HistoryCommand` class.

### `string GetHelp`

Gets a string that describes the command's purpose, usage syntax, and any supported options or parameters. This property is intended for display to the user when help is requested.

### `List<string> ValidateArguments`

Returns a list of validation error messages. Each string in the list describes a specific issue with the command's current arguments or state. An empty list indicates that the command is valid and ready to execute. This method does not modify the command's state.

### `async Task<int> ExecuteAsync`

Executes the command asynchronously. The returned integer represents the exit code: `0` indicates successful execution, while any non-zero value signals an error or abnormal termination. The method may throw exceptions for unrecoverable failures (e.g., network errors, invalid data sources).

## Usage

The following examples demonstrate typical usage of `HistoryCommand`.

### Example 1: Basic execution

```csharp
var command = new HistoryCommand();
var errors = command.ValidateArguments();

if (errors.Count == 0)
{
    int exitCode = await command.ExecuteAsync();
    Console.WriteLine($"Command exited with code {exitCode}");
}
else
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Displaying help before execution

```csharp
var command = new HistoryCommand();

// Show help text
Console.WriteLine(command.GetHelp);

// Validate and execute only if help was not requested
var errors = command.ValidateArguments();
if (errors.Count == 0)
{
    int exitCode = await command.ExecuteAsync();
    // Handle exit code
}
```

## Notes

- **Edge cases**: The `ValidateArguments` method may return errors for missing or malformed arguments (e.g., invalid date ranges, unsupported filters). The `ExecuteAsync` method may throw exceptions if the underlying data source is unreachable or if the command is executed without prior validation. It is recommended to always check `ValidateArguments` before calling `ExecuteAsync`.
- **Thread safety**: Instances of `HistoryCommand` are not thread-safe. Concurrent access to the same instance from multiple threads may result in undefined behavior. Each thread should use its own instance, or external synchronization must be applied.

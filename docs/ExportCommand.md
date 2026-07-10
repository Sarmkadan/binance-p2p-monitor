# ExportCommand

The `ExportCommand` class serves as a CLI command handler within the `binance-p2p-monitor` application, responsible for orchestrating the export of monitored Binance P2P data. It implements the standard command lifecycle by providing help text, validating input arguments before execution, and performing the asynchronous data export operation, returning an exit code to indicate success or failure.

## API

### `public ExportCommand`
Initializes a new instance of the `ExportCommand` class. This constructor sets up the internal state required for the command to function, typically wiring up dependencies needed for data retrieval and file system operations.

### `public string GetHelp`
Retrieves the help documentation string for this command.
*   **Return Value**: A `string` containing usage instructions, available options, and a brief description of the command's functionality.
*   **Remarks**: This property is read-only and safe to access at any time after instantiation. It does not perform any I/O or asynchronous operations.

### `public List<string> ValidateArguments`
Validates the current set of arguments provided to the command.
*   **Return Value**: A `List<string>` containing error messages. If the list is empty, the arguments are considered valid. If the list contains one or more strings, each string represents a specific validation failure.
*   **Behavior**: This method checks for required parameters, valid formats, and logical consistency between arguments. It does not throw exceptions for validation failures; instead, it aggregates them into the returned list.

### `public async Task<int> ExecuteAsync`
Executes the primary logic of the export command.
*   **Return Value**: A `Task<int>` that resolves to an exit code. Conventionally, `0` indicates success, while non-zero values indicate specific error conditions.
*   **Parameters**: This method typically relies on internal state or context injected during construction or set prior to execution, as no parameters are exposed in the signature.
*   **Exceptions**: May throw exceptions related to I/O failures (e.g., inability to write to the output file), network errors during data fetching, or unexpected runtime states if the command was not validated prior to execution.

## Usage

### Example 1: Standard Execution Flow
This example demonstrates the typical lifecycle of instantiating the command, validating arguments, and executing the export if validation passes.

```csharp
var command = new ExportCommand();

// Assume arguments are set via a separate mechanism or constructor overload not shown here
var errors = command.ValidateArguments();

if (errors.Any())
{
    Console.WriteLine("Invalid arguments:");
    foreach (var error in errors)
    {
        Console.WriteLine($"- {error}");
    }
    return 1;
}

try
{
    int exitCode = await command.ExecuteAsync();
    if (exitCode != 0)
    {
        Console.WriteLine($"Command failed with exit code: {exitCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Critical error during execution: {ex.Message}");
    return -1;
}
```

### Example 2: Retrieving Help Text
This example shows how to retrieve and display the help information for the command when a user requests assistance.

```csharp
var command = new ExportCommand();

// Display help information
Console.WriteLine("Usage Guide for ExportCommand:");
Console.WriteLine(command.GetHelp);

// No execution or validation is performed in this path
```

## Notes

*   **Validation Responsibility**: The `ExecuteAsync` method assumes that `ValidateArguments` has been called successfully prior to invocation. Calling `ExecuteAsync` without prior validation may result in unhandled exceptions or undefined behavior if the internal state is inconsistent.
*   **Thread Safety**: The `GetHelp` property is thread-safe as it returns a static or immutable string. However, `ValidateArguments` and `ExecuteAsync` are not guaranteed to be thread-safe if they interact with shared mutable state within the instance. Concurrent calls to `ExecuteAsync` on the same instance should be avoided.
*   **Asynchronous Nature**: As `ExecuteAsync` performs I/O and network operations, it should always be awaited. Blocking on the returned task (e.g., using `.Result` or `.Wait()`) in a UI or ASP.NET context may lead to deadlocks.
*   **Exit Codes**: Consumers of this command must inspect the integer result of `ExecuteAsync` to determine the outcome, as success is not indicated by the completion of the task alone but by the specific return value.

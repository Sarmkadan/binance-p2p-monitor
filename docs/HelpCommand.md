# HelpCommand

The `HelpCommand` type encapsulates the behavior of the help sub‑command in the binance‑p2p‑monitor CLI. It provides the help text, validates that no unexpected arguments are supplied, and writes the help output to the console when executed.

## API

### HelpCommand()
Initializes a new instance of the `HelpCommand` class.  
- **Parameters:** none  
- **Return value:** a new `HelpCommand` object ready for use.  
- **Exceptions:** none.

### string GetHelp()
Retrieves the formatted help text for the application.  
- **Parameters:** none  
- **Return value:** a string containing the help message.  
- **Exceptions:** may throw `InvalidOperationException` if the internal help resource cannot be loaded.

### List<string> ValidateArguments(IReadOnlyList<string> args)
Validates the arguments supplied to the help command.  
- **Parameters:**  
  - `args`: the command‑line arguments to validate.  
- **Return value:** a list of validation error messages; an empty list indicates the arguments are valid.  
- **Exceptions:** throws `ArgumentNullException` if `args` is `null`.

### Task<int> ExecuteAsync(IReadOnlyList<string> args, CancellationToken cancellationToken = default)
Asynchronously executes the help command, writing the help text to the standard output stream.  
- **Parameters:**  
  - `args`: the command‑line arguments (typically empty or ignored).  
  - `cancellationToken`: optional token to observe for cancellation requests.  
- **Return value:** a task that completes with an exit code (`0` for success, non‑zero for failure).  
- **Exceptions:**  
  - `OperationCanceledException` if `cancellationToken` is triggered.  
  - `IOException` if writing to the console fails.

## Usage

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Create the command instance
var help = new HelpCommand();

// Validate that no extra arguments were passed
IReadOnlyList<string> arguments = new List<string> { "--verbose" };
var errors = help.ValidateArguments(arguments);
if (errors.Count > 0)
{
    foreach (var err in errors)
        Console.Error.WriteLine(err);
    return 1;
}

// Execute the help command
int exitCode = await help.ExecuteAsync(arguments, CancellationToken.None);
return exitCode;
```

```csharp
using System.Threading.Tasks;

// Simple fire‑and‑forget usage when no arguments are expected
var help = new HelpCommand();
await help.ExecuteAsync(System.Array.Empty<string>());
```

## Notes

- The class does not store mutable state after construction; therefore multiple threads can safely invoke `GetHelp`, `ValidateArguments`, and `ExecuteAsync` on the same instance concurrently.  
- `ValidateArguments` does not modify the supplied `args` list; callers may reuse it after validation.  
- `ExecuteAsync` writes to the console; concurrent calls from different threads may interleave output, so external synchronization is required if ordered output is needed.  
- Passing `null` for the `args` parameter to `ValidateArguments` will result in an `ArgumentNullException`.  
- If the help text resource is missing or corrupted, `GetHelp` will throw an `InvalidOperationException`; callers should handle this exception if a fallback message is desired.  
- The `ExecuteAsync` method respects the supplied `cancellationToken`; if cancellation is requested before the help text is written, the method will throw `OperationCanceledException` and no output will be produced.

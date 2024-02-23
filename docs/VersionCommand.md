# VersionCommand

`VersionCommand` is a console-command implementation that prints the application version to the console. It validates its arguments, displays a short help text, and returns an exit code suitable for use in CLI pipelines.

## API

### `public VersionCommand()`

Constructs a new `VersionCommand` instance. No configuration or dependencies are required.

### `public string GetHelp()`

Returns a short help text describing the command’s purpose and usage.

- **Return value**: A non-null string containing the help text.

### `public List<string> ValidateArguments(string[] args)`

Validates the supplied command-line arguments.

- **Parameters**:
  - `args` – The command-line arguments to validate.
- **Return value**: An empty list if the arguments are valid; otherwise, a list of error messages.
- **Exceptions**: Throws `ArgumentNullException` if `args` is null.

### `public Task<int> ExecuteAsync()`

Prints the application version to the console and returns an exit code.

- **Return value**: A `Task<int>` that completes with exit code 0 on success or a non-zero value on failure.
- **Exceptions**: Propagates any exceptions thrown by `ValidateArguments` or during console output.

## Usage

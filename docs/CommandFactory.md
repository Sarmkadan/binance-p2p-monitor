# CommandFactory

`CommandFactory` is a registry and factory for command objects. It stores named command types, validates registration, and creates command instances by name. It is the central point for discovering and instantiating commands in the application, typically used by a command-line interface or interactive console to map user input to executable commands.

## API

### `public CommandFactory()`

Default constructor. Initializes an empty command registry with no registered commands.

### `public void RegisterCommand(string name, System.Type commandType)`

Registers a command type under a unique, case-insensitive name.

- **Parameters:**
  - `name` — A non-null, non-empty string that uniquely identifies the command. Leading and trailing whitespace is trimmed.
  - `commandType` — A `Type` that must implement `ICommand` and have a public parameterless constructor.
- **Throws:**
  - `ArgumentNullException` if `name` is null or `commandType` is null.
  - `ArgumentException` if `name` is empty or whitespace-only, if `commandType` does not implement `ICommand`, or if `commandType` lacks a public parameterless constructor.
  - `InvalidOperationException` if a command with the same name (case-insensitive) is already registered.
- **Return value:** None (void).

### `public ICommand? CreateCommand(string name)`

Creates an instance of a previously registered command.

- **Parameters:**
  - `name` — The case-insensitive name of the command to instantiate. Leading and trailing whitespace is trimmed.
- **Returns:** A new instance of the registered `ICommand` type, or `null` if no command is registered under the given name.
- **Throws:**
  - `ArgumentNullException` if `name` is null.
  - `InvalidOperationException` if the registered type cannot be instantiated at runtime (e.g., the parameterless constructor throws or is unexpectedly inaccessible), wrapping the original exception.

### `public IReadOnlyList<string> GetAvailableCommands()`

Returns all registered command names.

- **Returns:** A read-only list of command names in their originally registered casing. Returns an empty list if no commands are registered.
- **Throws:** Nothing.

### `public bool IsCommandRegistered(string name)`

Checks whether a command name is registered.

- **Parameters:**
  - `name` — The command name to check. Leading and trailing whitespace is trimmed. Case-insensitive.
- **Returns:** `true` if the name is registered; `false` otherwise.
- **Throws:**
  - `ArgumentNullException` if `name` is null.

## Usage

### Example 1: Registering and creating a command

```csharp
var factory = new CommandFactory();

// Register a command type
factory.RegisterCommand("greet", typeof(GreetCommand));

// Check registration
if (factory.IsCommandRegistered("greet"))
{
    ICommand? command = factory.CreateCommand("greet");
    command?.Execute();
}
```

### Example 2: Building a simple dispatcher

```csharp
var factory = new CommandFactory();
factory.RegisterCommand("status", typeof(StatusCommand));
factory.RegisterCommand("fetch", typeof(FetchCommand));

Console.WriteLine("Available commands:");
foreach (var name in factory.GetAvailableCommands())
{
    Console.WriteLine($"  {name}");
}

string userInput = Console.ReadLine()?.Trim() ?? "";
ICommand? cmd = factory.CreateCommand(userInput);

if (cmd is not null)
{
    cmd.Execute();
}
else
{
    Console.WriteLine($"Unknown command: {userInput}");
}
```

## Notes

- **Case insensitivity:** All name lookups (`CreateCommand`, `IsCommandRegistered`) are case-insensitive. `GetAvailableCommands` returns names in their original casing as registered.
- **Whitespace handling:** Names are trimmed before registration and lookup. Registering `"  status "` and `"status"` is treated as the same name and will throw on the second registration.
- **Null returns:** `CreateCommand` returns `null` for unregistered names rather than throwing, allowing callers to handle unknown commands gracefully without exception overhead.
- **Thread safety:** `CommandFactory` is not thread-safe. Concurrent calls to `RegisterCommand` or simultaneous registration and creation may lead to race conditions. Synchronization must be applied externally if shared across threads.
- **Type validation:** Registration validates that the type implements `ICommand` and has a public parameterless constructor at registration time, not at creation time. This catches configuration errors early.
- **Lifetime:** Each call to `CreateCommand` produces a new instance. The factory does not cache, reuse, or dispose of created commands.

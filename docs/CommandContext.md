# CommandContext
The `CommandContext` class is a central component in the `binance-p2p-monitor` project, providing a unified way to access and manipulate command-related data and services. It serves as a bridge between the command execution logic and the underlying infrastructure, allowing for flexible and extensible command handling.

## API
* `public string CommandName`: Gets the name of the command being executed.
* `public string[] Arguments`: Gets an array of arguments passed to the command.
* `public Dictionary<string, string> Options`: Gets a dictionary of options provided to the command.
* `public Dictionary<string, string> Flags`: Gets a dictionary of flags set for the command.
* `public IServiceProvider ServiceProvider`: Gets the service provider instance associated with the command context.
* `public CancellationToken CancellationToken`: Gets the cancellation token for the command execution.
* `public bool HasOption`: Gets a value indicating whether an option is present.
* `public bool HasFlag`: Gets a value indicating whether a flag is set.
* `public string? GetOption`: Gets the value of a specific option, or `null` if not found.
* `public string GetOption`: Overload to retrieve an option value, throws if not found.
* `public T? GetService<T>`: Attempts to retrieve a service of type `T` from the service provider, returns `null` if not found.
* `public T GetRequiredService<T>`: Retrieves a service of type `T` from the service provider, throws if not found.

## Usage
```csharp
// Example 1: Accessing command arguments and options
var context = new CommandContext("example", new[] { "arg1", "arg2" }, new Dictionary<string, string> { { "opt1", "value1" } });
Console.WriteLine(context.CommandName); // Output: example
Console.WriteLine(string.Join(", ", context.Arguments)); // Output: arg1, arg2
Console.WriteLine(context.Options["opt1"]); // Output: value1
```

```csharp
// Example 2: Using services and cancellation token
var serviceProvider = new ServiceCollection().AddTransient<ILogger, Logger>().BuildServiceProvider();
var context = new CommandContext("example", Array.Empty<string>(), new Dictionary<string, string>(), serviceProvider);
var logger = context.GetService<ILogger>();
if (logger != null)
{
    logger.LogInformation("Command execution started");
}
context.CancellationToken.Register(() => Console.WriteLine("Command execution cancelled"));
```

## Notes
When using the `GetOption` and `GetService` methods, be aware that they may return `null` if the requested option or service is not found. The `GetRequiredService` method, on the other hand, will throw an exception if the service is not registered. Additionally, the `CancellationToken` property can be used to handle command execution cancellation, but it is the responsibility of the command handler to properly handle cancellation requests. The `CommandContext` class is designed to be thread-safe, but it is still important to follow proper synchronization practices when accessing and manipulating its properties and services.

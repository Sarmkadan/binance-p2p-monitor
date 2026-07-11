# VersionCommandExtensions

The `VersionCommandExtensions` static class provides extension methods and utility functions for handling version-related command operations in the `binance-p2p-monitor` application. It centralizes the retrieval of version information, argument validation, and asynchronous printing of version details, enabling consistent behavior across command-line entry points.

## API

### `GetVersionString`

```csharp
public static string GetVersionString
```

**Purpose**  
Returns a human-readable string representing the current application version.

**Parameters**  
None.

**Return value**  
A `string` containing the version identifier (e.g., `"1.2.3"` or `"1.2.3+build123"`).

**Exceptions**  
This member does not throw exceptions under normal circumstances.

---

### `GetInfoDictionary`

```csharp
public static IReadOnlyDictionary<string, string> GetInfoDictionary
```

**Purpose**  
Provides a read-only dictionary of key-value pairs describing the application’s version and build metadata. Typical keys might include `"Version"`, `"BuildDate"`, `"CommitHash"`, or `"Runtime"`.

**Parameters**  
None.

**Return value**  
An `IReadOnlyDictionary<string, string>` containing version-related information. The dictionary is immutable and safe for concurrent reads.

**Exceptions**  
This member does not throw exceptions under normal circumstances.

---

### `ValidateNoArguments`

```csharp
public static bool ValidateNoArguments
```

**Purpose**  
Validates that no command-line arguments have been supplied. This is typically used to ensure that a version command is invoked without extra parameters.

**Parameters**  
None.

**Return value**  
`true` if no arguments are present; otherwise `false`.

**Exceptions**  
This member does not throw exceptions under normal circumstances.

---

### `PrintVersionInfoAsync`

```csharp
public static Task<int> PrintVersionInfoAsync
```

**Purpose**  
Asynchronously prints the full version information (typically obtained from `GetVersionString` or `GetInfoDictionary`) to the standard output stream. The method returns an exit code suitable for terminating the process.

**Parameters**  
None.

**Return value**  
A `Task<int>` that completes with an exit code. A value of `0` indicates success; any non‑zero value indicates an error (e.g., if output could not be written).

**Exceptions**  
This method may throw an `IOException` if writing to the standard output fails (e.g., when the console is redirected or unavailable). Other exceptions may be thrown by underlying I/O operations.

---

## Usage

### Example 1: Basic version command handler

```csharp
using System.Threading.Tasks;
using YourNamespace.VersionCommandExtensions;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--version")
        {
            if (!ValidateNoArguments)
            {
                await Console.Error.WriteLineAsync("The --version command does not accept arguments.");
                return 1;
            }

            return await PrintVersionInfoAsync();
        }

        // Other command handling...
        return 0;
    }
}
```

### Example 2: Retrieving version metadata for logging

```csharp
using System;
using Microsoft.Extensions.Logging;
using YourNamespace.VersionCommandExtensions;

public class Startup
{
    private readonly ILogger<Startup> _logger;

    public Startup(ILogger<Startup> logger)
    {
        _logger = logger;
    }

    public void LogVersion()
    {
        var version = GetVersionString;
        var info = GetInfoDictionary;

        _logger.LogInformation("Application version: {Version}", version);
        foreach (var kvp in info)
        {
            _logger.LogDebug("{Key}: {Value}", kvp.Key, kvp.Value);
        }
    }
}
```

## Notes

- **Edge cases**  
  - `ValidateNoArguments` returns `true` only when no arguments are present. If the application uses a command‑line parser that normalizes arguments (e.g., removing empty entries), the result may differ from raw `args.Length`.  
  - `PrintVersionInfoAsync` writes to `System.Console.Out`. If the output stream is closed or redirected to a non‑seekable device, an `IOException` may be thrown. Callers should handle this exception appropriately.  
  - `GetInfoDictionary` returns a snapshot of the version metadata at the time of the call. The dictionary is not updated if the application’s version changes during runtime (which is unlikely in a typical process).

- **Thread safety**  
  All members are static and do not modify any shared mutable state. `GetVersionString` and `GetInfoDictionary` are safe to call concurrently from multiple threads. `PrintVersionInfoAsync` serializes writes to the console output, but concurrent calls may interleave output; callers should synchronize access if deterministic output order is required.

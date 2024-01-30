# LoggingExtensions

Provides logging extensions and file-based logging infrastructure for the Binance P2P Monitor application. Includes performance tracking, price change notifications, alert logging, and database operation logging capabilities with file-based output.

## API

### `AddFileLogging`

Adds file-based logging to the `ILoggingBuilder`.

```csharp
public static ILoggingBuilder AddFileLogging(this ILoggingBuilder builder)
```

**Parameters**
- `builder`: The `ILoggingBuilder` instance to configure.

**Return Value**
- The configured `ILoggingBuilder` for method chaining.

**Throws**
- `ArgumentNullException`: If `builder` is `null`.

---

### `LogPerformance`

Logs performance metrics with optional context.

```csharp
public static void LogPerformance(this ILogger logger, string operation, TimeSpan duration, object? context = null)
```

**Parameters**
- `logger`: The logger instance.
- `operation`: The name of the operation being measured.
- `duration`: The duration of the operation.
- `context`: Optional additional context data.

**Throws**
- `ArgumentNullException`: If `logger` or `operation` is `null`.

---

### `LogPriceChange`

Logs price change events with direction and magnitude.

```csharp
public static void LogPriceChange(this ILogger logger, decimal oldPrice, decimal newPrice, string symbol)
```

**Parameters**
- `logger`: The logger instance.
- `oldPrice`: The previous price value.
- `newPrice`: The new price value.
- `symbol`: The trading symbol (e.g., "USDT").

**Throws**
- `ArgumentNullException`: If `logger` or `symbol` is `null`.

---

### `LogAlert`

Logs alert events with severity and message.

```csharp
public static void LogAlert(this ILogger logger, LogLevel level, string message, object? context = null)
```

**Parameters**
- `logger`: The logger instance.
- `level`: The severity level of the alert.
- `message`: The alert message.
- `context`: Optional additional context data.

**Throws**
- `ArgumentNullException`: If `logger` or `message` is `null`.

---
### `LogDatabaseOperation`

Logs database operation events with operation type and status.

```csharp
public static void LogDatabaseOperation(this ILogger logger, string operation, bool success, object? context = null)
```

**Parameters**
- `logger`: The logger instance.
- `operation`: The database operation being performed (e.g., "Insert", "Update").
- `success`: Whether the operation succeeded.
- `context`: Optional additional context data.

**Throws**
- `ArgumentNullException`: If `logger` or `operation` is `null`.

---
### `FileLoggerProvider`

Factory for creating `FileLogger` instances.

```csharp
public sealed class FileLoggerProvider : ILoggerProvider, IDisposable
```

**Implements**
- `ILoggerProvider`
- `IDisposable`

---
### `CreateLogger`

Creates a new `FileLogger` instance.

```csharp
public ILogger CreateLogger(string categoryName)
```

**Parameters**
- `categoryName`: The category name for the logger.

**Return Value**
- A new `FileLogger` instance.

**Throws**
- `ArgumentNullException`: If `categoryName` is `null`.

---
### `Dispose`

Disposes the provider and releases resources.

```csharp
public void Dispose()
```

---
### `FileLogger`

File-based logger implementation.

```csharp
public sealed class FileLogger : ILogger, IDisposable
```

**Implements**
- `ILogger`
- `IDisposable`

---
### `BeginScope<TState>`

Begins a logical operation scope.

```csharp
public IDisposable BeginScope<TState>(TState state)
```

**Type Parameters**
- `TState`: The type of the state object.

**Parameters**
- `state`: The identifier for the scope.

**Return Value**
- A disposable object that ends the logical operation scope on dispose.

---
### `IsEnabled`

Checks if the given log level is enabled.

```csharp
public bool IsEnabled(LogLevel logLevel)
```

**Parameters**
- `logLevel`: The log level to check.

**Return Value**
- `true` if the log level is enabled; otherwise, `false`.

---
### `Log<TState>`

Writes a log entry.

```csharp
public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
```

**Type Parameters**
- `TState`: The type of the object to be written.

**Parameters**
- `logLevel`: Entry will be written on this level.
- `eventId`: Id of the event.
- `state`: The entry to be written. Can be also an object.
- `exception`: The exception related to this entry.
- `formatter`: Function to create a string message of the state and exception.

---
### `Dispose`

Disposes the logger and releases resources.

```csharp
public void Dispose()
```

## Usage

### Basic Setup

```csharp
var services = new ServiceCollection();

// Add file logging
services.AddLogging(builder => builder.AddFileLogging());

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Log a price change
logger.LogPriceChange(100.50m, 101.25m, "USDT");

// Log a performance metric
logger.LogPerformance("PriceFetch", TimeSpan.FromMilliseconds(125), new { Symbol = "USDT" });
```

### Advanced Usage with Alerts

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFileLogging();
});

var logger = loggerFactory.CreateLogger("Alerts");

// Log a critical alert
logger.LogAlert(LogLevel.Critical, "Price threshold exceeded for BTC/USDT", new { Threshold = 50000m });

// Log a successful database operation
logger.LogDatabaseOperation("InsertOrder", success: true, new { OrderId = 12345 });
```

## Notes

- File logging writes to a rotating log file in the application's working directory. Logs are retained for 7 days by default.
- Thread safety is ensured through internal locking in `FileLogger` and `FileLoggerProvider`. Multiple threads can safely log concurrently.
- Log file paths are constructed using `Path.Combine` with the application's base directory. Ensure the application has write permissions to the target directory.
- The `LogPerformance` method measures time using `Stopwatch` for high precision timing.
- Log levels follow the standard `Microsoft.Extensions.Logging.LogLevel` enum values.
- Disposing the `FileLoggerProvider` will dispose all associated `FileLogger` instances and release file handles.
- Scope support is implemented but does not affect file output formatting. Scopes are primarily for filtering and correlation.

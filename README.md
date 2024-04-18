// entire file content ...
// ... goes in between

## ConsoleOutputWriter

The `ConsoleOutputWriter` class provides a set of methods for writing colored and formatted output to the console. It allows for writing headers, success messages, errors, warnings, and information messages, as well as tables and key-value pairs.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

var consoleOutputWriter = new ConsoleOutputWriter();

consoleOutputWriter.WriteHeader("Header text");
consoleOutputWriter.WriteSuccess("Operation completed successfully");
consoleOutputWriter.WriteError("An error occurred");
consoleOutputWriter.WriteWarning("This is a warning");
consoleOutputWriter.WriteInfo("This is some information");

consoleOutputWriter.WriteSection("Section title");

consoleOutputWriter.WriteKeyValue("Key", "Value");

var rows = new[]
{
    new Dictionary<string, string> { {"Column1", "Value1"}, {"Column2", "Value2"} },
    new Dictionary<string, string> { {"Column1", "Value3"}, {"Column2", "Value4"} }
};

consoleOutputWriter.WriteTable(rows);

consoleOutputWriter.WriteBlankLine();

consoleOutputWriter.WriteRaw("Pre-formatted text");
```

// ... rest of file content ...

## LoggingExtensions

The `LoggingExtensions` class provides extension methods for configuring file-based logging and structured logging throughout the application. It includes methods for logging performance metrics, price changes, alerts, and database operations with appropriate log levels and formatting.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;
using Microsoft.Extensions.Logging;

// Configure file logging with daily rotation
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFileLogging("logs"); // Creates logs/app-YYYY-MM-dd.log
});

var logger = loggerFactory.CreateLogger<Program>();

// Log performance metrics
logger.LogPerformance("Price fetch", TimeSpan.FromMilliseconds(150), isSuccess: true);

// Log price changes
logger.LogPriceChange("USDT", "EUR", 100.50m, 102.75m);

// Log alerts
logger.LogAlert(
    "Price threshold",
    "BTC",
    "USD",
    "Price exceeded maximum threshold",
    new Dictionary<string, string> { ["threshold"] = "50000", ["current"] = "52000" }
);

// Log database operations
logger.LogDatabaseOperation(
    "INSERT",
    "PriceData",
    affectedRows: 1,
    TimeSpan.FromMilliseconds(45)
);
```

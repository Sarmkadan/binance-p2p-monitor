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

## PerformanceMetrics

The `PerformanceMetrics` class tracks and analyzes operation execution metrics including success/failure rates, durations, and timestamps. It provides methods to record operations, retrieve individual or aggregated metrics, generate comprehensive reports, and clear collected data.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

// Create a performance metrics tracker for a specific operation
var metricsTracker = new PerformanceMetrics("PriceFetchOperation");

// Record successful operations
metricsTracker.RecordOperation(TimeSpan.FromMilliseconds(125));
metricsTracker.RecordOperation(TimeSpan.FromMilliseconds(95));

// Record failed operations
metricsTracker.RecordOperation(TimeSpan.FromMilliseconds(85), isSuccess: false);

// Get metrics for the current operation
var currentMetrics = metricsTracker.GetMetrics();
if (currentMetrics != null)
{
    Console.WriteLine($"Total: {currentMetrics.TotalCount}, Success: {currentMetrics.SuccessCount}, " +
                     $"Failure: {currentMetrics.FailureCount}, Success Rate: {currentMetrics.SuccessRate:P1}");
    Console.WriteLine($"Duration - Avg: {currentMetrics.AverageDuration.TotalMilliseconds:F2}ms, " +
                     $"Min: {currentMetrics.MinDuration.TotalMilliseconds:F2}ms, " +
                     $"Max: {currentMetrics.MaxDuration.TotalMilliseconds:F2}ms");
}

// Get all tracked operations
var allMetrics = metricsTracker.GetAllMetrics();
foreach (var kvp in allMetrics)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value.TotalCount} operations");
}

// Generate a comprehensive report
var report = metricsTracker.GenerateReport();
Console.WriteLine(report);

// Clear collected metrics
metricsTracker.Clear();
```

## ArgumentValidationException

`ArgumentValidationException` is thrown when argument validation fails. It contains a dictionary of validation errors mapping parameter names to error messages, and provides constructors for single or multiple errors. The `ToString` method is overridden to include the detailed error information.

### Usage

```csharp
using BinanceP2pMonitor.Exceptions;
using System.Collections.Generic;

// Create a dictionary of validation errors
var errors = new Dictionary<string, string>
{
    ["username"] = "Username cannot be empty",
    ["age"] = "Age must be a positive integer"
};

// Instantiate the exception with multiple errors
var ex = new ArgumentValidationException(
    "One or more arguments are invalid.",
    errors
);

// Access the ValidationErrors property
foreach (var kvp in ex.ValidationErrors)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Output the exception details
Console.WriteLine(ex.ToString());
```


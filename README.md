## UtilityExtensionsTests

The `UtilityExtensionsTests` class provides comprehensive unit tests for various utility extension methods used throughout the application, including date/time manipulation, enumerable processing, numeric calculations, string formatting, and data validation helpers. These tests ensure the reliability and correct behavior of these foundational extension methods.

```csharp
using BinanceP2pMonitor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

// DateTime extension usage
var now = DateTime.UtcNow;
var unixTimestamp = now.ToUnixTimestampMs();
Console.WriteLine($"Unix timestamp (ms): {unixTimestamp}");

var timeAgo = now.AddMinutes(-5).GetTimeAgoString();
Console.WriteLine($"Time ago: {timeAgo}"); // "5m ago"

// Enumerable extension usage
var items = new List<int> { 1, 2, 3, 4, 5 };
var chunks = Enumerable.Chunk(items, 2).ToList();
Console.WriteLine($"Chunks count: {chunks.Count}");

var firstItem = items.FirstOrNull();
Console.WriteLine($"First item: {firstItem}");

// Numeric extension usage
var rounded = 123.456m.RoundTo(2);
Console.WriteLine($"Rounded: {rounded}"); // 123.46

var percentageChange = 110m.CalculatePercentageChange(100m);
Console.WriteLine($"Percentage change: {percentageChange}%"); // 10%

// String extension usage
var truncated = "LongStringExample".Truncate(5);
Console.WriteLine($"Truncated: {truncated}"); // "Lo..."

var snakeCase = "PascalCaseString".ToSnakeCase();
Console.WriteLine($"Snake case: {snakeCase}"); // "pascal_case_string"

// Validation helper usage
var isValidEmail = ValidationHelper.IsValidEmail("test@example.com");
Console.WriteLine($"Valid email: {isValidEmail}");

var isValidTicker = ValidationHelper.IsValidTicker("USDT");
Console.WriteLine($"Valid ticker: {isValidTicker}");
```

## LoggingExtensionsValidation

The `LoggingExtensionsValidation` class provides validation helpers for logging configuration and parameters to ensure correct usage before actual logging occurs. It contains extension methods for validating `ILoggingBuilder`, `ILogger`, and various logging parameters, returning lists of validation problems or boolean validity checks.

```csharp
using BinanceP2pMonitor.Infrastructure;
using Microsoft.Extensions.Logging;
using System;

// Validating an ILoggingBuilder instance
var loggingBuilder = new LoggerFactory().CreateLogger("test").ToString(); // Example - in practice, use actual ILoggingBuilder
var builderValidation = loggingBuilder.Validate(); // Returns IReadOnlyList<string> of problems
bool isBuilderValid = loggingBuilder.IsValid(); // Returns true if valid
loggingBuilder.EnsureValid(); // Throws ArgumentException if invalid

// Validating an ILogger instance
ILogger logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("test");
var loggerValidation = logger.Validate(); // Returns IReadOnlyList<string> of problems
bool isLoggerValid = logger.IsValid(); // Returns true if valid
logger.EnsureValid(); // Throws ArgumentException if invalid

// Validating log path parameters
var logPathValidation = LoggingExtensionsValidation.Validate("logs/app.log"); // Returns IReadOnlyList<string>
bool isLogPathValid = LoggingExtensionsValidation.IsValid("logs/app.log"); // Returns true if valid
LoggingExtensionsValidation.EnsureValid("logs/app.log"); // Throws ArgumentException if invalid

// Validating LogPerformance parameters
var perfValidation = LoggingExtensionsValidation.Validate("DatabaseQuery", TimeSpan.FromSeconds(2.5), true, "key=value");
bool isPerfValid = LoggingExtensionsValidation.IsValid("DatabaseQuery", TimeSpan.FromSeconds(2.5), true, "key=value");
LoggingExtensionsValidation.EnsureValid("DatabaseQuery", TimeSpan.FromSeconds(2.5), true, "key=value");

// Validating LogPriceChange parameters
var priceValidation = LoggingExtensionsValidation.Validate("BTC", "USDT", 45000.50m, 45500.75m);
bool isPriceValid = LoggingExtensionsValidation.IsValid("BTC", "USDT", 45000.50m, 45500.75m);
LoggingExtensionsValidation.EnsureValid("BTC", "USDT", 45000.50m, 45500.75m);

// Validating LogAlert parameters
var alertValidation = LoggingExtensionsValidation.Validate("PriceAlert", "BTC", "USDT", "Price exceeded threshold", 
    new Dictionary<string, string> { { "threshold", "45000" } });
bool isAlertValid = LoggingExtensionsValidation.IsValid("PriceAlert", "BTC", "USDT", "Price exceeded threshold", 
    new Dictionary<string, string> { { "threshold", "45000" } });
LoggingExtensionsValidation.EnsureValid("PriceAlert", "BTC", "USDT", "Price exceeded threshold", 
    new Dictionary<string, string> { { "threshold", "45000" } });

// Validating LogDatabaseOperation parameters
var dbValidation = LoggingExtensionsValidation.Validate("INSERT", "trades", 100, TimeSpan.FromMilliseconds(150));
bool isDbValid = LoggingExtensionsValidation.IsValid("INSERT", "trades", 100, TimeSpan.FromMilliseconds(150));
LoggingExtensionsValidation.EnsureValid("INSERT", "trades", 100, TimeSpan.FromMilliseconds(150));
```

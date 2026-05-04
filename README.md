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

## HttpClientFactory

The `HttpClientFactory` class provides a factory for creating configured HTTP clients with standard headers and policies for making API calls. It simplifies HTTP communication by managing a shared `HttpClient` instance with pre-configured settings such as user-agent, accept headers, and timeout values.

### Usage

```csharp
using BinanceP2pMonitor.Integration;
using Microsoft.Extensions.Logging;

// Create and configure services
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<HttpClientFactory>();
var httpClient = new HttpClient();
var httpClientFactory = new HttpClientFactory(httpClient, logger);

// Create a configured API client for a specific base URL
var apiClient = httpClientFactory.CreateApiClient("https://api.binance.com");

// Make a GET request to retrieve deserialized JSON response
var priceData = await httpClientFactory.GetAsync<PriceData>(
    "/p2p/v1/friendly/loan/config"
);

// Make a POST request with JSON body and get typed response
var requestData = new { symbol = "USDT", fiat = "EUR" };
var response = await httpClientFactory.PostAsync<ApiResponse>(
    "/p2p/v1/friendly/loan/price",
    requestData
);

// Get raw response as string
var rawResponse = await httpClientFactory.GetStringAsync(
    "https://api.binance.com/p2p/v1/friendly/loan/config"
);
```

## IWebhookNotificationClient

The `IWebhookNotificationClient` interface defines methods for sending webhook notifications to external monitoring systems. It provides two main methods: `SendAlertAsync` for generic alerts and `SendPriceAlertAsync` for price-specific notifications. Both methods return a boolean indicating whether the webhook was successfully delivered.

### Usage

```csharp
using BinanceP2pMonitor.Integration;
using Microsoft.Extensions.Logging;

// Create services
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<WebhookNotificationClient>();
var httpClientFactory = new HttpClientFactory(new HttpClient(), logger);
var appSettings = new AppSettings { WebhookUrl = "https://your-webhook-endpoint.com/api/alerts" };

// Create webhook notification client
var webhookClient = new WebhookNotificationClient(httpClientFactory, appSettings, logger);

// Send a generic alert
var alertResult = await webhookClient.SendAlertAsync(new WebhookPayload
{
    Event = "system_alert",
    Asset = "USDT",
    Fiat = "EUR",
    BuyPrice = 100.50m,
    SellPrice = 101.25m,
    AlertReason = "API rate limit exceeded",
    CustomData = "{\"service\": \"binance-p2p-monitor\"}"
});

// Send a price-specific alert (convenience method)
var priceAlertResult = await webhookClient.SendPriceAlertAsync(
    asset: "BTC",
    fiat: "USDT",
    buyPrice: 50000.50m,
    sellPrice: 50010.75m,
    alertReason: "Price threshold exceeded"
);
```

## EventBus

The `EventBus` class implements an in-memory event bus using the publish-subscribe pattern. It allows components to communicate asynchronously through strongly-typed events without direct dependencies. The bus supports both single event publishing and batch operations, with thread-safe subscription management and comprehensive logging.

### Usage

```csharp
using BinanceP2pMonitor.Events;
using Microsoft.Extensions.Logging;

// Create an event bus with logging
var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<EventBus>();
var eventBus = new EventBus(logger);

// Define a custom event implementing IEvent
public record PriceThresholdExceededEvent(
    string Symbol,
    decimal Threshold,
    decimal CurrentPrice) : IEvent;

// Subscribe to events
var subscriptionToken = eventBus.Subscribe<PriceThresholdExceededEvent>(
    async (priceEvent, ct) => {
        Console.WriteLine($"Threshold exceeded: {priceEvent.Symbol} at {priceEvent.CurrentPrice}");
        // Handle the event
        await Task.CompletedTask;
    }
);

// Publish a single event
var priceEvent = new PriceThresholdExceededEvent("USDT", 50000m, 52000m);
await eventBus.PublishAsync(priceEvent);

// Publish multiple events at once
await eventBus.PublishManyAsync(new[] {
    new PriceThresholdExceededEvent("BTC", 45000m, 46500m),
    new PriceThresholdExceededEvent("ETH", 3000m, 3200m)
});

// Unsubscribe when no longer needed
eventBus.Unsubscribe<PriceThresholdExceededEvent>(subscriptionToken);
```

## PriceUpdatedEvent

The `PriceUpdatedEvent` class represents an event that is fired whenever a price update occurs for a specific trading pair. It contains both current and previous price values, along with offer counts, allowing subscribers to track price changes and react to market movements.

### Usage

```csharp
using BinanceP2pMonitor.Events;

// Create a price updated event for USDT/BTC pair
var priceEvent = new PriceUpdatedEvent
{
    Asset = "BTC",
    Fiat = "USDT",
    BuyPrice = 50000.50m,
    SellPrice = 50010.75m,
    PreviousBuyPrice = 49990.25m,
    PreviousSellPrice = 50005.50m,
    BuyOfferCount = 15,
    SellOfferCount = 20
};

// Access event properties
Console.WriteLine($"Asset: {priceEvent.Asset}");
Console.WriteLine($"Fiat: {priceEvent.Fiat}");
Console.WriteLine($"Buy Price: {priceEvent.BuyPrice}");
Console.WriteLine($"Sell Price: {priceEvent.SellPrice}");
Console.WriteLine($"Previous Buy Price: {priceEvent.PreviousBuyPrice}");
Console.WriteLine($"Previous Sell Price: {priceEvent.PreviousSellPrice}");
Console.WriteLine($"Buy Offer Count: {priceEvent.BuyOfferCount}");
Console.WriteLine($"Sell Offer Count: {priceEvent.SellOfferCount}");
```

## SerializationException

`SerializationException` is thrown when serialization or deserialization fails. It includes a `DataType` property to identify the type being serialized and provides an overridden `ToString()` method that includes this information.

### Usage

```csharp
using BinanceP2pMonitor.Exceptions;

// Create a serialization exception for a specific data type
var ex = new SerializationException(
    "Failed to serialize user data",
    dataType: "User"
);

// Access the DataType property
Console.WriteLine($"DataType: {ex.DataType}");

// Output the exception details
Console.WriteLine(ex.ToString());
```

## BinanceP2pException

`BinanceP2pException` is the base exception class for all application-specific errors. It provides properties for error code and context, as well as constructors for creating exceptions with or without inner exceptions. The `ToString` method is overridden to include the error code and context information.

### Usage

```csharp
using BinanceP2pMonitor.Exceptions;

// Create a BinanceP2pException with error code and context
var ex = new BinanceP2pException(
    "An error occurred.",
    errorCode: "ERROR_001",
    context: new Dictionary<string, object> { ["key"] = "value" }
);

// Access the ErrorCode and Context properties
Console.WriteLine($"ErrorCode: {ex.ErrorCode}");
Console.WriteLine($"Context: {string.Join(", ", ex.Context.Select(kv => $"{kv.Key}={kv.Value}"))}");

// Output the exception details
Console.WriteLine(ex.ToString());
```


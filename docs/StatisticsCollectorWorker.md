# StatisticsCollectorWorker

Background worker for collecting and aggregating price statistics.

## Overview

The `StatisticsCollectorWorker` class is a background service that periodically collects and aggregates price statistics for monitored assets and fiats. It runs on a 5-minute interval and handles errors gracefully with exponential backoff.

## Responsibilities

- Collects price statistics every 5 minutes
- Processes all combinations of monitored assets and fiats
- Handles exceptions with appropriate logging and retry delays
- Uses dependency injection for services and configuration
- Implements graceful shutdown via cancellation tokens

## Implementation Details

### Constructor

```csharp
public StatisticsCollectorWorker(
    IServiceProvider serviceProvider,
    ILogger<StatisticsCollectorWorker> logger,
    AppSettings appSettings)
```

Parameters:
- `serviceProvider`: Service provider for creating scoped services
- `logger`: Logger instance for logging activities
- `appSettings`: Application settings containing monitored assets and fiats

### ExecuteAsync Method

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
```

Process:
1. **Startup Logging**: Logs that the worker has started
2. **Main Loop**: Continues until cancellation is requested
   - **Statistics Collection**: Calls `CollectStatisticsAsync` to gather and process data
   - **Delay**: Waits 5 minutes between collection cycles
   - **Error Handling**:
     - `OperationCanceledException`: Logs shutdown and breaks loop
     - General exceptions: Logs error and waits 30 seconds before retrying

### CollectStatisticsAsync Method

```csharp
private async Task CollectStatisticsAsync(CancellationToken ct)
```

Process:
1. **Scope Creation**: Creates a service scope for repository resolution
2. **Repository Resolution**: Gets `IPriceRepository` and `IHistoryRepository` instances
3. **Configuration Parsing**: Splits monitored assets and fiats from configuration
4. **Data Collection**:
   - Iterates through all asset/fiat combinations
   - For each combination, fetches last 24 hours of prices (placeholder logic)
   - Logs debug information for each processed pair
   - Handles individual pair errors without stopping the entire process

## Error Handling

The worker implements resilient error handling:
- Collection errors are logged as warnings but don't stop the worker
- General exceptions in the main loop trigger a 30-second delay before retrying
- Cancellation tokens are respected for graceful shutdown
- Individual asset/fiat pair failures don't affect other pairs

## Logging

Uses `ILogger<StatisticsCollectorWorker>` for structured logging:
- Information level: Worker start/stop events
- Debug level: Statistics collection progress for each asset/fiat pair
- Warning level: Individual pair collection errors
- Error level: General collection cycle failures

## Usage

This worker is registered as a hosted service in the application's dependency injection container and runs automatically when the application starts.

## Related Components

- `IPriceRepository`: Interface for accessing current price data
- `IHistoryRepository`: Interface for accessing historical price data
- `AppSettings`: Configuration containing monitored assets and fiats
- `BackgroundService`: Base class implementing the worker pattern

## Conventions

- Follows the .NET BackgroundService pattern for long-running tasks
- Uses dependency injection for all external dependencies
- Respects cancellation tokens for graceful shutdown
- Separates concerns with focused methods for collection logic
- Uses configurable intervals (hardcoded to 5 minutes in this implementation)
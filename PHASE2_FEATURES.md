# Phase 2: Features & Infrastructure

## Overview

Phase 2 adds comprehensive command-line interface, middleware pipeline, utilities, formatters, integration modules, caching layer, event system, and background workers to complete the application infrastructure.

**Total New Files: 43**
**Total Lines of Code: 3,500+**

## Architecture Components

### 1. CLI Interface (4 files, 300+ lines)

The command-line interface provides a complete argument parsing and command execution framework.

- **CommandContext.cs**: Execution context containing parsed arguments, options, flags, and service provider
- **ICommand.cs**: Interface defining command contract with validation and execution
- **CommandParser.cs**: Parses command-line arguments into structured context with intelligent option/flag detection
- **CommandFactory.cs**: Factory for registering and creating command instances

**Key Features:**
- Full argument and option parsing with `--option=value` and `-f value` syntax
- Command validation pipeline
- Service provider integration for dependency injection
- Extensible command registration system

### 2. Middleware & Pipeline (3 files, 200+ lines)

Middleware components process commands through a request pipeline.

- **LoggingMiddleware.cs**: Logs command execution with timing and result tracking
- **ExceptionHandlingMiddleware.cs**: Centralized exception handling with specialized error messages
- **ValidationMiddleware.cs**: Validates command arguments before execution

**Key Features:**
- Structured exception handling for different error types
- Performance monitoring with execution timing
- Pre-execution validation with detailed error reporting
- Support for verbose logging and stack traces

### 3. Utility Classes (6 files, 400+ lines)

Production-ready extension methods and utility classes.

- **EnumerableExtensions.cs**: Chunk, batch, distinct, and aggregation operations
- **DateTimeExtensions.cs**: Unix timestamps, time ranges (day/week/month), and human-readable formatting
- **StringExtensions.cs**: Case conversion, truncation, masking, and parsing utilities
- **NumericExtensions.cs**: Percentage calculations, clamping, currency formatting
- **ValidationException.cs**: Custom exception for validation errors
- **NumericExtensions.cs**: Additional numeric operations and conversions

**Coverage:**
- LINQ-style operations (Chunk, DistinctBy, ForEach)
- Date/time calculations and formatting
- String manipulation (camelCase→snake_case, masking sensitive data)
- Numeric operations (percentage change, bounds checking, formatting)

### 4. Output Formatters (4 files, 350+ lines)

Multiple output format support for data presentation.

- **IOutputFormatter.cs**: Interface for output formatters
- **JsonOutputFormatter.cs**: JSON formatting with pretty-printing
- **TableOutputFormatter.cs**: ASCII table with auto-width columns
- **CsvOutputFormatter.cs**: CSV export with proper escaping

**Formats Supported:**
- JSON (indented, structured)
- CSV (RFC 4180 compliant with proper escaping)
- ASCII Table (formatted with borders and padding)

### 5. Caching Layer (2 files, 250+ lines)

In-memory caching with automatic expiration and cleanup.

- **ICache.cs**: Cache interface with async/await support
- **MemoryCache.cs**: Thread-safe in-memory cache with TTL support

**Features:**
- Get/Set/Remove operations
- Automatic expiration cleanup (5-minute interval)
- GetOrCreate factory pattern
- Thread-safe with ReaderWriterLockSlim

### 6. Event System (3 files, 200+ lines)

Domain event pub-sub pattern for loosely coupled components.

- **IEvent.cs**: Event interface and base class
- **IEventPublisher.cs**: Publisher and subscriber interfaces
- **EventBus.cs**: In-memory pub-sub event bus
- **PriceUpdatedEvent.cs**: Domain events (PriceUpdated, SpreadAlert, AlertSent)

**Features:**
- Type-safe event publishing
- Async event handlers
- Exception isolation between handlers
- Event metadata tracking (ID, timestamp, type)

### 7. Integration Modules (2 files, 280+ lines)

External service integration wrappers.

- **HttpClientFactory.cs**: Configured HTTP client factory with retry support
- **TelegramNotificationClient.cs**: Telegram bot integration for alerts

**Features:**
- Automatic header injection (User-Agent, Accept)
- GET/POST with JSON serialization
- Telegram message sending with HTML parsing
- Rate limiting support for message sending
- Test message functionality

### 8. Background Workers (2 files, 200+ lines)

Long-running background tasks for maintenance and collection.

- **StatisticsCollectorWorker.cs**: Collects price statistics every 5 minutes
- **DatabaseCleanupWorker.cs**: Removes old records based on retention policy

**Features:**
- Graceful cancellation support
- Error recovery with exponential backoff
- Configurable intervals
- Scope-based dependency injection

### 9. Infrastructure Utilities (11 files, 1,200+ lines)

Core infrastructure components and utilities.

- **RateLimiter.cs**: Token bucket rate limiting algorithm
- **ConsoleOutputWriter.cs**: Colored console output with formatting
- **CachedPriceMonitoringService.cs**: Decorator adding caching to monitoring service
- **ApiResponse.cs**: Standard API response wrapper
- **LoggingExtensions.cs**: Structured logging helpers and file logging
- **RetryPolicy.cs**: Retry logic with exponential backoff
- **ConfigurationValidator.cs**: Validates all configuration settings
- **DataExporter.cs**: Exports data to JSON, CSV, and generates reports
- **PerformanceMetrics.cs**: Performance tracking and reporting

**Features:**
- Token bucket rate limiting
- Rich console output with colors and symbols
- Service decoration for transparent caching
- Standardized error response format
- File logging with daily rotation
- Configurable retry strategies
- Comprehensive configuration validation
- Statistical data export and aggregation
- Performance metrics collection and reporting

### 10. Commands (7 files, 700+ lines)

CLI command implementations with full argument validation.

- **MonitorCommand.cs**: Start real-time monitoring with event subscription
- **StatusCommand.cs**: Display current prices and system status
- **HelpCommand.cs**: Interactive help with command listing
- **AlertCommand.cs**: Manage price alerts (list, create, delete, test)
- **HistoryCommand.cs**: Query and analyze historical data
- **ExportCommand.cs**: Export data to CSV/JSON files
- **VersionCommand.cs**: Display version and build information

**Features:**
- Comprehensive argument validation
- Detailed help text with examples
- Table/JSON/CSV output options
- Event-driven monitoring
- Alert management CRUD operations
- Historical data analysis with statistics
- Multi-format data export

### 11. Exception Handling (1 file, 10+ lines)

- **ConfigurationException.cs**: Specialized exception for configuration errors

## Design Patterns

### Middleware Pipeline
Implements ASP.NET Core-style middleware pipeline for command processing:
```
CommandParser → LoggingMiddleware → ValidationMiddleware 
  → ExceptionHandlingMiddleware → Command Execution
```

### Decorator Pattern
- `CachedPriceMonitoringService` decorates `IPriceMonitoringService` with transparent caching
- Maintains interface compatibility while adding functionality

### Factory Pattern
- `CommandFactory` registers and creates commands by name
- `HttpClientFactory` creates configured HTTP clients

### Observer Pattern
- `EventBus` implements pub-sub for loosely coupled event handling
- Multiple handlers can subscribe to same event type

### Repository Pattern
- Existing `IPriceRepository`, `IAlertRepository` etc. maintained
- New repositories follow same pattern

## Configuration

All new infrastructure components are registered in `Program.cs`:

```csharp
// Register caching
services.AddSingleton<ICache, MemoryCache>();

// Register event bus
services.AddSingleton<IEventBus, EventBus>();

// Register CLI infrastructure
services.AddSingleton<CommandParser>();
services.AddSingleton<CommandFactory>();
services.AddSingleton<ConsoleOutputWriter>();

// Register HTTP client and integration services
services.AddHttpClient();
services.AddSingleton<HttpClientFactory>();
services.AddSingleton<TelegramNotificationClient>();

// And more...
```

## Usage Examples

### Monitor Command
```bash
binance-p2p-monitor monitor --asset=BTC --fiat=USDT --interval=30
```

### Check Status
```bash
binance-p2p-monitor status --format=json
```

### View History
```bash
binance-p2p-monitor history --asset=ETH --days=30 --stats
```

### Export Data
```bash
binance-p2p-monitor export --output=prices.csv --format=csv --days=7
```

### Manage Alerts
```bash
binance-p2p-monitor alert create --asset=BTC --fiat=USDT --type=spread
binance-p2p-monitor alert list
binance-p2p-monitor alert test
```

## Extension Points

1. **New Commands**: Implement `ICommand` and register in `Program.cs`
2. **New Formatters**: Implement `IOutputFormatter`
3. **New Middleware**: Create middleware class and add to pipeline
4. **Event Handlers**: Subscribe to `IEventBus` in services
5. **Custom Cache**: Implement `ICache` interface

## Testing Considerations

- `RateLimiter` thread-safe under concurrent access
- `MemoryCache` includes expiration cleanup
- `EventBus` handles exceptions per handler
- `ConfigurationValidator` validates all critical settings
- `RetryPolicy` with configurable predicates for selective retry

## Performance Optimizations

- In-memory caching with TTL expiration
- Rate limiting prevents API throttling
- Batch operations support for bulk imports
- Performance metrics for profiling
- Lazy initialization of expensive resources

## Security

- Sensitive string masking (API keys, tokens)
- Input validation at all boundaries
- Exception details hidden from console output
- Configuration validation prevents invalid states
- Rate limiting protects against abuse

## Future Enhancements

- Redis-backed distributed caching
- Database-backed event store
- Advanced statistics with ML predictions
- Web dashboard interface
- Webhook notification support
- Multi-currency portfolio tracking

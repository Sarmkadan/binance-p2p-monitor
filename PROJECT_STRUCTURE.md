# Project Structure

Comprehensive guide to the binance-p2p-monitor codebase organization and conventions.

## Directory Layout

```
binance-p2p-monitor/
├── .github/
│   └── workflows/
│       └── build.yml                    # CI/CD pipeline (GitHub Actions)
├── docs/
│   ├── getting-started.md               # Installation & first run guide
│   ├── architecture.md                  # System design & patterns
│   ├── api-reference.md                 # Complete API documentation
│   ├── deployment.md                    # Production deployment
│   └── faq.md                          # Frequently asked questions
├── examples/
│   ├── 01-real-time-monitor.cs         # WebSocket price monitoring
│   ├── 02-telegram-alerts.cs           # Alert configuration & dispatch
│   ├── 03-spread-analyzer.cs           # Bid/ask spread analysis
│   ├── 04-price-exporter.cs            # Export to CSV/JSON
│   ├── 05-auto-trader-integration.cs   # Trading bot integration
│   └── 06-historical-analysis.cs       # Time-series analysis
├── src/
│   ├── CLI/                            # Command-line interface
│   │   ├── CommandContext.cs
│   │   ├── CommandFactory.cs
│   │   ├── CommandParser.cs
│   │   └── ICommand.cs
│   ├── Caching/                        # In-memory caching layer
│   │   ├── ICache.cs
│   │   └── MemoryCache.cs
│   ├── Commands/                       # Command implementations
│   │   ├── MonitorCommand.cs
│   │   ├── AlertCommand.cs
│   │   ├── HistoryCommand.cs
│   │   ├── ExportCommand.cs
│   │   ├── StatusCommand.cs
│   │   ├── HelpCommand.cs
│   │   └── VersionCommand.cs
│   ├── Configuration/                  # Settings & validation
│   │   └── AppSettings.cs
│   ├── Constants/                      # Enumeration constants
│   │   ├── AlertCondition.cs
│   │   ├── AlertType.cs
│   │   ├── ApplicationConstants.cs
│   │   └── TradeType.cs
│   ├── Data/                           # Database initialization
│   │   └── DatabaseContext.cs
│   ├── Events/                         # Event-driven architecture
│   │   ├── EventBus.cs
│   │   ├── IEvent.cs
│   │   ├── IEventPublisher.cs
│   │   └── PriceUpdatedEvent.cs
│   ├── Exceptions/                     # Custom exceptions
│   │   └── BinanceP2pException.cs
│   ├── Formatters/                     # Output formatting
│   │   ├── IOutputFormatter.cs
│   │   ├── JsonOutputFormatter.cs
│   │   ├── TableOutputFormatter.cs
│   │   └── CsvOutputFormatter.cs
│   ├── Infrastructure/                 # Cross-cutting concerns
│   │   ├── ApiResponse.cs
│   │   ├── CachedPriceMonitoringService.cs
│   │   ├── ConfigurationValidator.cs
│   │   ├── ConsoleOutputWriter.cs
│   │   ├── DataExporter.cs
│   │   ├── LoggingExtensions.cs
│   │   ├── PerformanceMetrics.cs
│   │   ├── RateLimiter.cs
│   │   └── RetryPolicy.cs
│   ├── Integration/                    # External service integration
│   │   ├── HttpClientFactory.cs
│   │   └── TelegramNotificationClient.cs
│   ├── Middleware/                     # Request/response middleware
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   ├── LoggingMiddleware.cs
│   │   └── ValidationMiddleware.cs
│   ├── Models/                         # Domain data models
│   │   ├── Currency.cs
│   │   ├── Market.cs
│   │   ├── Price.cs
│   │   ├── PriceAlert.cs
│   │   ├── PriceHistory.cs
│   │   ├── Spread.cs
│   │   ├── TradeOffer.cs
│   │   └── UserProfile.cs
│   ├── Repositories/                   # Data access patterns
│   │   ├── IPriceRepository.cs
│   │   ├── PriceRepository.cs
│   │   ├── IAlertRepository.cs
│   │   ├── AlertRepository.cs
│   │   ├── IHistoryRepository.cs
│   │   ├── HistoryRepository.cs
│   │   ├── ITradeOfferRepository.cs
│   │   └── TradeOfferRepository.cs
│   ├── Services/                       # Business logic layer
│   │   ├── IPriceMonitoringService.cs
│   │   ├── PriceMonitoringService.cs
│   │   ├── IAlertService.cs
│   │   ├── AlertService.cs
│   │   ├── ISpreadAnalysisService.cs
│   │   ├── SpreadAnalysisService.cs
│   │   ├── IPriceHistoryService.cs
│   │   ├── PriceHistoryService.cs
│   │   ├── IWebSocketService.cs
│   │   ├── WebSocketService.cs
│   │   └── MonitoringHostedService.cs
│   ├── Utilities/                      # Helper functions
│   │   ├── DateTimeExtensions.cs
│   │   ├── EnumerableExtensions.cs
│   │   ├── FormatHelper.cs
│   │   ├── NumericExtensions.cs
│   │   ├── PriceCalculator.cs
│   │   ├── StringExtensions.cs
│   │   ├── ValidationHelper.cs
│   │   └── ValidationException.cs
│   ├── Workers/                        # Background workers
│   │   ├── DatabaseCleanupWorker.cs
│   │   └── StatisticsCollectorWorker.cs
│   ├── GlobalUsings.cs                 # Global using statements
│   └── Program.cs                      # Application entry point
├── .editorconfig                       # Code style conventions
├── .gitignore                          # Git ignore rules
├── appsettings.json                    # Configuration template
├── appsettings.development.json        # Development overrides
├── appsettings.example.json            # Configuration example
├── binance-p2p-monitor.csproj          # Project file
├── build.sh                            # Unix build script
├── build.bat                           # Windows build script
├── CHANGELOG.md                        # Version history
├── CONTRIBUTING.md                     # Contribution guidelines
├── Dockerfile                          # Docker image definition
├── docker-compose.yml                  # Docker Compose config
├── Makefile                            # Build automation
├── PROJECT_STRUCTURE.md                # This file
├── README.md                           # Main documentation
└── LICENSE                             # MIT License

```

## Code Organization Principles

### 1. Layered Architecture

```
┌─────────────────────────────┐
│  CLI / Commands             │ Presentation Layer
├─────────────────────────────┤
│  Services & Business Logic  │ Application Layer
├─────────────────────────────┤
│  Repositories & Data Access │ Persistence Layer
├─────────────────────────────┤
│  Database (SQLite)          │ Storage Layer
└─────────────────────────────┘
```

**Benefits:**
- Clear separation of concerns
- Easy to test (mock repositories)
- Database-agnostic business logic
- Extensible (add new services without changing UI)

### 2. Dependency Injection

All services registered in `Program.cs` constructor:

```csharp
// In Program.cs
services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
services.AddScoped<IAlertService, AlertService>();
services.AddSingleton<IEventBus, EventBus>();
```

**Benefits:**
- Loose coupling
- Easy testing (inject mocks)
- Lifecycle management

### 3. Repository Pattern

Database access abstracted through repositories:

```csharp
public interface IPriceRepository
{
    Task<Price> GetByIdAsync(int id);
    Task<IEnumerable<Price>> GetRecentAsync(string asset, string fiat, int limit);
    Task AddAsync(Price price);
}
```

**Benefits:**
- Centralized query logic
- Easy to swap implementations
- Testable data access

### 4. Event-Driven Communication

Components communicate through events:

```csharp
eventBus.Subscribe<PriceUpdatedEvent>(async @event =>
{
    await alertService.EvaluateAlertsAsync(@event.Price);
});
```

**Benefits:**
- Loose coupling
- Non-blocking I/O
- Easy to add subscribers

## File Naming Conventions

### Classes
- Service classes: `{Feature}Service.cs`
- Repository classes: `{Entity}Repository.cs`
- Command classes: `{Action}Command.cs`
- Models: `{EntityName}.cs`
- Exceptions: `{Type}Exception.cs`
- Formatters: `{Format}OutputFormatter.cs`

### Interfaces
- Always prefix with `I`: `I{ClassName}.cs`
- Service interfaces: `I{Feature}Service.cs`
- Repository interfaces: `I{Entity}Repository.cs`

### Examples
```
Good:                          Not recommended:
├── PriceMonitoringService.cs   ├── PriceMonitor.cs
├── IPriceMonitoringService.cs  ├── priceMonitoringService.cs
├── AlertCommand.cs             ├── alert_command.cs
├── AlertRepository.cs          ├── repository_alert.cs
├── PriceUpdatedEvent.cs        ├── Event_PriceUpdated.cs
└── IOutputFormatter.cs         └── OutputFormatter.cs
```

## Namespace Organization

```csharp
// Organized by feature/layer
BinanceP2pMonitor                       // Root
├── BinanceP2pMonitor.CLI               // Commands
├── BinanceP2pMonitor.Services          // Business logic
├── BinanceP2pMonitor.Repositories      // Data access
├── BinanceP2pMonitor.Models            // Domain models
├── BinanceP2pMonitor.Events            // Event definitions
├── BinanceP2pMonitor.Infrastructure    // Cross-cutting
├── BinanceP2pMonitor.Integration       // External services
├── BinanceP2pMonitor.Utilities         // Helpers
├── BinanceP2pMonitor.Constants         // Enums/Constants
├── BinanceP2pMonitor.Formatters        // Output formatting
└── BinanceP2pMonitor.Middleware        // Middleware
```

## Code Size Guidelines

| Component | Max Lines | Reason |
|-----------|-----------|--------|
| Class | 300 | Cohesion |
| Method | 30 | Readability |
| File | 200 | Navigation |
| Constructor | 20 | Clarity |
| Interface | 15 | Simplicity |

## Dependency Rules

**Allowed:**
```
CLI → Commands → Services → Repositories → Database
             ↓
        Infrastructure
             ↓
         Models, Constants
```

**NOT Allowed:**
```
Database → Repositories (don't call services)
Commands ← Services (services don't know about CLI)
UI ← Infrastructure (infrastructure is generic)
```

## Testing Structure

### Unit Tests
- Test public methods only
- Mock external dependencies
- Test error conditions
- Location: `{FeatureArea}Tests.cs` in test project

```csharp
[TestClass]
public class AlertServiceTests
{
    private Mock<IAlertRepository> _mockRepository;
    private AlertService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IAlertRepository>();
        _service = new AlertService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task CreateAlert_WithValidInput_ReturnsAlert()
    {
        // Arrange
        var alert = new PriceAlert { /* ... */ };

        // Act
        var result = await _service.CreateAlertAsync(alert);

        // Assert
        Assert.IsNotNull(result);
    }
}
```

### Integration Tests
- Use real SQLite database
- Test full workflows
- Clean up test data
- Location: `{Feature}IntegrationTests.cs`

## Documentation Standards

### Method Documentation

```csharp
/// <summary>
/// Evaluates all alerts against current price.
/// </summary>
/// <param name="price">Current price data from API</param>
/// <returns>Collection of triggered alerts</returns>
/// <exception cref="InvalidOperationException">If evaluation fails</exception>
public async Task<IEnumerable<PriceAlert>> EvaluateAlertsAsync(Price price)
{
    // Implementation
}
```

### Complex Logic Comments

```csharp
// WebSocket uses exponential backoff: 1s, 2s, 4s, 8s, max 60s
// This prevents overwhelming the API during brief outages
var backoffMs = Math.Min(Math.Pow(2, retryCount) * 1000, 60000);
```

### File Headers

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Services;
```

## Configuration Files

### appsettings.json
- **Use for:** Default production settings
- **Examples:** Database connection, monitoring interval
- **Check in:** Yes (with defaults, no secrets)

### appsettings.development.json
- **Use for:** Local development overrides
- **Examples:** Debug logging, localhost URLs
- **Check in:** Yes

### appsettings.local.json
- **Use for:** Local secrets and API keys
- **Examples:** Telegram token, Binance API keys
- **Check in:** NO (.gitignore)

### .env files
- **Use for:** Docker/production environment variables
- **Examples:** Database URL, API keys
- **Check in:** NO (.gitignore)

## Build & Deployment

### Build Process
1. Restore NuGet packages
2. Compile C# code
3. Run unit tests
4. Run code quality checks
5. Publish binaries

### Deployment Outputs
- **Debug:** `./bin/Debug/net10.0/` (development)
- **Release:** `./bin/Release/net10.0/` (production)
- **Published:** `./publish/` (self-contained)
- **Docker:** `binance-p2p-monitor:latest`

## Performance Considerations

### Hot Paths
- `PriceMonitoringService.GetPriceAsync()` — Called every 30 seconds
- `AlertService.EvaluateAlertsAsync()` — Called on every price update
- `PriceRepository.GetRecentAsync()` — Called frequently for UI

### Optimization
- Cache frequently accessed data (TTL-based)
- Index database columns by asset/fiat/timestamp
- Use async/await for I/O operations
- Batch API calls when possible

## Security Considerations

### Secrets Management
- **Never hardcode:** API keys, passwords, tokens
- **Use:** Environment variables, configuration files (.gitignore)
- **Rotate:** Compromised credentials immediately
- **Log:** Never log sensitive data (sanitize first)

### Input Validation
- **CLI arguments:** Validate length, format, whitelist values
- **API responses:** Validate data types and ranges
- **Database:** Use parameterized queries
- **User input:** Sanitize before storage

## Common Tasks

### Adding a New Command
1. Create `{Action}Command.cs` in `Commands/`
2. Implement `ICommand` interface
3. Register in `CommandFactory.cs`
4. Add help text to `HelpCommand.cs`

### Adding a New Service
1. Create interface `I{Service}Service.cs` in `Services/`
2. Create implementation `{Service}Service.cs`
3. Register in `Program.cs` DI container
4. Add tests

### Adding a New Model
1. Create class in `Models/`
2. Add file header comment
3. Create repository if data persistence needed
4. Update `DatabaseContext.cs` schema

### Adding a New Alert Type
1. Add to `AlertType.cs` enum
2. Create handler in `AlertService.cs`
3. Add validation in `AlertCommand.cs`
4. Update documentation

## Resources

- **Architecture Details:** See `docs/architecture.md`
- **API Reference:** See `docs/api-reference.md`
- **Deployment Guide:** See `docs/deployment.md`
- **Examples:** See `examples/` directory
- **Build Commands:** See `Makefile`

## Quick Reference

| Task | Command |
|------|---------|
| Build | `dotnet build -c Release` |
| Test | `dotnet test -c Release` |
| Format | `dotnet format` |
| Run | `dotnet run` |
| Publish | `dotnet publish -c Release -r linux-x64` |
| Docker | `docker build -t binance-p2p-monitor .` |
| Clean | `dotnet clean` |

## Contributing

See `CONTRIBUTING.md` for guidelines on:
- Code style
- Testing requirements
- Commit messages
- Pull request process

# Contributing to binance-p2p-monitor

Thank you for your interest in contributing! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## How to Contribute

### 1. Reporting Bugs

**Before submitting a bug report, please check:**
- Search existing issues to avoid duplicates
- Check the FAQ in `docs/faq.md`
- Verify the issue reproduces on the latest version

**When submitting a bug report, include:**
- Descriptive title (e.g., "WebSocket reconnection fails with 401 error")
- Environment: OS, .NET version, application version
- Steps to reproduce
- Expected vs. actual behavior
- Logs/error output (sanitize sensitive data)
- Configuration (with secrets removed)

**Example:**
```
Title: WebSocket disconnects after 5 minutes

Environment:
- OS: Ubuntu 22.04
- .NET: 10.0
- App version: 1.2.0

Steps:
1. Start monitoring with default config
2. Wait 5 minutes
3. Check logs

Expected: Continuous monitoring
Actual: WebSocket disconnects, reconnection fails

Error: [ERROR] WebSocket connection timeout after 300s
```

### 2. Suggesting Features

**Before suggesting, check:**
- Existing feature requests on GitHub Discussions
- Planned features in CHANGELOG.md (Roadmap section)

**When suggesting, provide:**
- Clear use case and motivation
- Example workflow
- Why this feature is important

**Example:**
```
Feature: Slack integration for alerts

Use Case:
As a trader using Slack for communication, I want alerts sent to my Slack channel 
so that I don't miss important price movements.

Workflow:
1. Configure Slack webhook URL
2. Create alert as normal
3. Alert triggers → Slack notification sent

Why: Most teams already use Slack; email/Telegram integration is too limited.
```

### 3. Submitting Pull Requests

**Preparation:**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/descriptive-name`
3. Make changes with clear commits
4. Test thoroughly
5. Run code quality checks: `make fmt lint test`

**Before opening PR:**
```bash
# Format code
dotnet format

# Run tests
dotnet test -c Release

# Build release
dotnet build -c Release

# Check no breaking changes to public APIs
```

**PR Template:**
```markdown
## Description
Brief description of changes

## Motivation & Context
Why is this change needed? Fixes #123

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] Feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] All tests passing

## Checklist
- [ ] Code follows style guidelines (`dotnet format`)
- [ ] No new warnings generated
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] No sensitive data committed
```

### 4. Code Style & Standards

**Follow these conventions:**

```csharp
// File header (required)
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

// Namespace with file-scoped namespace
namespace BinanceP2pMonitor.Features;

// Class with documentation
public class ExampleService
{
    // Private fields with underscore
    private readonly ILogger<ExampleService> _logger;
    
    // Public properties with PascalCase
    public string Name { get; set; }
    
    // Constants with UPPER_CASE
    private const int DefaultTimeout = 30;
    
    // Methods with clear naming
    public async Task<Result> ProcessAsync(string input)
    {
        // Use guards early
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input cannot be empty");
        
        // Use var for readability
        var result = await CallApiAsync(input);
        
        // Keep methods <30 lines
        return result;
    }
}
```

**Naming Conventions:**
- Classes: `PascalCase` (e.g., `PriceMonitoringService`)
- Methods: `PascalCase` (e.g., `GetPricesAsync`)
- Parameters: `camelCase` (e.g., `assetSymbol`)
- Private fields: `_camelCase` (e.g., `_priceCache`)
- Constants: `PascalCase` (e.g., `DefaultTimeout`)
- Interfaces: Start with `I` (e.g., `IAlertService`)

**Code Quality:**
- Enable nullable reference types: `#nullable enable`
- Use pattern matching for validation
- Avoid `null!` force-dereference
- Use `async/await` consistently
- Keep methods under 30 lines
- Avoid deep nesting (max 3 levels)

### 5. Commit Message Guidelines

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation only
- `style:` Code style (formatting, missing semicolons)
- `refactor:` Code refactoring without feature/bug changes
- `perf:` Performance improvements
- `test:` Adding/updating tests
- `chore:` Build, dependencies, CI/CD

**Examples:**
```
feat(alerts): add spread anomaly detection

Implement automatic spread analysis and alert triggering when
spread exceeds configured threshold. Uses EventBus for loose
coupling with other services.

Fixes #45
```

```
fix(websocket): handle reconnection timeout gracefully

Increase reconnection timeout from 5s to 30s with exponential
backoff. Prevents tight reconnection loop under poor network
conditions.

Fixes #89
```

### 6. Testing Requirements

**Unit Tests:**
- Test public methods
- Test error conditions
- Use mocks for dependencies
- Achieve >80% code coverage

```csharp
[TestClass]
public class AlertServiceTests
{
    [TestMethod]
    public async Task EvaluateAlerts_WithPriceIncrease_TriggersAlert()
    {
        // Arrange
        var mockRepository = new Mock<IAlertRepository>();
        var service = new AlertService(mockRepository.Object);
        var alert = new PriceAlert { Threshold = 5.0m };
        var price = new Price { Bid = 105.0m };
        
        // Act
        var result = await service.EvaluateAlertsAsync(price);
        
        // Assert
        Assert.IsNotNull(result);
    }
}
```

**Integration Tests:**
- Use real SQLite database
- Test full command flow
- Clean up test data

### 7. Documentation Updates

Update these files when needed:
- `README.md` — For major features
- `docs/api-reference.md` — For API changes
- `docs/deployment.md` — For infrastructure changes
- `CHANGELOG.md` — For all changes
- Code comments — For complex logic only

### 8. Performance Considerations

When contributing performance-critical code:

1. **Benchmark existing implementation:**
   ```csharp
   var sw = Stopwatch.StartNew();
   for (int i = 0; i < 10000; i++)
       await ProcessAsync();
   sw.Stop();
   Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
   ```

2. **Test memory impact:**
   ```bash
   dotnet publish -c Release -r linux-x64 --self-contained
   /usr/bin/time -v ./bin/Release/net10.0/linux-x64/publish/binance-p2p-monitor
   ```

3. **Profile with dotnet-trace:**
   ```bash
   dotnet-trace collect -- dotnet run
   ```

### 9. Security & Privacy

**Never commit:**
- API keys or secrets
- Private credentials
- Sensitive configuration
- Personal information
- Passwords or tokens

**If accidentally committed:**
1. Rotate the compromised credential
2. Use BFG Repo-Cleaner to remove from history
3. Notify maintainers

### 10. Documentation Style

**Write documentation that is:**
- Clear and concise
- Includes examples
- Links to related docs
- Explains the "why", not just "how"

**Example:**
```markdown
## Alert Cooldown

Cooldown prevents alert fatigue by enforcing a minimum interval between
repeated alerts for the same condition.

**Why:** Without cooldown, a price fluctuating around your threshold
would trigger hundreds of notifications per minute.

**Default:** 5 minutes (configurable)

**Example:**
```csharp
var alert = new PriceAlert
{
    CooldownMinutes = 15,  // Won't trigger more than every 15 minutes
};
```
```

## Development Setup

### Prerequisites
- .NET 10 SDK
- Git
- Editor (VS Code, Rider, Visual Studio)
- Docker (optional, for testing)

### First Time Setup

```bash
# Clone fork
git clone https://github.com/YOUR_USERNAME/binance-p2p-monitor.git
cd binance-p2p-monitor

# Install dependencies
make install

# Run tests to verify setup
make test

# Run application
make run
```

### Day-to-Day Workflow

```bash
# Create feature branch
git checkout -b feature/my-feature

# Make changes, test frequently
dotnet test -c Release

# Format and lint
make fmt lint

# Commit with clear messages
git commit -m "feat(service): add new monitoring capability"

# Push and open PR
git push origin feature/my-feature
```

## Review Process

### What We Look For

- ✅ Code quality (clarity, consistency, efficiency)
- ✅ Test coverage (unit + integration)
- ✅ Documentation (code comments, user docs)
- ✅ Performance (no regressions)
- ✅ Security (no vulnerabilities)
- ✅ Compliance (style guide, naming)

### Timeline

- **Small PRs** (1-3 files, <100 lines): 1-2 days
- **Medium PRs** (3-10 files, <500 lines): 2-5 days
- **Large PRs** (>500 lines): May ask to split into smaller PRs

### Common Feedback

- ❌ No file header comment
  - **Fix:** Add standard file header with author info

- ❌ Missing unit tests
  - **Fix:** Add tests for public methods

- ❌ Method too long (>50 lines)
  - **Fix:** Extract into smaller methods

- ❌ No CHANGELOG entry
  - **Fix:** Add entry to CHANGELOG.md

- ❌ Breaking API change without discussion
  - **Fix:** Open issue first to discuss

## Acknowledgments

Contributors will be acknowledged in:
- CHANGELOG.md
- GitHub contributions graph
- Project README (for significant contributions)

## Questions?

- 💬 **GitHub Discussions:** https://github.com/Sarmkadan/binance-p2p-monitor/discussions
- 📧 **Direct Contact:** vladyslav.zaiets@amdaris.com
- 📖 **Documentation:** See `docs/` directory

Thank you for contributing! 🚀

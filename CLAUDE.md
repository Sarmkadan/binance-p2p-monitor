# CLAUDE.md

## Overview
Binance P2P price monitoring CLI for .NET 10: WebSocket/HTTP price feed, spread analysis, SQLite history, Telegram/webhook alerts.

## Build
- `dotnet restore` / `make install`
- `dotnet build -c Debug` / `make build`
- `dotnet build -c Release` / `make release`
- `dotnet run -- <command>` (commands: monitor, status, help, alert, summary, history, export, version, backtest, spread, compare, doctor, prune)
- `make publish` - self-contained binaries for linux-x64 / win-x64 / osx-x64
- Docker: `make docker-build`, `make docker-run` (needs `TELEGRAM_BOT_TOKEN`, `TELEGRAM_ADMIN_CHAT_ID`)
- SDK pinned in `global.json` (10.0.100, rollForward latestMinor)

## Tests
- `dotnet test -c Release` (CI); `make test` assumes a prior build (`--no-build`)
- Test project: `tests/binance-p2p-monitor.Tests/` - xunit + FluentAssertions + Moq + NSubstitute
- Solution file: `binance-p2p-monitor.slnx` (main project, tests, benchmarks)

## Lint / Format
- `dotnet format --verify-no-changes` / `make lint`
- `dotnet format` / `make fmt`
- Style rules in `.editorconfig` (Allman braces, 4-space indent, nullable enabled, warnings not treated as errors)

## Key Directories
- `src/Program.cs` - entry point (`BinanceP2pMonitor.Program`); DI host setup and command registry
- `src/Commands/` - one class per CLI command (`XxxCommand.cs`)
- `src/Services/` - business logic (`IXxxService` + `XxxService`), `MonitoringHostedService`, `WebSocketService`
- `src/Repositories/` - SQLite data access (`IXxxRepository` + `XxxRepository`)
- `src/Data/` - `DatabaseContext`
- `src/Workers/` - background workers (cleanup, statistics)
- `src/Integration/` - Telegram / webhook notification clients
- `src/Configuration/`, `src/Constants/`, `src/Models/`, `src/Exceptions/`, `src/Extensions/`, `src/Formatters/`, `src/Caching/`, `src/Middleware/`, `src/Backtesting/`
- `src/GlobalUsings.cs` - global usings for all project namespaces
- `benchmarks/` - BenchmarkDotNet project; `examples/` - excluded from compile
- `appsettings.json`, `appsettings.development.json` (copied to output); `appsettings.example.json` as template
- `.github/workflows/` - ci, build, codeql, docker, release

## Conventions
- Root namespace `BinanceP2pMonitor`, sub-namespace per folder
- Interfaces prefixed `I`, registered via DI in `Program.cs`
- Partial-style split files: `XxxExtensions.cs`, `XxxValidation.cs`, `XxxJsonExtensions.cs` next to the main class
- Test files mirror source names: `XxxTests.cs`, `XxxTestsExtensions.cs`, `XxxTestsValidation.cs`
- `tests/`, `examples/`, `benchmarks/` are excluded from the main csproj via `Compile Remove`
- Do not commit `*.db`, `bin/`, `obj/`, `.aider*`

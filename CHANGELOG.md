# Changelog

All notable changes to binance-p2p-monitor are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-11-18

### Added
- WebSocket reconnection with exponential backoff
- Spread anomaly detection alerts
- Historical price aggregation (hourly, daily)
- CSV/JSON export functionality
- Performance metrics collection and reporting
- Database auto-cleanup worker
- Rate limiting with token bucket algorithm
- Telegram notification batching
- System health check command
- Docker Compose deployment configuration
- CI/CD pipeline with GitHub Actions
- Comprehensive documentation suite
- Example code for common use cases
- BenchmarkDotNet benchmarks for hot paths

### Changed
- Improved alert evaluation performance (now O(1) per alert)
- Enhanced price caching strategy (TTL-based)
- Refactored service layer for better testability
- Optimized SQLite indexes for faster queries

### Fixed
- WebSocket connection timeout handling
- Database lock contention under high load
- Telegram notification delivery delays
- Memory leak in cached price collection

### Security
- Sanitize API responses before logging
- Validate all user input at CLI layer

## [0.9.0] - 2025-10-28

### Added
- Event bus for loosely-coupled component communication
- Memory caching layer for price data
- Price history repository with time-series queries
- Alert cooldown enforcement
- Monitoring status command
- Console output formatters (table, JSON, CSV)
- Middleware pipeline (logging, validation, exception handling)

### Changed
- Migrated from direct HTTP polling to WebSocket for real-time updates
- Improved error messages for CLI commands
- Enhanced database schema with proper indexes

### Fixed
- WebSocket message parsing edge cases
- Concurrent alert evaluation race conditions
- Database query performance degradation over time

## [0.7.0] - 2025-10-06

### Added
- In-memory caching with configurable TTL
- Retry policy with jitter for transient failures
- Per-user alert limit enforcement
- `export` command for CSV and JSON output
- `history` command with date-range filtering
- Performance metrics tracking (latency, throughput)

### Changed
- Alert service refactored to use repository abstraction
- Configuration validation now runs at startup before DI resolution

### Fixed
- Null reference in spread calculation when no offers returned
- Alert deduplication across monitoring cycles

## [0.5.0] - 2025-09-09

### Added
- Spread analysis service with buy/sell gap detection
- Telegram notification client with retry
- `alert` command: create, list, remove, pause
- SQLite auto-migration on first run
- `--output` flag for json/csv/table format selection

### Changed
- Replaced raw HttpClient usage with typed `IHttpClientFactory`
- Moved constants into `ApplicationConstants` and enum files

### Fixed
- Rate limiter burst window off-by-one
- Telegram message encoding for special Markdown characters

## [0.3.0] - 2025-08-12

### Added
- Price history persistence (SQLite)
- CLI command framework (`CommandFactory`, `CommandParser`)
- `monitor` command with `--assets`, `--fiats`, `--interval` flags
- `status` command showing uptime and WebSocket health
- `help` and `version` commands
- Dependency injection container wired in `Program.cs`

### Changed
- Project restructured into `src/` subdirectories
- Configuration now loaded from `appsettings.json` with env-var override

### Fixed
- Database connection string not resolving relative paths correctly

## [0.1.0] - 2025-07-14

### Added
- Initial release
- Binance P2P REST API integration
- Real-time price monitoring for BTC, ETH, BNB, USDT
- Basic price alert system (price-change and spread thresholds)
- Telegram bot notification support
- SQLite database backend
- `AppSettings` configuration model
- Interfaces: `IPriceMonitoringService`, `IAlertService`, `ISpreadAnalysisService`

---

## Version History Summary

| Version | Release Date | Status   | Highlights                              |
|---------|-------------|----------|-----------------------------------------|
| 1.0.0   | 2025-11-18  | Latest   | Production-ready, WebSocket, Analytics  |
| 0.9.0   | 2025-10-28  | Stable   | Event bus, Caching, Middleware          |
| 0.7.0   | 2025-10-06  | Stable   | Retry policy, Export, Metrics           |
| 0.5.0   | 2025-09-09  | Stable   | Spread analysis, Telegram, Alerts       |
| 0.3.0   | 2025-08-12  | Stable   | CLI framework, History, DI              |
| 0.1.0   | 2025-07-14  | Archive  | Initial release                         |

## Upgrade Guide

### From 0.9.x to 1.0.0

**Breaking Changes:** None

**Migration Steps:**
1. Backup database: `cp binance_p2p.db binance_p2p.db.backup`
2. Add new configuration options (all have defaults — update is optional)
3. Restart application

**New Configuration Options:**
```json
{
  "EnableAutoCleanup": true,
  "HistoryRetentionDays": 30,
  "MaxHistoryRecords": 100000
}
```

### From 0.7.x to 0.9.0

**Breaking Changes:** Alert repository schema updated

**Migration Steps:**
1. Run database migration: `sqlite3 binance_p2p.db < migrations/001_add_alert_index.sql`
2. Restart application

### From 0.3.x to 0.5.0

**Breaking Changes:** CLI flag `--currency` renamed to `--fiats`

**Migration Steps:**
1. Update any scripts that call `--currency` to use `--fiats`
2. Restart application

---

## Development Roadmap

### Planned for 1.1.0
- [ ] Prometheus metrics export
- [ ] Redis caching backend
- [ ] PostgreSQL support
- [ ] REST API endpoints

### Planned for 1.2.0
- [ ] Slack integration
- [ ] Discord bot support
- [ ] Email notifications
- [ ] Webhook support

### Future Considerations
- [ ] Support for additional exchanges
- [ ] Backtesting framework
- [ ] Mobile app (iOS/Android)

## Known Issues

### Current Release (1.0.0)

**Database Locking (Rare)**
- Under sustained high load (>1000 prices/sec), occasional database lock timeouts
- Workaround: Increase `DatabaseCommandTimeoutSeconds` to 60 or higher

**WebSocket Reconnection Lag**
- First reconnection after disconnect may take up to 30 seconds
- Subsequent reconnections are faster due to exponential backoff

**Large History Exports**
- Exporting >1M records may consume significant memory
- Workaround: Export in smaller batches using date range filters

## Support & Feedback

- **Bug Reports:** https://github.com/Sarmkadan/binance-p2p-monitor/issues
- **Feature Requests:** https://github.com/Sarmkadan/binance-p2p-monitor/discussions

## Acknowledgments

This project is maintained by [Vladyslav Zaiets](https://sarmkadan.com).

Special thanks to:
- The .NET community
- Binance P2P API team
- Open source contributors

# Changelog

All notable changes to binance-p2p-monitor are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

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

### Changed
- Improved alert evaluation performance (now O(1) per alert)
- Enhanced price caching strategy (TTL-based)
- Refactored service layer for better testability
- Updated .NET to version 10.0
- Optimized SQLite indexes for faster queries

### Fixed
- WebSocket connection timeout handling
- Database lock contention under high load
- Telegram notification delivery delays
- Memory leak in cached price collection

### Security
- Sanitize API responses before logging
- Validate all user input at CLI layer
- Encrypt sensitive configuration options

## [1.1.0] - 2026-04-15

### Added
- Event bus for component communication
- Memory caching layer for price data
- Price history repository with time-series queries
- Alert cooldown enforcement
- Monitoring status command
- In-process dependency injection
- Console output formatters (table, JSON, CSV)

### Changed
- Migrated from direct HTTP to WebSocket for real-time updates
- Improved error messages for CLI commands
- Enhanced database schema with proper indexes

### Fixed
- WebSocket message parsing edge cases
- Concurrent alert evaluation race conditions
- Database query performance degradation over time

## [1.0.0] - 2026-03-20

### Added
- Core price monitoring service
- Binance P2P API integration
- SQLite database persistence
- CLI command framework
- Alert system with configurable rules
- Telegram bot notifications
- Price history tracking
- Basic export functionality

### Features
- Monitor multiple trading pairs simultaneously
- Create custom price alerts (change, spread, level)
- Real-time Telegram notifications
- Historical data export (CSV, JSON)
- Configurable monitoring intervals
- SQLite database backend
- Extensible architecture

## [0.9.0-beta] - 2026-02-28

### Added
- Initial beta release
- Basic monitoring capabilities
- Simple alert system
- SQLite persistence
- CLI interface

---

## Version History Summary

| Version | Release Date | Status | Features |
|---------|-------------|--------|----------|
| 1.2.0 | 2026-05-04 | Latest | Production-ready, WebSocket, Analytics |
| 1.1.0 | 2026-04-15 | Stable | Event bus, Caching, History |
| 1.0.0 | 2026-03-20 | Stable | Core monitoring, Alerts |
| 0.9.0-beta | 2026-02-28 | Archive | Initial release |

## Upgrade Guide

### From 1.1.x to 1.2.0

**Breaking Changes:** None

**Migration Steps:**
1. Backup database: `cp binance_p2p.db binance_p2p.db.backup`
2. Update configuration for new settings (optional)
3. Restart application

**New Configuration Options:**
```json
{
  "EnableAutoCleanup": true,
  "HistoryRetentionDays": 30,
  "MaxHistoryRecords": 100000
}
```

### From 1.0.x to 1.1.0

**Breaking Changes:** Alert repository schema updated

**Migration Steps:**
1. Run database migration script: `sqlite3 binance_p2p.db < migrations/001_add_alert_index.sql`
2. Clear cache (safe to delete): `rm -f cache/*.json`
3. Restart application

### From 0.9.x to 1.0.0

**Breaking Changes:** Complete database schema overhaul

**Migration Steps:**
1. Export all historical data: `dotnet run -- export --format json --output backup.json`
2. Delete old database: `rm binance_p2p.db`
3. Restart application (will create new schema)
4. Recreate alerts as needed

---

## Development Roadmap

### Planned for 1.3.0 (Q3 2026)
- [ ] Prometheus metrics export
- [ ] Redis caching backend
- [ ] PostgreSQL support
- [ ] REST API endpoints
- [ ] Web dashboard
- [ ] Advanced charting

### Planned for 1.4.0 (Q4 2026)
- [ ] Slack integration
- [ ] Discord bot support
- [ ] Email notifications
- [ ] SMS alerts
- [ ] Webhook support
- [ ] Custom indicators

### Future Considerations
- [ ] Support for additional exchanges
- [ ] Backtesting framework
- [ ] Machine learning price prediction
- [ ] Multi-language support
- [ ] Mobile app (iOS/Android)
- [ ] Cloud hosted monitoring

## Known Issues

### Current Release (1.2.0)

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

- 🐛 **Bug Reports:** https://github.com/Sarmkadan/binance-p2p-monitor/issues
- 💡 **Feature Requests:** https://github.com/Sarmkadan/binance-p2p-monitor/discussions
- 📧 **Direct Contact:** vladyslav.zaiets@amdaris.com

## Acknowledgments

This project is maintained by [Vladyslav Zaiets](https://sarmkadan.com).

Special thanks to:
- The .NET community
- Binance P2P API team
- Open source contributors

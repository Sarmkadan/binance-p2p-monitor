# ConfigurationValidator

Validates application configuration before startup.

## Namespace
`BinanceP2pMonitor.Infrastructure`

## Class
```csharp
public sealed class ConfigurationValidator
```

## Constructor
```csharp
public ConfigurationValidator(AppSettings appSettings, ILogger<ConfigurationValidator> logger)
```
- `appSettings`: Application settings to validate
- `logger`: Logger for validation messages

## Methods

### Validate()
Validates all configuration settings.

**Returns:** List of validation errors, empty if valid.

**Process:**
1. Logs "Validating configuration..."
2. Runs validation in sequence:
   - Database settings
   - Monitoring settings
   - Alerting settings
   - Telegram settings
   - Assets settings
3. If errors found:
   - Logs error count
   - Logs each error
4. If no errors:
   - Logs "Configuration validation passed"
5. Returns list of errors

### Private Validation Methods

#### ValidateDatabase()
Validates database-related settings:
- `DatabaseConnectionString`: Required (not null/empty/whitespace)
- `HistoryRetentionDays`: Must be ≥ 1
- `MaxHistoryRecords`: Must be ≥ 100
- `DatabaseCommandTimeoutSeconds`: Must be ≥ 5

#### ValidateMonitoring()
Validates monitoring settings:
- `MonitoringIntervalSeconds`: Must be ≥ 5 seconds
- Notification methods: At least one of `EnableWebSocket` or `EnableTelegramNotifications` must be true

#### ValidateAlerting()
Validates alerting settings:
- `AlertCooldownMinutes`: Must be ≥ 1
- `MaxAlertsPerUser`: Must be ≥ 1
- `DefaultPriceChangeThreshold`: Cannot be negative
- `DefaultSpreadThreshold`: Cannot be negative

#### ValidateTelegram()
Validates Telegram settings (only if `EnableTelegramNotifications` is true):
- `TelegramBotToken`: Required
- `TelegramAdminChatId`: Required and must be a valid numeric chat ID

#### ValidateAssets()
Validates monitored assets:
- Logs warning if `MonitoredAssets` is empty
- Logs warning if `MonitoredFiats` is empty
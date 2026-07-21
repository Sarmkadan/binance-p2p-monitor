#nullable enable

using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Infrastructure;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to validate configuration and check system health
/// </summary>
public sealed class DoctorCommand : ICommand
{
    private readonly ConfigurationValidator _configValidator;
    private readonly DatabaseContext _dbContext;
    private readonly ConsoleOutputWriter _output;
    private readonly AppSettings _appSettings;
    private readonly ILogger<DoctorCommand> _logger;

    public string Name => "doctor";
    public string Description => "Validate configuration and check system health";

    public DoctorCommand(
        ConfigurationValidator configValidator,
        DatabaseContext dbContext,
        ConsoleOutputWriter output,
        AppSettings appSettings,
        ILogger<DoctorCommand> logger)
    {
        _configValidator = configValidator ?? throw new ArgumentNullException(nameof(configValidator));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GetHelp()
    {
        return @"Usage: binance-p2p-monitor doctor [options]

Validate application configuration and check system health.

Options:
  -h, --help  Show this help message

Examples:
  binance-p2p-monitor doctor
  binance-p2p-monitor doctor --help
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        return new List<string>();
    }

    public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        _output.WriteHeader("System Health Check");
        _output.WriteBlankLine();

        var allChecksPassed = true;
        var results = new List<(string CheckName, bool Passed, string? Message)>();

        // Check 1: Configuration validation
        _output.WriteSection("1. Configuration Validation");
        var configErrors = _configValidator.Validate();
        var configPassed = !configErrors.Any();
        results.Add(("Configuration", configPassed, configPassed ? "All configuration checks passed" : $"Failed: {configErrors.Count} errors"));

        if (!configPassed)
        {
            foreach (var error in configErrors)
            {
                _output.WriteError($"  - {error}");
            }
        }
        else
        {
            _output.WriteSuccess("  ✓ All configuration checks passed");
        }
        _output.WriteBlankLine();

        // Check 2: Database connection
        _output.WriteSection("2. Database Connection");
        var dbPassed = await CheckDatabaseConnectionAsync();
        results.Add(("Database Connection", dbPassed, dbPassed ? "Database connection successful" : "Failed to connect to database"));
        _output.WriteBlankLine();

        // Check 3: Database schema initialization
        _output.WriteSection("3. Database Schema");
        var schemaPassed = await CheckDatabaseSchemaAsync();
        results.Add(("Database Schema", schemaPassed, schemaPassed ? "Database schema is valid" : "Database schema validation failed"));
        _output.WriteBlankLine();

        // Check 4: Configuration values
        _output.WriteSection("4. Configuration Values");
        var configValuesPassed = await CheckConfigurationValuesAsync();
        results.Add(("Configuration Values", configValuesPassed, configValuesPassed ? "All configuration values are valid" : "Configuration values validation failed"));
        _output.WriteBlankLine();

        // Summary
        _output.WriteSection("Summary");
        var totalChecks = results.Count;
        var passedChecks = results.Count(r => r.Passed);
        var failedChecks = totalChecks - passedChecks;

        foreach (var result in results)
        {
            var status = result.Passed ? "✓ PASS" : "✗ FAIL";
            if (result.Passed)
            {
                _output.WriteSuccess($"  {status} {result.CheckName}: {result.Message}");
            }
            else
            {
                _output.WriteError($"  {status} {result.CheckName}: {result.Message}");
            }
        }

        _output.WriteBlankLine();
        if (failedChecks == 0)
        {
            _output.WriteSuccess($"All {totalChecks} checks passed! System is healthy.");
            return 0;
        }
        else
        {
            _output.WriteError($"{failedChecks} of {totalChecks} checks failed. Please review the errors above.");
            return 1;
        }
    }

    private async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            var connection = _dbContext.GetConnection();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                _output.WriteSuccess("  ✓ Database connection established");
                _output.WriteKeyValue("  Connection State", connection.State.ToString());
                _output.WriteKeyValue("  Connection String", TruncateConnectionString(_appSettings.DatabaseConnectionString));
                return true;
            }
            else
            {
                _output.WriteError("  ✗ Database connection is not open");
                _output.WriteKeyValue("  Connection State", connection.State.ToString());
                return false;
            }
        }
        catch (Exception ex)
        {
            _output.WriteError($"  ✗ Failed to connect to database: {ex.Message}");
            _logger.LogError(ex, "Database connection check failed");
            return false;
        }
    }

    private async Task<bool> CheckDatabaseSchemaAsync()
    {
        try
        {
            // Try to execute a simple query to verify schema exists
            var connection = _dbContext.GetConnection();

            // Check if Prices table exists
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Prices'";
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());

            if (count > 0)
            {
                _output.WriteSuccess("  ✓ Database schema validated");
                _output.WriteKeyValue("  Tables Found", count.ToString());

                // Get table count
                command.CommandText = "SELECT COUNT(*) FROM Prices";
                var recordCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                _output.WriteKeyValue("  Sample Records", recordCount.ToString());

                return true;
            }
            else
            {
                _output.WriteError("  ✗ Database schema not initialized - Prices table not found");
                return false;
            }
        }
        catch (Exception ex)
        {
            _output.WriteError($"  ✗ Failed to validate database schema: {ex.Message}");
            _logger.LogError(ex, "Database schema validation failed");
            return false;
        }
    }

    private async Task<bool> CheckConfigurationValuesAsync()
    {
        try
        {
            var checksPassed = true;

            _output.WriteKeyValue("  Monitoring Interval", $"{_appSettings.MonitoringIntervalSeconds}s");
            _output.WriteKeyValue("  Alert Cooldown", $"{_appSettings.AlertCooldownMinutes}m");
            _output.WriteKeyValue("  WebSocket Enabled", _appSettings.EnableWebSocket ? "Yes" : "No");
            _output.WriteKeyValue("  Telegram Enabled", _appSettings.EnableTelegramNotifications ? "Yes" : "No");
            _output.WriteKeyValue("  History Retention", $"{_appSettings.HistoryRetentionDays} days");
            _output.WriteKeyValue("  Max Alerts per User", $"{_appSettings.MaxAlertsPerUser}");
            _output.WriteKeyValue("  Default Price Change Threshold", $"{_appSettings.DefaultPriceChangeThreshold}%");
            _output.WriteKeyValue("  Default Spread Threshold", $"{_appSettings.DefaultSpreadThreshold}%");
            _output.WriteKeyValue("  Database Timeout", $"{_appSettings.DatabaseCommandTimeoutSeconds}s");

            // Check monitored assets
            if (_appSettings.MonitoredAssets.Any())
            {
                _output.WriteKeyValue("  Monitored Assets", string.Join(", ", _appSettings.MonitoredAssets.Take(5)));
                if (_appSettings.MonitoredAssets.Count > 5)
                {
                    _output.WriteKeyValue("  ... and more", $"{_appSettings.MonitoredAssets.Count - 5} additional assets");
                }
            }
            else
            {
                _output.WriteWarning("  ⚠ No monitored assets configured");
                checksPassed = false;
            }

            // Check monitored fiats
            if (_appSettings.MonitoredFiats.Any())
            {
                _output.WriteKeyValue("  Monitored Fiats", string.Join(", ", _appSettings.MonitoredFiats.Take(5)));
                if (_appSettings.MonitoredFiats.Count > 5)
                {
                    _output.WriteKeyValue("  ... and more", $"{_appSettings.MonitoredFiats.Count - 5} additional fiats");
                }
            }
            else
            {
                _output.WriteWarning("  ⚠ No monitored fiats configured");
                checksPassed = false;
            }

            return checksPassed;
        }
        catch (Exception ex)
        {
            _output.WriteError($"  ✗ Failed to check configuration values: {ex.Message}");
            _logger.LogError(ex, "Configuration values check failed");
            return false;
        }
    }

    private string TruncateConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "(not set)";

        // Truncate for security
        if (connectionString.Contains("Password="))
        {
            var parts = connectionString.Split(new[] { ";Password=" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                return parts[0] + ";Password=***REDACTED***";
            }
        }

        if (connectionString.Contains("password="))
        {
            var parts = connectionString.Split(new[] { ";password=" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                return parts[0] + ";password=***REDACTED***";
            }
        }

        // Return first 50 chars if no password found
        return connectionString.Length > 50 ? connectionString[..50] + "..." : connectionString;
    }
}

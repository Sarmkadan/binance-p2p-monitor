#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Workers;

/// <summary>
/// Background worker for cleaning up old records from database
/// </summary>
public class DatabaseCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseCleanupWorker> _logger;
    private readonly AppSettings _appSettings;

    public DatabaseCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<DatabaseCleanupWorker> logger,
        AppSettings appSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _appSettings = appSettings;
    }

    /// <summary>
    /// Runs cleanup every 6 hours
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database cleanup worker started");

        // Initial delay to avoid running during startup
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldRecordsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Database cleanup worker stopped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }
    }

    private async Task CleanupOldRecordsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var historyRepository = scope.ServiceProvider.GetRequiredService<IHistoryRepository>();

        _logger.LogInformation("Starting database cleanup. Retention period: {Days} days", _appSettings.HistoryRetentionDays);

        var cutoffDate = DateTime.UtcNow.AddDays(-_appSettings.HistoryRetentionDays);

        // Delete old price history records
        var deletedCount = 0;
        _logger.LogInformation("Deleted {Count} old history records (older than {CutoffDate})", deletedCount, cutoffDate);

        // Count total records
        _logger.LogInformation("Database cleanup completed. Current record count: ~{EstimatedCount}",
            _appSettings.MaxHistoryRecords);

        // Log database size
        _logger.LogDebug("Database maintenance completed successfully");
    }
}

#nullable enable
using BinanceP2pMonitor.Services;

namespace BinanceP2pMonitor.Workers;

/// <summary>
/// Background worker for cleaning up old records from database
/// </summary>
public class DatabaseCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseCleanupWorker> _logger;
    private readonly AppSettings _appSettings;
    private readonly IDatabaseCleanupService _databaseCleanupService;

    public DatabaseCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<DatabaseCleanupWorker> logger,
        AppSettings appSettings,
        IDatabaseCleanupService databaseCleanupService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _appSettings = appSettings;
        _databaseCleanupService = databaseCleanupService ?? throw new ArgumentNullException(nameof(databaseCleanupService));
    }

    /// <summary>
    /// Runs cleanup every 6 hours
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database cleanup worker started");

        // Initial delay to avoid running during startup
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldRecordsAsync(stoppingToken).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Database cleanup worker stopped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database cleanup");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task CleanupOldRecordsAsync(CancellationToken ct)
    {
        _logger.LogInformation("Starting database cleanup. Retention period: {Days} days", _appSettings.HistoryRetentionDays);

        var deletedCount = await _databaseCleanupService.DeleteOldRecordsAsync(_appSettings.HistoryRetentionDays).ConfigureAwait(false);

        var remainingCount = await _databaseCleanupService.GetTotalHistoryCountAsync().ConfigureAwait(false);

        // Enforce maximum record cap as a secondary safeguard
        if (remainingCount > _appSettings.MaxHistoryRecords)
        {
            _logger.LogWarning("History record count ({Count}) exceeds MaxHistoryRecords ({Max}). " +
                "Consider reducing HistoryRetentionDays or increasing MaxHistoryRecords.",
                remainingCount, _appSettings.MaxHistoryRecords);
        }

        _logger.LogInformation("Database cleanup completed. Deleted {DeletedCount} records. Current count: {Count}",
            deletedCount, remainingCount);
    }
}

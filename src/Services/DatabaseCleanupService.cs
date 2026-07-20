#nullable enable

using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Exceptions;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service implementation for cleaning up old records from the database
/// </summary>
public class DatabaseCleanupService : IDatabaseCleanupService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly ILogger<DatabaseCleanupService> _logger;

    public DatabaseCleanupService(
        IHistoryRepository historyRepository,
        ILogger<DatabaseCleanupService> logger)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Deletes records older than the specified number of days
    /// </summary>
    /// <param name="daysOld">Number of days to keep (records older than this will be deleted)</param>
    /// <returns>Number of records deleted</returns>
    public async Task<int> DeleteOldRecordsAsync(int daysOld)
    {
        if (daysOld < 0)
        {
            _logger.LogWarning("DeleteOldRecordsAsync called with negative daysOld: {DaysOld}", daysOld);
            throw new ArgumentException("Days old must be non-negative", nameof(daysOld));
        }

        _logger.LogInformation("Deleting records older than {DaysOld} days", daysOld);

        try
        {
            // The repository method returns true on success, but we need the count
            // Since we can't easily get the count from the DELETE operation, we'll query before and after
            var initialCount = await _historyRepository.GetTotalHistoryCountAsync().ConfigureAwait(false);

            var success = await _historyRepository.DeleteOldRecordsAsync(daysOld).ConfigureAwait(false);

            var remainingCount = await _historyRepository.GetTotalHistoryCountAsync().ConfigureAwait(false);
            var deletedCount = (int)(initialCount - remainingCount);

            _logger.LogInformation("Deleted {DeletedCount} records older than {DaysOld} days. Remaining: {RemainingCount}",
                deletedCount, daysOld, remainingCount);

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete old records");
            throw new DataAccessException("Failed to delete old records", ex);
        }
    }

    /// <summary>
    /// Gets the total number of history records in the database
    /// </summary>
    /// <returns>Total count of history records</returns>
    public async Task<long> GetTotalHistoryCountAsync()
    {
        try
        {
            return await _historyRepository.GetTotalHistoryCountAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history count");
            throw new DataAccessException("Failed to get history count", ex);
        }
    }
}

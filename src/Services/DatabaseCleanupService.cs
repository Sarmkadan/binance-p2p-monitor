#nullable enable

using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Exceptions;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service implementation for cleaning up old records from the database
/// </summary>
public class DatabaseCleanupService : IDatabaseCleanupService
{
    private const int MinimumDaysOld = 0;
    private const string NegativeDaysOldLogMessage = "DeleteOldRecordsAsync called with negative daysOld: {DaysOld}";
    private const string NonNegativeDaysOldErrorMessage = "Days old must be non-negative";
    private const string DeletingRecordsLogMessage = "Deleting records older than {DaysOld} days";
    private const string DeletedRecordsLogMessage = "Deleted {DeletedCount} records older than {DaysOld} days. Remaining: {RemainingCount}";
    private const string FailedToDeleteOldRecordsMessage = "Failed to delete old records";
    private const string FailedToGetHistoryCountMessage = "Failed to get history count";

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
        if (daysOld < MinimumDaysOld)
        {
            _logger.LogWarning(NegativeDaysOldLogMessage, daysOld);
            throw new ArgumentException(NonNegativeDaysOldErrorMessage, nameof(daysOld));
        }

        _logger.LogInformation(DeletingRecordsLogMessage, daysOld);

        try
        {
            // The repository method returns true on success, but we need the count
            // Since we can't easily get the count from the DELETE operation, we'll query before and after
            var initialCount = await _historyRepository.GetTotalHistoryCountAsync().ConfigureAwait(false);

            var success = await _historyRepository.DeleteOldRecordsAsync(daysOld).ConfigureAwait(false);

            var remainingCount = await _historyRepository.GetTotalHistoryCountAsync().ConfigureAwait(false);
            var deletedCount = (int)(initialCount - remainingCount);

            _logger.LogInformation(DeletedRecordsLogMessage,
                deletedCount, daysOld, remainingCount);

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToDeleteOldRecordsMessage);
            throw new DataAccessException(FailedToDeleteOldRecordsMessage, ex);
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
            _logger.LogError(ex, FailedToGetHistoryCountMessage);
            throw new DataAccessException(FailedToGetHistoryCountMessage, ex);
        }
    }
}

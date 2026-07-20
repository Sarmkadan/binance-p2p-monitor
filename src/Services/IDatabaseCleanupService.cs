#nullable enable

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service for cleaning up old records from the database
/// </summary>
public interface IDatabaseCleanupService
{
    /// <summary>
    /// Deletes records older than the specified number of days
    /// </summary>
    /// <param name="daysOld">Number of days to keep (records older than this will be deleted)</param>
    /// <returns>Number of records deleted</returns>
    Task<int> DeleteOldRecordsAsync(int daysOld);

    /// <summary>
    /// Gets the total number of history records in the database
    /// </summary>
    /// <returns>Total count of history records</returns>
    Task<long> GetTotalHistoryCountAsync();
}

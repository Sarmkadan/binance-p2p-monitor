namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides extension methods for <see cref="PriceHistory"/>.
/// </summary>
public static class PriceHistoryExtensions
{
    /// <summary>
    /// Determines if the price history record is older than a specified number of minutes.
    /// </summary>
    /// <param name="priceHistory">The price history record.</param>
    /// <param name="minutes">The number of minutes.</param>
    /// <returns><c>true</c> if the price history record is older than the specified number of minutes; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="priceHistory"/> is <c>null</c>.</exception>
    public static bool IsOlderThan(this PriceHistory priceHistory, int minutes)
    {
        ArgumentNullException.ThrowIfNull(priceHistory);

        return priceHistory.GetAgeInMinutes() > minutes;
    }

    /// <summary>
    /// Gets the price history records that are within a specified time range.
    /// </summary>
    /// <param name="priceHistories">The collection of price history records.</param>
    /// <param name="startDate">The start date of the time range.</param>
    /// <param name="endDate">The end date of the time range.</param>
    /// <returns>An <see cref="IReadOnlyList{PriceHistory}"/> of price history records within the specified time range.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="priceHistories"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startDate"/> is after <paramref name="endDate"/>.</exception>
    public static IReadOnlyList<PriceHistory> GetInTimeRange(this IEnumerable<PriceHistory> priceHistories, DateTime startDate, DateTime endDate)
    {
        ArgumentNullException.ThrowIfNull(priceHistories);
        ArgumentOutOfRangeException.ThrowIfLessThan(endDate, startDate, nameof(endDate));

        return priceHistories
            .Where(ph => ph.RecordedAt >= startDate && ph.RecordedAt <= endDate)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Calculates the average price change percentage for a collection of price history records.
    /// </summary>
    /// <param name="priceHistories">The collection of price history records.</param>
    /// <returns>The average price change percentage.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="priceHistories"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
    public static decimal CalculateAveragePriceChangePercentage(this IEnumerable<PriceHistory> priceHistories)
    {
        ArgumentNullException.ThrowIfNull(priceHistories);

        return priceHistories.Average(ph => ph.PriceChangePercent);
    }

    /// <summary>
    /// Filters price history records to only include recent records (within the last hour).
    /// </summary>
    /// <param name="priceHistories">The collection of price history records to filter.</param>
    /// <returns>An <see cref="IReadOnlyList{PriceHistory}"/> containing only recent records.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="priceHistories"/> is <c>null</c>.</exception>
    public static IReadOnlyList<PriceHistory> WhereRecent(this IEnumerable<PriceHistory> priceHistories)
    {
        ArgumentNullException.ThrowIfNull(priceHistories);

        return priceHistories
            .Where(ph => ph.IsRecent())
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Calculates the average spread percentage across all price history records.
    /// </summary>
    /// <param name="priceHistories">The collection of price history records.</param>
    /// <returns>The average spread percentage.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="priceHistories"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
    public static decimal CalculateAverageSpreadPercentage(this IEnumerable<PriceHistory> priceHistories)
    {
        ArgumentNullException.ThrowIfNull(priceHistories);

        return priceHistories.Average(ph => ph.SpreadPercentage);
    }

    /// <summary>
    /// Calculates the average mid-price across all price history records.
    /// </summary>
    /// <param name="priceHistories">The collection of price history records.</param>
    /// <returns>The average mid-price.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="priceHistories"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the collection is empty.</exception>
    public static decimal CalculateAverageMidPrice(this IEnumerable<PriceHistory> priceHistories)
    {
        ArgumentNullException.ThrowIfNull(priceHistories);

        return priceHistories.Average(ph => ph.GetMidPrice());
    }
}

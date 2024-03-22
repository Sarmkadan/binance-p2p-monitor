#nullable enable

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Extension methods for <see cref="PerformanceMetrics"/> providing additional functionality
/// </summary>
public static class PerformanceMetricsExtensions
{
    /// <summary>
    /// Gets the success rate percentage for the operation (0-100)
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to get metrics for.</param>
    /// <returns>The success rate percentage (0-100), or 0 if operation has not been recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static double GetSuccessRate(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.GetSuccessRate ?? 0;
    }

    /// <summary>
    /// Gets the average duration in milliseconds for the operation
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to get metrics for.</param>
    /// <returns>The average duration in milliseconds, or 0 if operation has not been recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static double GetAverageDurationMs(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.GetAverageDurationMs ?? 0;
    }

    /// <summary>
    /// Gets the failure count for the operation
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to get metrics for.</param>
    /// <returns>The number of failed operations, or 0 if operation has not been recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static int GetFailureCount(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.FailureCount ?? 0;
    }

    /// <summary>
    /// Gets the total count of operations recorded
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to get metrics for.</param>
    /// <returns>The total count of operations, or 0 if operation has not been recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static int GetTotalCount(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.TotalCount ?? 0;
    }

    /// <summary>
    /// Gets the total duration for the operation in milliseconds
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to get metrics for.</param>
    /// <returns>The total duration in milliseconds, or 0 if operation has not been recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static double GetTotalDurationMs(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.TotalDuration.TotalMilliseconds ?? 0;
    }

    /// <summary>
    /// Checks if the operation has been executed at least once
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to check.</param>
    /// <returns><see langword="true"/> if the operation has been executed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static bool HasExecuted(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics != null;
    }

    /// <summary>
    /// Gets the last execution time for the operation
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <param name="operationName">Name of the operation to get metrics for.</param>
    /// <returns>The last execution time, or <see langword="null"/> if operation has not been recorded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="operationName"/> is <see langword="null"/></exception>
    public static DateTime? GetLastExecutionTime(this PerformanceMetrics metrics, string operationName)
    {
        ArgumentNullException.ThrowIfNull(operationName);
        ArgumentNullException.ThrowIfNull(metrics);

        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.LastExecutionTime;
    }

    /// <summary>
    /// Gets the operation metrics for the most recently executed operation
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <returns>The metrics for the most recently executed operation, or <see langword="null"/> if no operations have been recorded.</returns>
    public static PerformanceMetrics.OperationMetrics? GetMostRecentOperation(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.GetAllMetrics()
            .OrderByDescending(kvp => kvp.Value.LastExecutionTime)
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the operation name with the highest failure rate
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <returns>The name of the operation with the highest failure rate, or <see langword="null"/> if no operations have been recorded.</returns>
    public static string? GetOperationWithHighestFailureRate(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.GetAllMetrics()
            .OrderByDescending(kvp => kvp.Value.FailureCount)
            .ThenByDescending(kvp => kvp.Value.TotalCount)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the average success rate across all operations
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <returns>The average success rate percentage (0-100) across all operations, or 0 if no operations have been recorded.</returns>
    public static double GetAverageSuccessRate(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        var allMetrics = metrics.GetAllMetrics();
        return allMetrics.Count == 0
            ? 0
            : allMetrics.Average(kvp => kvp.Value.GetSuccessRate);
    }

    /// <summary>
    /// Gets the total number of operations tracked
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <returns>The total number of distinct operations tracked.</returns>
    public static int GetOperationCount(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.GetAllMetrics().Count;
    }

    /// <summary>
    /// Gets the total number of operations that have failed at least once
    /// </summary>
    /// <param name="metrics">The performance metrics instance.</param>
    /// <returns>The count of operations with at least one failure.</returns>
    public static int GetFailedOperationCount(this PerformanceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.GetAllMetrics()
            .Count(kvp => kvp.Value.FailureCount > 0);
    }
}
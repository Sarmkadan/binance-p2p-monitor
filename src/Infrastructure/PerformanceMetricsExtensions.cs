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
    public static double GetSuccessRate(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.GetSuccessRate ?? 0;
    }

    /// <summary>
    /// Gets the average duration in milliseconds for the operation
    /// </summary>
    public static double GetAverageDurationMs(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.GetAverageDurationMs ?? 0;
    }

    /// <summary>
    /// Gets the failure count for the operation
    /// </summary>
    public static int GetFailureCount(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.FailureCount ?? 0;
    }

    /// <summary>
    /// Gets the total count of operations recorded
    /// </summary>
    public static int GetTotalCount(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.TotalCount ?? 0;
    }

    /// <summary>
    /// Gets the total duration for the operation in milliseconds
    /// </summary>
    public static double GetTotalDurationMs(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.TotalDuration.TotalMilliseconds ?? 0;
    }

    /// <summary>
    /// Checks if the operation has been executed at least once
    /// </summary>
    public static bool HasExecuted(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics != null;
    }

    /// <summary>
    /// Gets the last execution time for the operation
    /// </summary>
    public static DateTime? GetLastExecutionTime(this PerformanceMetrics metrics, string operationName)
    {
        var operationMetrics = metrics.GetMetrics(operationName);
        return operationMetrics?.LastExecutionTime;
    }

    /// <summary>
    /// Gets the operation metrics for the most recently executed operation
    /// </summary>
    public static PerformanceMetrics.OperationMetrics? GetMostRecentOperation(this PerformanceMetrics metrics)
    {
        return metrics.GetAllMetrics()
            .OrderByDescending(kvp => kvp.Value.LastExecutionTime)
            .Select(kvp => kvp.Value)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the operation name with the highest failure rate
    /// </summary>
    public static string? GetOperationWithHighestFailureRate(this PerformanceMetrics metrics)
    {
        return metrics.GetAllMetrics()
            .OrderByDescending(kvp => kvp.Value.FailureCount)
            .ThenByDescending(kvp => kvp.Value.TotalCount)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the average success rate across all operations
    /// </summary>
    public static double GetAverageSuccessRate(this PerformanceMetrics metrics)
    {
        var allMetrics = metrics.GetAllMetrics();
        if (!allMetrics.Any())
            return 0;

        return allMetrics.Average(kvp => kvp.Value.GetSuccessRate);
    }

    /// <summary>
    /// Gets the total number of operations tracked
    /// </summary>
    public static int GetOperationCount(this PerformanceMetrics metrics)
    {
        return metrics.GetAllMetrics().Count;
    }

    /// <summary>
    /// Gets the total number of operations that have failed at least once
    /// </summary>
    public static int GetFailedOperationCount(this PerformanceMetrics metrics)
    {
        return metrics.GetAllMetrics()
            .Count(kvp => kvp.Value.FailureCount > 0);
    }
}
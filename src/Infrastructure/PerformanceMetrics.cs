#nullable enable
namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Tracks and reports application performance metrics
/// </summary>
public class PerformanceMetrics
{
    private readonly Dictionary<string, OperationMetrics> _metrics = new();
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Records an operation metric
    /// </summary>
    public void RecordOperation(string operationName, TimeSpan duration, bool success = true)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_metrics.TryGetValue(operationName, out var metrics))
            {
                metrics = new OperationMetrics { OperationName = operationName };
                _metrics[operationName] = metrics;
            }

            metrics.TotalCount++;
            metrics.TotalDuration += duration;

            if (success)
                metrics.SuccessCount++;
            else
                metrics.FailureCount++;

            metrics.MinDuration = metrics.MinDuration == TimeSpan.Zero
                ? duration
                : TimeSpan.FromMilliseconds(Math.Min(metrics.MinDuration.TotalMilliseconds, duration.TotalMilliseconds));

            metrics.MaxDuration = TimeSpan.FromMilliseconds(Math.Max(metrics.MaxDuration.TotalMilliseconds, duration.TotalMilliseconds));
            metrics.LastExecutionTime = DateTime.UtcNow;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Gets metrics for a specific operation
    /// </summary>
    public OperationMetrics? GetMetrics(string operationName)
    {
        _lock.EnterReadLock();
        try
        {
            return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Gets all recorded metrics
    /// </summary>
    public IReadOnlyDictionary<string, OperationMetrics> GetAllMetrics()
    {
        _lock.EnterReadLock();
        try
        {
            return _metrics.AsReadOnly();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Generates a performance report
    /// </summary>
    public string GenerateReport()
    {
        _lock.EnterReadLock();
        try
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║           Performance Metrics Report                             ║");
            report.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            report.AppendLine();

            if (!_metrics.Any())
            {
                report.AppendLine("(No metrics recorded)");
                return report.ToString();
            }

            report.AppendLine($"{"Operation",-30} {"Count",8} {"Avg(ms)",10} {"Min(ms)",10} {"Max(ms)",10} {"Success%",10}");
            report.AppendLine(new string('-', 78));

            foreach (var (opName, metrics) in _metrics.OrderBy(m => m.Key))
            {
                var avgMs = metrics.TotalCount > 0 ? metrics.TotalDuration.TotalMilliseconds / metrics.TotalCount : 0;
                var successPercent = metrics.TotalCount > 0 ? (metrics.SuccessCount * 100.0 / metrics.TotalCount) : 0;

                report.AppendLine(
                    $"{opName,-30} {metrics.TotalCount,8} {avgMs,10:F2} {metrics.MinDuration.TotalMilliseconds,10:F2} {metrics.MaxDuration.TotalMilliseconds,10:F2} {successPercent,10:F1}");
            }

            report.AppendLine();

            return report.ToString();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Clears all metrics
    /// </summary>
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _metrics.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public class OperationMetrics
    {
        public string OperationName { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan MinDuration { get; set; }
        public TimeSpan MaxDuration { get; set; }
        public DateTime LastExecutionTime { get; set; }

        public double GetAverageDurationMs => TotalCount > 0 ? TotalDuration.TotalMilliseconds / TotalCount : 0;
        public double GetSuccessRate => TotalCount > 0 ? (SuccessCount * 100.0 / TotalCount) : 0;
    }
}

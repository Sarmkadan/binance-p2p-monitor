// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Middleware;

/// <summary>
/// Middleware for comprehensive command execution logging
/// </summary>
public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly Func<CommandContext, Task<int>> _next;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger, Func<CommandContext, Task<int>> next)
    {
        _logger = logger;
        _next = next;
    }

    /// <summary>
    /// Logs command execution with timing and result
    /// </summary>
    public async Task<int> InvokeAsync(CommandContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var commandInfo = $"{context.CommandName} {string.Join(" ", context.Arguments)}";

        _logger.LogInformation("Executing command: {Command}", commandInfo);

        if (context.Options.Any())
            _logger.LogDebug("Options: {Options}", string.Join(", ", context.Options.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        if (context.Flags.Any())
            _logger.LogDebug("Flags: {Flags}", string.Join(", ", context.Flags.Keys));

        try
        {
            var result = await _next(context);
            stopwatch.Stop();

            _logger.LogInformation("Command completed: {Command} in {ElapsedMs}ms with exit code {ExitCode}",
                commandInfo, stopwatch.ElapsedMilliseconds, result);

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Command cancelled: {Command} after {ElapsedMs}ms", commandInfo, stopwatch.ElapsedMilliseconds);
            return -1;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Command failed: {Command} after {ElapsedMs}ms", commandInfo, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}

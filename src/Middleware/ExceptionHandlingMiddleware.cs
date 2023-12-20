// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Middleware;

/// <summary>
/// Middleware for centralized exception handling and error reporting
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly Func<CommandContext, Task<int>> _next;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, Func<CommandContext, Task<int>> next)
    {
        _logger = logger;
        _next = next;
    }

    /// <summary>
    /// Catches and handles exceptions during command execution
    /// </summary>
    public async Task<int> InvokeAsync(CommandContext context)
    {
        try
        {
            return await _next(context);
        }
        catch (BinanceP2pException ex)
        {
            _logger.LogError(ex, "Binance P2P exception: {Message}", ex.Message);
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Utilities.ValidationException ex)
        {
            _logger.LogWarning("Validation error: {Message}", ex.Message);
            Console.Error.WriteLine($"Validation error: {ex.Message}");
            foreach (var error in ex.Errors)
                Console.Error.WriteLine($"  - {error}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            Console.Error.WriteLine($"Invalid operation: {ex.Message}");
            return 1;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation cancelled by user");
            return 130;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred: {Message}", ex.Message);
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            if (context.HasFlag("verbose"))
                Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}

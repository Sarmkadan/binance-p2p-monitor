#nullable enable
namespace BinanceP2pMonitor.Middleware;

/// <summary>
/// Middleware for validating command arguments before execution
/// </summary>
public class ValidationMiddleware
{
    private readonly ILogger<ValidationMiddleware> _logger;
    private readonly Func<CommandContext, ICommand?, Task<int>> _next;

    public ValidationMiddleware(ILogger<ValidationMiddleware> logger, Func<CommandContext, ICommand?, Task<int>> next)
    {
        _logger = logger;
        _next = next;
    }

    /// <summary>
    /// Validates command arguments and options before execution
    /// </summary>
    public async Task<int> InvokeAsync(CommandContext context, ICommand? command)
    {
        if (command is null)
        {
            _logger.LogWarning("Command not found: {CommandName}", context.CommandName);
            Console.Error.WriteLine($"Error: Unknown command '{context.CommandName}'");
            return 1;
        }

        var validationErrors = command.ValidateArguments(context);
        if (validationErrors.Any())
        {
            _logger.LogWarning("Command validation failed: {CommandName}", context.CommandName);
            Console.Error.WriteLine($"Validation failed for command '{command.Name}':");
            foreach (var error in validationErrors)
                Console.Error.WriteLine($"  - {error}");
            Console.Error.WriteLine($"\nRun with --help for usage information");
            return 1;
        }

        _logger.LogDebug("Validation passed for command: {CommandName}", context.CommandName);
        return await _next(context, command);
    }
}

#nullable enable

using System.Threading;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.CLI;

/// <summary>
/// Interface for CLI commands
/// </summary>
public interface ICommand
{
    /// <summary>
    /// The name of the command (e.g., "monitor", "alert", "history")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Short description of what the command does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Extended help text with usage examples
    /// </summary>
    string GetHelp();

    /// <summary>
    /// Validates command arguments and options before execution
    /// </summary>
    /// <returns>Validation errors, empty list if valid</returns>
    List<string> ValidateArguments(CommandContext context);

    /// <summary>
    /// Executes the command asynchronously
    /// </summary>
    /// <param name="context">Command context with arguments and services</param>
    /// <param name="cancellationToken">Cancellation token for cooperative cancellation</param>
    Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default);
}
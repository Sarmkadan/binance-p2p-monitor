// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    Task<int> ExecuteAsync(CommandContext context);
}

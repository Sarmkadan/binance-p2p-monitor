#nullable enable

using Microsoft.Extensions.DependencyInjection;

namespace BinanceP2pMonitor.CLI;

/// <summary>
/// Extension methods for <see cref="CommandFactory"/> providing additional convenience functionality
/// </summary>
public static class CommandFactoryExtensions
{
    /// <summary>
    /// Creates a command instance by name with case-insensitive comparison
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <param name="commandName">The command name (case-insensitive)</param>
    /// <returns>The created command instance, or null if not found</returns>
    public static ICommand? CreateCommand(this CommandFactory factory, ReadOnlySpan<char> commandName)
    {
        return factory.CreateCommand(commandName.ToString());
    }

    /// <summary>
    /// Checks if any of the provided command names are registered
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <param name="commandNames">Collection of command names to check</param>
    /// <returns>True if any command name is registered</returns>
    public static bool IsAnyCommandRegistered(this CommandFactory factory, IEnumerable<string> commandNames)
    {
        foreach (var name in commandNames)
        {
            if (factory.IsCommandRegistered(name))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets all registered command names as a HashSet for efficient lookups
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <returns>A HashSet containing all registered command names</returns>
    public static HashSet<string> GetAvailableCommandsSet(this CommandFactory factory)
    {
        return new HashSet<string>(factory.GetAvailableCommands(), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Attempts to create a command instance
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <param name="commandName">The command name to check</param>
    /// <returns>True if the command is registered</returns>
    public static bool IsCommandAvailable(this CommandFactory factory, string commandName)
    {
        return factory.IsCommandRegistered(commandName);
    }

    /// <summary>
    /// Gets the number of registered commands
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <returns>The count of registered commands</returns>
    public static int GetCommandCount(this CommandFactory factory)
    {
        return factory.GetAvailableCommands().Count;
    }

    /// <summary>
    /// Attempts to create a command instance
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <param name="commandName">The command name to create</param>
    /// <param name="command">Output parameter containing the created command, or null if not found</param>
    /// <returns>True if the command was successfully created</returns>
    public static bool TryCreateCommand(this CommandFactory factory, string commandName, out ICommand? command)
    {
        command = factory.CreateCommand(commandName);
        return command is not null;
    }

    /// <summary>
    /// Gets the first available command name that matches any of the provided patterns
    /// </summary>
    /// <param name="factory">The command factory</param>
    /// <param name="commandPatterns">Collection of command name patterns to check</param>
    /// <returns>The first matching command name, or null if none found</returns>
    public static string? FindFirstAvailableCommand(this CommandFactory factory, IEnumerable<string> commandPatterns)
    {
        foreach (var pattern in commandPatterns)
        {
            if (factory.IsCommandRegistered(pattern))
            {
                return pattern;
            }
        }
        return null;
    }
}
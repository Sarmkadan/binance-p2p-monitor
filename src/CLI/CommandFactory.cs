// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.CLI;

/// <summary>
/// Factory for creating command instances based on command names
/// </summary>
public class CommandFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandFactory> _logger;
    private readonly Dictionary<string, Type> _registeredCommands = new();

    public CommandFactory(IServiceProvider serviceProvider, ILogger<CommandFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Registers a command type for a given command name
    /// </summary>
    public void RegisterCommand(string name, Type commandType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
            throw new ArgumentException($"Type {commandType.Name} does not implement ICommand");

        _registeredCommands[name.ToLowerInvariant()] = commandType;
        _logger.LogDebug("Registered command: {CommandName} -> {CommandType}", name, commandType.Name);
    }

    /// <summary>
    /// Creates a command instance by name
    /// </summary>
    public ICommand? CreateCommand(string commandName)
    {
        var normalizedName = commandName.ToLowerInvariant();

        if (!_registeredCommands.TryGetValue(normalizedName, out var commandType))
        {
            _logger.LogWarning("Command not found: {CommandName}", commandName);
            return null;
        }

        try
        {
            var command = ActivatorUtilities.CreateInstance(_serviceProvider, commandType) as ICommand;
            return command;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create command instance for {CommandName}", commandName);
            return null;
        }
    }

    /// <summary>
    /// Gets all registered command names
    /// </summary>
    public IReadOnlyList<string> GetAvailableCommands() => _registeredCommands.Keys.ToList();

    /// <summary>
    /// Checks if a command is registered
    /// </summary>
    public bool IsCommandRegistered(string commandName)
    {
        return _registeredCommands.ContainsKey(commandName.ToLowerInvariant());
    }
}

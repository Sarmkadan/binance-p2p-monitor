#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.CLI;

/// <summary>
/// Parses command-line arguments into structured command contexts
/// </summary>
public class CommandParser
{
    private readonly ILogger<CommandParser> _logger;

    public CommandParser(ILogger<CommandParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses raw command-line arguments into a command context
    /// </summary>
    public CommandContext Parse(string[] args, IServiceProvider serviceProvider)
    {
        var context = new CommandContext
        {
            ServiceProvider = serviceProvider,
            Arguments = args
        };

        if (args.Length == 0)
        {
            context.CommandName = "help";
            return context;
        }

        context.CommandName = args[0];
        var optionsAndFlags = args.Skip(1).ToList();
        var positionalArgs = new List<string>();

        for (int i = 0; i < optionsAndFlags.Count; i++)
        {
            var arg = optionsAndFlags[i];

            if (arg.StartsWith("--"))
            {
                var keyValue = arg[2..].Split('=', 2);
                var key = keyValue[0];
                var value = keyValue.Length > 1 ? keyValue[1] : "true";
                context.Options[key] = value;
            }
            else if (arg.StartsWith("-") && arg.Length == 2)
            {
                var key = arg[1].ToString();
                if (i + 1 < optionsAndFlags.Count && !optionsAndFlags[i + 1].StartsWith("-"))
                {
                    context.Options[key] = optionsAndFlags[++i];
                }
                else
                {
                    context.Flags[key] = "true";
                }
            }
            else if (!arg.StartsWith("-") || (arg.Length > 1 && char.IsDigit(arg[1])))
            {
                positionalArgs.Add(arg);
            }
        }

        context.Arguments = positionalArgs.ToArray();
        _logger.LogDebug("Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            context.CommandName, context.Options.Count, context.Flags.Count);

        return context;
    }
}

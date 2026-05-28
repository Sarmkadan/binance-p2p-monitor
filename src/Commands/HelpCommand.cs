#nullable enable
namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to display help information
/// </summary>
public class HelpCommand : ICommand
{
    private readonly CommandFactory _commandFactory;
    private readonly ConsoleOutputWriter _output;

    public string Name => "help";
    public string Description => "Display help information";

    public HelpCommand(CommandFactory commandFactory, ConsoleOutputWriter output)
    {
        _commandFactory = commandFactory;
        _output = output;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor help [command]

Display help information about commands.

Options:
  [command]  Specific command to get help for
  -h, --help Show this help message

Examples:
  binance-p2p-monitor help
  binance-p2p-monitor help monitor
  binance-p2p-monitor monitor --help
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        return new List<string>();
    }

    public Task<int> ExecuteAsync(CommandContext context)
    {
        _output.WriteHeader("Binance P2P Monitor");

        var commandArg = context.Arguments.FirstOrDefault();

        if (!string.IsNullOrEmpty(commandArg))
        {
            var command = _commandFactory.CreateCommand(commandArg);
            if (command is not null)
            {
                Console.WriteLine(command.GetHelp());
                return Task.FromResult(0);
            }

            _output.WriteError($"Unknown command: {commandArg}");
            return Task.FromResult(1);
        }

        _output.WriteInfo("Monitor and analyze Binance P2P trading prices in real-time");
        _output.WriteBlankLine();

        _output.WriteSection("Available Commands");

        var commands = _commandFactory.GetAvailableCommands();
        foreach (var cmdName in commands.OrderBy(c => c))
        {
            var cmd = _commandFactory.CreateCommand(cmdName);
            if (cmd is not null)
            {
                Console.WriteLine($"  {cmd.Name.PadRight(15)} - {cmd.Description}");
            }
        }

        _output.WriteBlankLine();
        _output.WriteInfo("Run 'binance-p2p-monitor help <command>' for detailed help");
        _output.WriteInfo("Run 'binance-p2p-monitor <command> --help' for command-specific help");

        return Task.FromResult(0);
    }
}

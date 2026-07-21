#nullable enable
namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to display version information
/// </summary>
public sealed class VersionCommand : ICommand
{
    private readonly ConsoleOutputWriter _output;

    public string Name => "version";
    public string Description => "Display version information";

    public VersionCommand(ConsoleOutputWriter output)
    {
        _output = output;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor version

Display application version and build information.

Examples:
  binance-p2p-monitor version
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        return new List<string>();
    }

    public Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        var version = typeof(VersionCommand).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        _output.WriteHeader("Binance P2P Monitor");
        _output.WriteKeyValue("Version", version);
        _output.WriteKeyValue("Author", "Vladyslav Zaiets");
        _output.WriteKeyValue("Website", "https://sarmkadan.com");
        _output.WriteKeyValue(".NET Version", ".NET 10");
        _output.WriteKeyValue("License", "MIT");
        _output.WriteBlankLine();

        return Task.FromResult(0);
    }
}

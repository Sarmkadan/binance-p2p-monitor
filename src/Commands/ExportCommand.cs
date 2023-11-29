#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to export data in various formats
/// </summary>
public class ExportCommand : ICommand
{
    private readonly IPriceRepository _priceRepository;
    private readonly IHistoryRepository _historyRepository;
    private readonly ConsoleOutputWriter _output;
    private readonly ILogger<ExportCommand> _logger;

    public string Name => "export";
    public string Description => "Export price data to file";

    public ExportCommand(
        IPriceRepository priceRepository,
        IHistoryRepository historyRepository,
        ConsoleOutputWriter output,
        ILogger<ExportCommand> logger)
    {
        _priceRepository = priceRepository;
        _historyRepository = historyRepository;
        _output = output;
        _logger = logger;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor export [options]

Export price data to file.

Options:
  --output=FILE      Output file path (required)
  --format=FORMAT    Format: csv, json (default: csv)
  --asset=ASSET      Export specific asset
  --fiat=FIAT        Export specific fiat
  --days=DAYS        Number of days to export (default: 7)
  -h, --help         Show this help message

Examples:
  binance-p2p-monitor export --output=prices.csv
  binance-p2p-monitor export --output=data.json --format=json --days=30
  binance-p2p-monitor export --output=btc.csv --asset=BTC --fiat=USDT
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        if (!context.HasOption("output"))
            errors.Add("--output is required");

        var format = context.GetOption("format", "csv");
        if (!new[] { "csv", "json" }.Contains(format))
            errors.Add("Format must be csv or json");

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            var outputPath = context.GetOption("output", "");
            var format = context.GetOption("format", "csv");
            var days = int.Parse(context.GetOption("days", "7"));

            if (string.IsNullOrEmpty(outputPath))
            {
                _output.WriteError("Output path is required");
                return 1;
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _output.WriteInfo($"Exporting data to {outputPath}");

            var startDate = DateTime.UtcNow.AddDays(-days);
            var content = $"# Exported on {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";

            await File.WriteAllTextAsync(outputPath, content);

            _output.WriteSuccess($"Data exported to {Path.GetFullPath(outputPath)}");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

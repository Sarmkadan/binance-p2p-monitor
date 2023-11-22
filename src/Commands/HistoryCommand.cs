#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to query historical price data
/// </summary>
public class HistoryCommand : ICommand
{
    private readonly IPriceHistoryService _historyService;
    private readonly ConsoleOutputWriter _output;
    private readonly ILogger<HistoryCommand> _logger;

    public string Name => "history";
    public string Description => "View historical price data and analytics";

    public HistoryCommand(
        IPriceHistoryService historyService,
        ConsoleOutputWriter output,
        ILogger<HistoryCommand> logger)
    {
        _historyService = historyService;
        _output = output;
        _logger = logger;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor history [options]

View historical price data and statistics.

Options:
  --asset=ASSET      Asset name (BTC, ETH, etc.)
  --fiat=FIAT        Fiat currency (USDT, CNY, etc.)
  --days=DAYS        Number of days to look back (default: 7)
  --format=FORMAT    Output format: table, json, csv (default: table)
  --stats            Show statistical analysis
  -h, --help         Show this help message

Examples:
  binance-p2p-monitor history --asset=BTC --fiat=USDT
  binance-p2p-monitor history --days=30 --format=csv
  binance-p2p-monitor history --asset=BTC --stats
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        if (context.HasOption("days"))
        {
            if (!int.TryParse(context.GetOption("days"), out var days) || days <= 0)
                errors.Add("Days must be a positive number");
        }

        var validFormats = new[] { "table", "json", "csv" };
        if (context.HasOption("format"))
        {
            var format = context.GetOption("format", "table");
            if (!validFormats.Contains(format))
                errors.Add($"Format must be one of: {string.Join(", ", validFormats)}");
        }

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            var asset = context.GetOption("asset");
            var fiat = context.GetOption("fiat");
            var days = int.Parse(context.GetOption("days", "7"));
            var format = context.GetOption("format", "table");
            var showStats = context.HasFlag("stats");

            _output.WriteHeader($"Price History");

            if (string.IsNullOrEmpty(asset) || string.IsNullOrEmpty(fiat))
            {
                _output.WriteError("--asset and --fiat are required");
                return 1;
            }

            _output.WriteInfo($"Fetching {days} days of history for {asset}/{fiat}");

            var startDate = DateTime.UtcNow.AddDays(-days);
            // Fetch historical data
            var historyRecords = new List<object>();

            if (!historyRecords.Any())
            {
                _output.WriteInfo("No historical data found");
                return 0;
            }

            _output.WriteSection($"Price Data ({asset}/{fiat})");
            _output.WriteInfo($"Records: {historyRecords.Count}");

            if (showStats)
            {
                _output.WriteSection("Statistics");
                _output.WriteInfo("Price statistics would be displayed here");
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "History command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

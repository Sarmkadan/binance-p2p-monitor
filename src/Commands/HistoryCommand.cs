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
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<HistoryCommand> _logger;

    public string Name => "history";
    public string Description => "View historical price data and analytics";

    public HistoryCommand(
        IPriceHistoryService historyService,
        ConsoleOutputWriter output,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<HistoryCommand> logger)
    {
        _historyService = historyService;
        _output = output;
        _formatters = formatters;
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
            var asset = context.GetOption("asset", string.Empty);
            var fiat = context.GetOption("fiat", string.Empty);
            var daysString = context.GetOption("days", "7");
            var format = context.GetOption("format", "table");
            var showStats = context.HasFlag("stats");

            if (!int.TryParse(daysString, out int days) || days <= 0)
            {
                _output.WriteError("Invalid value for --days. Must be a positive integer.");
                return 1;
            }

            _output.WriteHeader($"Price History");

            if (string.IsNullOrEmpty(asset) || string.IsNullOrEmpty(fiat))
            {
                _output.WriteError("--asset and --fiat are required");
                return 1;
            }

            var formatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatter is null)
            {
                _output.WriteError($"Unsupported format: {format}. Available formats: {string.Join(", ", _formatters.Select(f => f.FormatType))}");
                return 1;
            }

            if (showStats)
            {
                _output.WriteSection($"Statistical Analysis ({asset}/{fiat}, last {days} days)");
                var analysis = await _historyService.GetDetailedAnalysisAsync(asset, fiat, days * 24).ConfigureAwait(false);
                _output.WriteRaw(formatter.Format(analysis));
            }
            else
            {
                _output.WriteInfo($"Fetching {days} days of history for {asset}/{fiat}...");
                var historyRecords = await _historyService.GetHistoryAsync(asset, fiat, days * 24).ConfigureAwait(false);

                if (!historyRecords.Any())
                {
                    _output.WriteInfo("No historical data found");
                    return 0;
                }

                _output.WriteSection($"Price Data ({asset}/{fiat}) - Last {days} Days");
                _output.WriteRaw(formatter.Format(historyRecords.Cast<object>()));
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

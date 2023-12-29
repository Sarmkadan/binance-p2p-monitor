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
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<ExportCommand> _logger;

    public string Name => "export";
    public string Description => "Export price data to file";

    public ExportCommand(
        IPriceRepository priceRepository,
        IHistoryRepository historyRepository,
        ConsoleOutputWriter output,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<ExportCommand> logger)
    {
        _priceRepository = priceRepository;
        _historyRepository = historyRepository;
        _output = output;
        _formatters = formatters;
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
        if (!new[] { "csv", "json" }.Contains(format, StringComparer.OrdinalIgnoreCase))
            errors.Add("Format must be csv or json");
            
        if (context.HasOption("days") && (!int.TryParse(context.GetOption("days"), out int days) || days <= 0))
            errors.Add("--days must be a positive integer");

        var hasAsset = context.HasOption("asset");
        var hasFiat = context.HasOption("fiat");

        if ((hasAsset && !hasFiat) || (!hasAsset && hasFiat))
            errors.Add("--asset and --fiat must be provided together if either is used for filtering.");

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            var outputPath = context.GetOption("output", string.Empty);
            var format = context.GetOption("format", "csv");
            var asset = context.GetOption("asset", string.Empty);
            var fiat = context.GetOption("fiat", string.Empty);
            var daysString = context.GetOption("days", "7");

            if (!int.TryParse(daysString, out int days) || days <= 0)
            {
                _output.WriteError("Invalid value for --days. Must be a positive integer.");
                return 1;
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                _output.WriteError("Output path is required.");
                return 1;
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                _output.WriteInfo($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            _output.WriteInfo($"Exporting data to {outputPath} in {format.ToUpper()} format for {days} days...");

            var startDate = DateTime.UtcNow.AddDays(-days);
            IEnumerable<PriceHistory> history;

            if (!string.IsNullOrEmpty(asset) && !string.IsNullOrEmpty(fiat))
            {
                history = await _historyRepository.GetHistoryByDateRangeAsync(asset, fiat, startDate, DateTime.UtcNow).ConfigureAwait(false);
            }
            else
            {
                // If no specific asset/fiat, get all recent history and filter by date range
                var allHistory = await _historyRepository.GetRecentHistoryAsync(days * 24 * 60).ConfigureAwait(false);
                history = allHistory.Where(h => h.RecordedAt >= startDate).ToList();
            }

            if (!history.Any())
            {
                _output.WriteInfo("No historical data found for the specified criteria.");
                return 0;
            }

            var formatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatter is null)
            {
                _output.WriteError($"Unsupported format: {format}. Available formats: {string.Join(", ", _formatters.Select(f => f.FormatType))}");
                return 1;
            }

            // Convert PriceHistory to object to allow generic formatting
            var formattedData = formatter.Format(history.Cast<object>());

            await File.WriteAllTextAsync(outputPath, formattedData).ConfigureAwait(false);

            _output.WriteSuccess($"Data exported successfully to {Path.GetFullPath(outputPath)}");
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

#nullable enable

using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to display current buy/sell spread for trading pairs
/// </summary>
public sealed class SpreadCommand : ICommand
{
    private readonly ISpreadAnalysisService _spreadAnalysisService;
    private readonly ConsoleOutputWriter _output;
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<SpreadCommand> _logger;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Gets the command name used on the command line (<c>spread</c>).
    /// </summary>
    public string Name => "spread";

    /// <summary>
    /// Gets the short description shown in the command list.
    /// </summary>
    public string Description => "Display current buy/sell spread for trading pairs";

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadCommand"/> class.
    /// </summary>
    /// <param name="spreadAnalysisService">Service that provides spread analysis data.</param>
    /// <param name="output">Writer used for console output.</param>
    /// <param name="formatters">Available output formatters, selected by the <c>--format</c> option.</param>
    /// <param name="logger">Logger used to record command failures.</param>
    /// <param name="appSettings">Application settings used for the configuration summary.</param>
    /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
    public SpreadCommand(
        ISpreadAnalysisService spreadAnalysisService,
        ConsoleOutputWriter output,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<SpreadCommand> logger,
        AppSettings appSettings)
    {
        _spreadAnalysisService = spreadAnalysisService ?? throw new ArgumentNullException(nameof(spreadAnalysisService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _formatters = formatters ?? throw new ArgumentNullException(nameof(formatters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    /// <summary>
    /// Returns the usage text for the <c>spread</c> command.
    /// </summary>
    /// <returns>The help text describing usage, options and examples.</returns>
    public string GetHelp()
    {
        return @"Usage: binance-p2p-monitor spread [options]

Display current buy/sell spread for trading pairs.

Options:
--asset=ASSET Show spread for specific asset only
--fiat=FIAT Show spread for specific fiat currency only
--format=FORMAT Output format: table, json, markdown (default: table)
--pair=PAIR Show spread for specific pair (format: ASSET/FIAT)
-h, --help Show this help message

Examples:
binance-p2p-monitor spread
binance-p2p-monitor spread --asset=BTC
binance-p2p-monitor spread --fiat=USD
binance-p2p-monitor spread --pair=BTC/USD
binance-p2p-monitor spread --format=json
";
    }

    /// <summary>
    /// Validates the command-line arguments before execution.
    /// </summary>
    /// <param name="context">The parsed command context.</param>
    /// <returns>A list of validation error messages; empty when the arguments are valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public List<string> ValidateArguments(CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var errors = new List<string>();
        var validFormats = new[] { "table", "json", "markdown" };

        if (context.HasOption("format") && !validFormats.Contains(context.GetOption("format"), StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"--format must be one of: {string.Join(", ", validFormats)}");
        }

        return errors;
    }

    /// <summary>
    /// Executes the spread command, printing spreads for the requested pair or all pairs, with optional asset and fiat filters.
    /// </summary>
    /// <param name="context">The parsed command context containing options.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>0 on success or when there is no data to show; 1 on invalid input or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _output.WriteHeader("Buy/Sell Spread Analysis");

        try
        {
            var assetFilter = context.GetOption("asset");
            var fiatFilter = context.GetOption("fiat");
            var pairFilter = context.GetOption("pair");
            var format = context.GetOption("format", "table");

            var formatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatter is null)
            {
                _output.WriteError($"Unsupported format: {format}. Available formats: {string.Join(", ", _formatters.Select(f => f.FormatType))}");
                return 1;
            }

            var spreads = new List<object>();

            if (!string.IsNullOrEmpty(pairFilter))
            {
                // Parse pair filter (format: ASSET/FIAT)
                var pairParts = pairFilter.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (pairParts.Length == 2)
                {
                    var pairAsset = pairParts[0].Trim();
                    var pairFiat = pairParts[1].Trim();

                    var spread = await _spreadAnalysisService.GetSpreadAnalysisAsync(pairAsset, pairFiat).ConfigureAwait(false);
                    if (spread != null)
                    {
                        spreads.Add(CreateSpreadDisplay(spread));
                    }
                    else
                    {
                        _output.WriteWarning($"No spread data available for {pairAsset}/{pairFiat}");
                        return 0;
                    }
                }
                else
                {
                    _output.WriteError("Invalid pair format. Use ASSET/FIAT (e.g., BTC/USD)");
                    return 1;
                }
            }
            else
            {
                // Get all spreads and apply filters
                var allSpreadsDict = await _spreadAnalysisService.GetAllSpreadsAsync().ConfigureAwait(false);

                foreach (var spread in allSpreadsDict.Values)
                {
                    // Apply asset filter
                    if (!string.IsNullOrEmpty(assetFilter) && !spread.Asset.Equals(assetFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Apply fiat filter
                    if (!string.IsNullOrEmpty(fiatFilter) && !spread.Fiat.Equals(fiatFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    spreads.Add(CreateSpreadDisplay(spread));
                }
            }

            if (!spreads.Any())
            {
                _output.WriteInfo("No spread data available");
                return 0;
            }

            _output.WriteRaw(formatter.Format(spreads));
            _output.WriteBlankLine();

            // Show configuration summary
            _output.WriteSection("Configuration");
            _output.WriteKeyValue("Default Spread Threshold", $"{_appSettings.DefaultSpreadThreshold}%");
            _output.WriteKeyValue("Spread Analysis History", $"{_appSettings.SpreadAnalysisHistoryHours} hours");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spread command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private object CreateSpreadDisplay(Spread spread)
    {
        var riskLevel = spread.GetRiskLevel();
        var variance = spread.GetVarianceFromAverage();
        var isHigh = spread.IsHighSpread();
        var isLow = spread.IsLowSpread();

        return new
        {
            Asset = spread.Asset,
            Fiat = spread.Fiat,
            Pair = $"{spread.Asset}/{spread.Fiat}",
            CurrentSpread = $"{spread.CurrentSpreadPercent:F4}%",
            AverageSpread = $"{spread.AverageSpreadPercent:F4}%",
            MinSpread = $"{spread.MinSpreadPercent:F4}%",
            MaxSpread = $"{spread.MaxSpreadPercent:F4}%",
            StdDev = $"{spread.StandardDeviation:F4}%",
            SampleCount = spread.SampleCount,
            RiskLevel = riskLevel,
            VarianceFromAverage = $"{variance:+0.00;-0.00;0}%",
            IsHigh = isHigh ? "⚠️ Yes" : "✅ No",
            IsLow = isLow ? "✅ Yes" : "❌ No",
            LastUpdated = spread.LastUpdatedAt.GetTimeAgoString()
        };
    }
}
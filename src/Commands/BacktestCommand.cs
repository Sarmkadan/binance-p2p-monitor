#nullable enable
using BinanceP2pMonitor.Backtesting;
using BinanceP2pMonitor.Extensions;
using BinanceP2pMonitor.Formatters;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// CLI command that runs the strategy backtester and optional Monte Carlo simulation
/// over historical P2P price data, then renders the results to the console.
/// </summary>
public sealed class BacktestCommand : ICommand
{
    private readonly IBacktestingService _backtestingService;
    private readonly ConsoleOutputWriter _output;
    private readonly AppSettings _settings;
    private readonly ILogger<BacktestCommand> _logger;
    private readonly TableOutputFormatter _tableFormatter;

    /// <inheritdoc />
    public string Name => "backtest";

    /// <inheritdoc />
    public string Description =>
        "Replay a spread-momentum strategy over historical P2P data with optional Monte Carlo risk projection";

    /// <summary>
    /// Initialises the command with its required service dependencies.
    /// </summary>
    public BacktestCommand(
        IBacktestingService backtestingService,
        ConsoleOutputWriter output,
        AppSettings settings,
        ILogger<BacktestCommand> logger,
        TableOutputFormatter tableFormatter)
    {
        _backtestingService = backtestingService ?? throw new ArgumentNullException(nameof(backtestingService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tableFormatter = tableFormatter ?? throw new ArgumentNullException(nameof(tableFormatter));
    }

    /// <inheritdoc />
    public string GetHelp() => @"
Usage: binance-p2p-monitor backtest [options]

Replay a spread-momentum trading strategy over recorded P2P price history.
Optionally follows the deterministic replay with a Monte Carlo simulation to
produce probabilistic outcome projections (VaR, CVaR, confidence intervals).

Required options:
  --asset=ASSET              Crypto asset to backtest (e.g. BTC, ETH, USDT)
  --fiat=FIAT                Fiat denomination (e.g. USDT, UAH, USD)

Strategy options:
  --hours=N                  Lookback window in hours (default: 720 = 30 days)
  --equity=AMOUNT            Starting portfolio equity (default: 10000)
  --position-size=FRACTION   Fraction of equity per trade, 0–1 (default: 0.10)
  --entry=SPREAD%            Entry spread threshold in % (default: 1.0)
  --exit=SPREAD%             Exit spread threshold in % (default: 0.3)
  --stop-loss=PCT%           Stop-loss percentage (default: 2.0)
  --take-profit=PCT%         Take-profit percentage (default: 4.0)
  --tx-cost=PCT%             Round-trip transaction cost % (default: 0.2)
  --lookback=N               Bars used for MA momentum filter (default: 50)

Monte Carlo options:
  --monte-carlo              Run Monte Carlo simulation after backtest
  --mc-iterations=N          Number of simulation paths (default: 1000)
  --confidence=LEVEL         Confidence level, 0–1 (default: 0.95)
  --vol-scale=FACTOR         Volatility scaling factor (default: 1.0)
  --seed=N                   Fixed RNG seed for reproducible runs

Output options:
  --format=FORMAT            Output format: summary, json, table (default: summary)
  --signals                  Print the generated trade signal log
  -h, --help                 Show this help message

Examples:
  binance-p2p-monitor backtest --asset=BTC --fiat=USDT
  binance-p2p-monitor backtest --asset=ETH --fiat=USDT --hours=1440 --monte-carlo
  binance-p2p-monitor backtest --asset=BTC --fiat=USDT --entry=1.5 --stop-loss=3 --monte-carlo --mc-iterations=5000
";

    /// <inheritdoc />
    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        if (!context.HasOption("asset"))
            errors.Add("--asset is required");

        if (!context.HasOption("fiat"))
            errors.Add("--fiat is required");

        if (context.HasOption("hours") &&
            (!int.TryParse(context.GetOption("hours"), out var h) || h < 1))
            errors.Add("--hours must be a positive integer");

        if (context.HasOption("equity") &&
            (!decimal.TryParse(context.GetOption("equity"), out var eq) || eq <= 0))
            errors.Add("--equity must be a positive number");

        if (context.HasOption("position-size") &&
            (!decimal.TryParse(context.GetOption("position-size"), out var ps) || ps <= 0 || ps > 1))
            errors.Add("--position-size must be in (0, 1]");

        if (context.HasOption("mc-iterations") &&
            (!int.TryParse(context.GetOption("mc-iterations"), out var mci) || mci < 10))
            errors.Add("--mc-iterations must be at least 10");

        if (context.HasOption("confidence") &&
            (!decimal.TryParse(context.GetOption("confidence"), out var cl) || cl <= 0 || cl >= 1))
            errors.Add("--confidence must be in (0, 1)");

        var validFormats = new[] { "summary", "json", "table" };
        if (context.HasOption("format") && !validFormats.Contains(context.GetOption("format")))
            errors.Add($"--format must be one of: {string.Join(", ", validFormats)}");

        return errors;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(CommandContext context)
    {
        var asset = context.GetOption("asset")!;
        var fiat = context.GetOption("fiat")!;
        var hours = ParseInt(context, "hours", 720);
        var format = context.GetOption("format", "summary");
        var runMc = context.HasOption("monte-carlo");
        var showSignals = context.HasOption("signals");

        var options = new BacktestOptions
        {
            InitialEquity = ParseDecimal(context, "equity", 10_000m),
            PositionSizeFraction = ParseDecimal(context, "position-size", 0.10m),
            EntrySpreadThreshold = ParseDecimal(context, "entry", 1.0m),
            ExitSpreadThreshold = ParseDecimal(context, "exit", 0.3m),
            StopLossPercent = ParseDecimal(context, "stop-loss", 2.0m),
            TakeProfitPercent = ParseDecimal(context, "take-profit", 4.0m),
            TransactionCostPercent = ParseDecimal(context, "tx-cost", 0.2m),
            LookbackPeriod = ParseInt(context, "lookback", 50),
            MonteCarloIterations = ParseInt(context, "mc-iterations", 1_000),
            ConfidenceLevel = ParseDecimal(context, "confidence", 0.95m),
            VolatilityScaleFactor = ParseDecimal(context, "vol-scale", 1.0m),
            RandomSeed = context.HasOption("seed")
                ? int.Parse(context.GetOption("seed")!)
                : null
        };

        _output.WriteHeader($"Strategy Backtester — {asset}/{fiat}");
        _output.WriteInfo($"Window: {hours}h | Equity: {options.InitialEquity:F2} | " +
                          $"Entry: {options.EntrySpreadThreshold}% | Exit: {options.ExitSpreadThreshold}%");
        if (runMc)
            _output.WriteInfo($"Monte Carlo: {options.MonteCarloIterations:N0} paths @ {options.ConfidenceLevel:P0} CL");

        _output.WriteBlankLine();

        try
        {
            BacktestResult result = runMc
                ? await _backtestingService.RunBacktestWithMonteCarloAsync(asset, fiat, options, hours)
                : await _backtestingService.RunBacktestAsync(asset, fiat, options, hours).ConfigureAwait(false);

            if (format == "json")
            {
                _output.WriteRaw(JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
            else if (format == "table")
            {
                var summary = BacktestingEngine.CreateSummaryReport(result);
                _output.WriteRaw(_tableFormatter.Format(summary));
            }
            else
            {
                _output.WriteRaw(result.ToSummaryString());
            }

            if (showSignals)
            {
                _output.WriteBlankLine();
                _output.WriteSection($"Trade Log ({result.TotalTrades} trades)");
                foreach (var trade in result.Trades)
                {
                    var sign = trade.NetPnL >= 0 ? "+" : "";
                    _output.WriteInfo(
                        $"  #{trade.Id:D3} {trade.EntryTime:MM-dd HH:mm} → {trade.ExitTime:MM-dd HH:mm} | " +
                        $"{trade.CloseReason,-14} | NetPnL: {sign}{trade.NetPnL:F4} | " +
                        $"Return: {sign}{trade.ReturnPercent:F2}%");
                }
            }

            _output.WriteBlankLine();
            _output.WriteSuccess("Backtest complete.");
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Backtest could not run for {Asset}/{Fiat}", asset, fiat);
            _output.WriteError($"Cannot run backtest: {ex.Message}");
            return 2;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Backtest command failed for {Asset}/{Fiat}", asset, fiat);
            _output.WriteError($"Backtest failed: {ex.Message}");
            return 1;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static decimal ParseDecimal(CommandContext ctx, string key, decimal fallback) =>
        ctx.HasOption(key) && decimal.TryParse(ctx.GetOption(key), out var v) ? v : fallback;

    private static int ParseInt(CommandContext ctx, string key, int fallback) =>
        ctx.HasOption(key) && int.TryParse(ctx.GetOption(key), out var v) ? v : fallback;
}

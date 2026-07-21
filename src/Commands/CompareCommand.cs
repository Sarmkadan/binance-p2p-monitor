#nullable enable

using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to compare buy/sell prices for the same asset across two fiat currencies
/// </summary>
public sealed class CompareCommand : ICommand
{
    private readonly IPriceMonitoringService _priceService;
    private readonly ConsoleOutputWriter _output;
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<CompareCommand> _logger;
    private readonly AppSettings _appSettings;

    public string Name => "compare";
    public string Description => "Compare buy/sell prices for the same asset across two fiat currencies";

    public CompareCommand(
        IPriceMonitoringService priceService,
        ConsoleOutputWriter output,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<CompareCommand> logger,
        AppSettings appSettings)
    {
        _priceService = priceService ?? throw new ArgumentNullException(nameof(priceService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _formatters = formatters ?? throw new ArgumentNullException(nameof(formatters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
    }

    public string GetHelp()
    {
        return @"Usage: binance-p2p-monitor compare [options]

Compare buy/sell prices for the same asset across two fiat currencies side by side.

Options:
--asset=ASSET Asset to compare (e.g., BTC, ETH, USDT)
--from=FIAT First fiat currency (required)
--to=FIAT Second fiat currency (required)
--format=FORMAT Output format: table, json, markdown (default: table)
-h, --help Show this help message

Examples:
binance-p2p-monitor compare --asset=BTC --from=USD --to=USDT
binance-p2p-monitor compare --asset=ETH --from=EUR --to=GBP
binance-p2p-monitor compare --asset=USDT --from=USD --to=CNY --format=json
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        var validFormats = new[] { "table", "json", "markdown" };
        if (context.HasOption("format") && !validFormats.Contains(context.GetOption("format"), StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"--format must be one of: {string.Join(", ", validFormats)}");
        }

        if (!context.HasOption("asset"))
        {
            errors.Add("--asset is required");
        }

        if (!context.HasOption("from"))
        {
            errors.Add("--from is required");
        }

        if (!context.HasOption("to"))
        {
            errors.Add("--to is required");
        }

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        _output.WriteHeader("Price Comparison");

        try
        {
            var asset = context.GetOption("asset");
            var fromFiat = context.GetOption("from");
            var toFiat = context.GetOption("to");
            var format = context.GetOption("format", "table");

            var formatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatter is null)
            {
                _output.WriteError($"Unsupported format: {format}. Available formats: {string.Join(", ", _formatters.Select(f => f.FormatType))}");
                return 1;
            }

            _output.WriteInfo($"Comparing {asset} prices across {fromFiat} and {toFiat}");
            _output.WriteBlankLine();

            // Get prices for both currency pairs
            var priceFrom = await _priceService.GetCurrentPriceAsync(asset, fromFiat).ConfigureAwait(false);
            var priceTo = await _priceService.GetCurrentPriceAsync(asset, toFiat).ConfigureAwait(false);

            if (priceFrom is null || priceTo is null)
            {
                _output.WriteError("Could not retrieve price data for one or both currency pairs");
                return 1;
            }

            var comparisonData = new List<object> { CreateComparisonDisplay(asset, priceFrom, priceTo) };

            _output.WriteRaw(formatter.Format(comparisonData));
            _output.WriteBlankLine();

            // Show price difference analysis
            _output.WriteSection("Analysis");
            var priceDiffPercent = CalculatePriceDifferencePercent(priceFrom.BuyPrice, priceTo.BuyPrice);
            var priceDiffAbs = priceFrom.BuyPrice - priceTo.BuyPrice;

            _output.WriteKeyValue("Price Difference (Buy)", $"{priceDiffAbs:F8} {fromFiat} ({priceDiffPercent:+0.00;-0.00;0}%)");
            _output.WriteKeyValue("Buy Price Ratio", $"{priceFrom.BuyPrice / priceTo.BuyPrice:F6} {fromFiat}/{toFiat}");
            _output.WriteKeyValue("Sell Price Ratio", $"{priceFrom.SellPrice / priceTo.SellPrice:F6} {fromFiat}/{toFiat}");
            _output.WriteKeyValue("Best Buy Location", priceFrom.BuyPrice < priceTo.BuyPrice ? $"{fromFiat}" : $"{toFiat}");
            _output.WriteKeyValue("Best Sell Location", priceFrom.SellPrice > priceTo.SellPrice ? $"{fromFiat}" : $"{toFiat}");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compare command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private object CreateComparisonDisplay(string asset, Price priceFrom, Price priceTo)
    {
        var buyDiffPercent = CalculatePriceDifferencePercent(priceFrom.BuyPrice, priceTo.BuyPrice);
        var sellDiffPercent = CalculatePriceDifferencePercent(priceFrom.SellPrice, priceTo.SellPrice);

        var buyDiffAbs = priceFrom.BuyPrice - priceTo.BuyPrice;
        var sellDiffAbs = priceFrom.SellPrice - priceTo.SellPrice;

        return new
        {
            Asset = asset,
            FromFiat = priceFrom.Fiat,
            ToFiat = priceTo.Fiat,

            // From currency pair
            FromBuyPrice = priceFrom.BuyPrice.ToString("F8"),
            FromSellPrice = priceFrom.SellPrice.ToString("F8"),
            FromSpread = $"{priceFrom.CalculateSpread():F4}%",
            FromChange = priceFrom.BuyChangePercent.ToString("+0.00;-0.00;0") + "%",

            // To currency pair
            ToBuyPrice = priceTo.BuyPrice.ToString("F8"),
            ToSellPrice = priceTo.SellPrice.ToString("F8"),
            ToSpread = $"{priceTo.CalculateSpread():F4}%",
            ToChange = priceTo.BuyChangePercent.ToString("+0.00;-0.00;0") + "%",

            // Differences
            BuyPriceDifference = $"{buyDiffAbs:F8} {priceFrom.Fiat} ({buyDiffPercent:+0.00;-0.00;0}%)",
            SellPriceDifference = $"{sellDiffAbs:F8} {priceFrom.Fiat} ({sellDiffPercent:+0.00;-0.00;0}%)",

            LastUpdated = DateTime.Now.ToString("HH:mm:ss")
        };
    }

    private decimal CalculatePriceDifferencePercent(decimal price1, decimal price2)
    {
        if (price2 == 0) return 0;
        return ((price1 - price2) / price2) * 100;
    }
}
#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Formatters;
using BinanceP2pMonitor.Repositories;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to display daily price summary
/// </summary>
public sealed class SummaryCommand : ICommand
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IPriceRepository _priceRepository;
    private readonly AppSettings _appSettings;
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<SummaryCommand> _logger;

    public string Name => "summary";
    public string Description => "Display daily price summary";

    public SummaryCommand(
        IHistoryRepository historyRepository,
        IPriceRepository priceRepository,
        AppSettings appSettings,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<SummaryCommand> logger)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _formatters = formatters ?? throw new ArgumentNullException(nameof(formatters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GetHelp()
    {
        return @"  Usage: binance-p2p-monitor summary [options]

  Display daily price summary with min, max, average, and current prices.

  Options:
  --format=FORMAT Output format: table, json, markdown (default: table)
  -h, --help     Show this help message

  Examples:
  binance-p2p-monitor summary
  binance-p2p-monitor summary --format=json
binance-p2p-monitor summary --format=markdown
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

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            var format = context.GetOption("format", "table");
            var formatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatter is null)
            {
                Console.Error.WriteLine($"Unsupported format: {format}. Available formats: {string.Join(", ", _formatters.Select(f => f.FormatType))}");
                return 1;
            }

            var dailySummary = await GetDailySummaryAsync().ConfigureAwait(false);

            if (dailySummary is null || !dailySummary.Assets.Any())
            {
                Console.WriteLine("No price history available for daily summary");
                return 0;
            }

            Console.WriteLine(formatter.Format(dailySummary.Assets));

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Summary command failed");
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<DailySummary> GetDailySummaryAsync()
    {
        var dailySummary = new DailySummary
        {
            Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Window = "24h",
            Timezone = "UTC",
            Assets = new List<Asset>()
        };

        bool hasData = false;

        foreach (var asset in _appSettings.MonitoredAssets)
        {
            var assetData = new Asset
            {
                Symbol = asset,
                Fiat = new List<Fiat>()
            };

            foreach (var fiat in _appSettings.MonitoredFiats)
            {
                try
                {
                    var history = (await _historyRepository
                        .GetHistoryByAssetAndFiatAsync(asset, fiat, 24)
                        .ConfigureAwait(false)).ToList();

                    if (history.Count == 0)
                    {
                        continue;
                    }

                    hasData = true;

                    var buyPrices = history.Select(h => h.BuyPrice).ToList();
                    var sellPrices = history.Select(h => h.SellPrice).ToList();
                    var allPrices = buyPrices.Concat(sellPrices).ToList();

                    var minPrice = allPrices.Min();
                    var maxPrice = allPrices.Max();
                    var avgPrice = allPrices.Average();

                    var latest = await _priceRepository
                        .GetLatestByAssetAndFiatAsync(asset, fiat)
                        .ConfigureAwait(false);

                    var currentBuy = latest?.BuyPrice ?? buyPrices.Last();
                    var currentSell = latest?.SellPrice ?? sellPrices.Last();

                    assetData.Fiat.Add(new Fiat
                    {
                        Symbol = fiat,
                        MinBuyPrice = minPrice,
                        MaxBuyPrice = maxPrice,
                        AvgBuyPrice = avgPrice,
                        CurrentBuyPrice = currentBuy,
                        MinSellPrice = minPrice,
                        MaxSellPrice = maxPrice,
                        AvgSellPrice = avgPrice,
                        CurrentSellPrice = currentSell
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to collect summary data for {Asset}/{Fiat}", asset, fiat);
                }
            }

            if (assetData.Fiat.Any())
            {
                dailySummary.Assets.Add(assetData);
            }
        }

        if (!hasData)
        {
            return null;
        }

        return dailySummary;
    }
}

public class DailySummary
{
    public string Date { get; set; }
    public string Window { get; set; }
    public string Timezone { get; set; }
    public List<Asset> Assets { get; set; } = new();
}

public class Asset
{
    public string Symbol { get; set; } = string.Empty;
    public List<Fiat> Fiat { get; set; } = new();
}

public class Fiat
{
    public string Symbol { get; set; } = string.Empty;
    public decimal MinBuyPrice { get; set; }
    public decimal MaxBuyPrice { get; set; }
    public decimal AvgBuyPrice { get; set; }
    public decimal CurrentBuyPrice { get; set; }
    public decimal MinSellPrice { get; set; }
    public decimal MaxSellPrice { get; set; }
    public decimal AvgSellPrice { get; set; }
    public decimal CurrentSellPrice { get; set; }
}
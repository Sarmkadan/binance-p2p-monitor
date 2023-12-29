#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to start real-time price monitoring
/// </summary>
public class MonitorCommand : ICommand
{
    private readonly IPriceMonitoringService _priceService;
    private readonly ISpreadAnalysisService _spreadService;
    private readonly IEventBus _eventBus;
    private readonly ConsoleOutputWriter _output;
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<MonitorCommand> _logger;
    private readonly AppSettings _appSettings;

    public string Name => "monitor";
    public string Description => "Start real-time price monitoring for configured assets";

    public MonitorCommand(
        IPriceMonitoringService priceService,
        ISpreadAnalysisService spreadService,
        IEventBus eventBus,
        ConsoleOutputWriter output,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<MonitorCommand> logger,
        AppSettings appSettings)
    {
        _priceService = priceService;
        _spreadService = spreadService;
        _eventBus = eventBus;
        _output = output;
        _formatters = formatters;
        _logger = logger;
        _appSettings = appSettings;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor monitor [options]

Start monitoring P2P prices in real-time with alerts.

Options:
  --asset=ASSET        Monitor specific asset (e.g., BTC, ETH)
  --fiat=FIAT          Monitor specific fiat currency (e.g., USDT, CNY)
  --interval=SECONDS   Check interval in seconds (default: 30)
  --format=FORMAT      Output format: table, json, csv (default: table)
  -v, --verbose        Enable verbose logging
  -h, --help           Show this help message

Examples:
  binance-p2p-monitor monitor
  binance-p2p-monitor monitor --asset=BTC --fiat=USDT
  binance-p2p-monitor monitor --interval=60 --format=json
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        if (context.HasOption("interval"))
        {
            if (!int.TryParse(context.GetOption("interval"), out var interval) || interval < 5)
                errors.Add("Interval must be a number >= 5");
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
        _output.WriteHeader("P2P Price Monitor");

        var interval = int.Parse(context.GetOption("interval", _appSettings.MonitoringIntervalSeconds.ToString()));
        var assetFilter = context.GetOption("asset");
        var fiatFilter = context.GetOption("fiat");
        var format = context.GetOption("format", "table");

        var tableFormatter = _formatters.FirstOrDefault(f => f.FormatType.Equals("table", StringComparison.OrdinalIgnoreCase)) as TableOutputFormatter;
        var specificFormatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));

        if (tableFormatter is null || specificFormatter is null)
        {
            _output.WriteError("Could not find required output formatters.");
            return 1;
        }

        _output.WriteInfo($"Starting monitoring with {interval}s interval");
        if (!string.IsNullOrEmpty(assetFilter) && !string.IsNullOrEmpty(fiatFilter))
            _output.WriteInfo($"Monitoring {assetFilter}/{fiatFilter}");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            _eventBus.Subscribe<PriceUpdatedEvent>(async (@event, ct) =>
            {
                // This event is primarily for internal logic, not direct console output
                // Console output is handled by the periodic refresh in the loop below
                await Task.CompletedTask;
            });

            _eventBus.Subscribe<SpreadAlertTriggeredEvent>(async (@event, ct) =>
            {
                _output.WriteWarning($"Spread Alert: {@event.Asset}/{@event.Fiat} - {(@event.SpreadPercentage):F2}%");
                await Task.CompletedTask;
            });

            _output.WriteBlankLine();
            _output.WriteInfo("Press Ctrl+C to stop monitoring");
            
            // Explicitly start the monitoring service which subscribes to WebSockets
            await _priceService.StartMonitoringAsync(cts.Token);

            while (!cts.Token.IsCancellationRequested)
            {
                Console.Clear();
                _output.WriteHeader("P2P Price Monitor - Live Data");
                _output.WriteInfo($"Last Updated: {DateTime.Now:HH:mm:ss}");
                _output.WriteBlankLine();

                var prices = await _priceService.GetAllCurrentPricesAsync().ConfigureAwait(false);
                var displayData = new List<object>();

                foreach (var price in prices)
                {
                    if ((!string.IsNullOrEmpty(assetFilter) && !price.Asset.Equals(assetFilter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(fiatFilter) && !price.Fiat.Equals(fiatFilter, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var spread = await _spreadService.GetSpreadAnalysisAsync(price.Asset, price.Fiat).ConfigureAwait(false);

                    displayData.Add(new
                    {
                        price.Asset,
                        price.Fiat,
                        BuyPrice = price.BuyPrice.ToString("F8"),
                        SellPrice = price.SellPrice.ToString("F8"),
                        Spread = spread?.CurrentSpreadPercent.ToString("F2") + "%" ?? "N/A",
                        Change = price.BuyChangePercent.ToString("+0.00;-0.00;0") + "%",
                        LastUpdate = price.UpdatedAt.ToString("HH:mm:ss")
                    });
                }

                if (displayData.Any())
                {
                    if (format.Equals("table", StringComparison.OrdinalIgnoreCase))
                    {
                        _output.WriteRaw(tableFormatter.Format(displayData));
                    }
                    else
                    {
                        _output.WriteRaw(specificFormatter.Format(displayData));
                    }
                }
                else
                {
                    _output.WriteInfo("No data to display. Ensure assets and fiats are configured and monitoring is active.");
                }

                _output.WriteBlankLine();
                _output.WriteInfo("Press Ctrl+C to stop monitoring");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(interval), cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            
            await _priceService.StopMonitoringAsync();
            _output.WriteSuccess("Monitoring stopped");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Monitor command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

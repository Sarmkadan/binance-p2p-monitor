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
    private readonly ILogger<MonitorCommand> _logger;
    private readonly AppSettings _appSettings;

    public string Name => "monitor";
    public string Description => "Start real-time price monitoring for configured assets";

    public MonitorCommand(
        IPriceMonitoringService priceService,
        ISpreadAnalysisService spreadService,
        IEventBus eventBus,
        ConsoleOutputWriter output,
        ILogger<MonitorCommand> logger,
        AppSettings appSettings)
    {
        _priceService = priceService;
        _spreadService = spreadService;
        _eventBus = eventBus;
        _output = output;
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
        var asset = context.GetOption("asset");
        var fiat = context.GetOption("fiat");

        _output.WriteInfo($"Starting monitoring with {interval}s interval");
        if (!string.IsNullOrEmpty(asset) && !string.IsNullOrEmpty(fiat))
            _output.WriteInfo($"Monitoring {asset}/{fiat}");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            // Subscribe to price updates
            _eventBus.Subscribe<PriceUpdatedEvent>(async (@event, ct) =>
            {
                _output.WriteSection($"Price Update: {@event.Asset}/{@event.Fiat}");
                _output.WriteKeyValue("Buy Price", $"{@event.BuyPrice:F8}");
                _output.WriteKeyValue("Sell Price", $"{@event.SellPrice:F8}");
                _output.WriteKeyValue("Buy Offers", @event.BuyOfferCount.ToString());
                _output.WriteKeyValue("Sell Offers", @event.SellOfferCount.ToString());

                if (@event.PreviousBuyPrice > 0)
                {
                    var changePercent = ((@event.BuyPrice - @event.PreviousBuyPrice) / @event.PreviousBuyPrice * 100);
                    _output.WriteKeyValue("Change", $"{changePercent:+0.00;-0.00;0}%");
                }

                await Task.CompletedTask;
            });

            // Subscribe to spread alerts
            _eventBus.Subscribe<SpreadAlertTriggeredEvent>(async (@event, ct) =>
            {
                _output.WriteWarning($"Spread Alert: {@event.Asset}/{@event.Fiat} - {(@event.SpreadPercentage):F2}%");
                await Task.CompletedTask;
            });

            _output.WriteBlankLine();
            _output.WriteInfo("Press Ctrl+C to stop monitoring");

            // Monitor loop
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(interval), cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

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

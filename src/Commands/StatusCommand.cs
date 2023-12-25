#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to display current prices and system status
/// </summary>
public class StatusCommand : ICommand
{
    private readonly IPriceMonitoringService _priceService;
    private readonly IAlertService _alertService;
    private readonly ConsoleOutputWriter _output;
    private readonly ILogger<StatusCommand> _logger;
    private readonly AppSettings _appSettings;

    public string Name => "status";
    public string Description => "Display current prices and system status";

    public StatusCommand(
        IPriceMonitoringService priceService,
        IAlertService alertService,
        ConsoleOutputWriter output,
        ILogger<StatusCommand> logger,
        AppSettings appSettings)
    {
        _priceService = priceService;
        _alertService = alertService;
        _output = output;
        _logger = logger;
        _appSettings = appSettings;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor status [options]

Display current market prices and system status.

Options:
  --asset=ASSET    Show specific asset only
  --format=FORMAT  Output format: table, json (default: table)
  -h, --help       Show this help message

Examples:
  binance-p2p-monitor status
  binance-p2p-monitor status --asset=BTC
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        return new List<string>();
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        _output.WriteHeader("System Status");

        try
        {
            _output.WriteSection("Configuration");
            _output.WriteKeyValue("Monitoring Interval", $"{_appSettings.MonitoringIntervalSeconds}s");
            _output.WriteKeyValue("Alert Cooldown", $"{_appSettings.AlertCooldownMinutes}m");
            _output.WriteKeyValue("WebSocket Enabled", _appSettings.EnableWebSocket ? "Yes" : "No");
            _output.WriteKeyValue("Telegram Enabled", _appSettings.EnableTelegramNotifications ? "Yes" : "No");

            _output.WriteSection("Current Prices");

            var prices = await _priceService.GetAllCurrentPricesAsync().ConfigureAwait(false);
            var priceList = prices.ToList();

            if (!priceList.Any())
            {
                _output.WriteInfo("No prices available");
                return 0;
            }

            var rows = priceList.Select(p => new Dictionary<string, string>
            {
                { "Asset", p.Asset },
                { "Fiat", p.Fiat },
                { "Buy", p.BuyPrice.ToString("F8") },
                { "Sell", p.SellPrice.ToString("F8") },
                { "Spread", ((p.SellPrice - p.BuyPrice) / p.BuyPrice * 100).ToString("F2") + "%" },
                { "Updated", p.Timestamp.GetTimeAgoString() }
            }).ToList();

            _output.WriteTable(rows);

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Status command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

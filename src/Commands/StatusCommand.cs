#nullable enable
namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to display current prices and system status
/// </summary>
public class StatusCommand : ICommand
{
    private readonly IPriceMonitoringService _priceService;
    private readonly IAlertService _alertService;
    private readonly ISpreadAnalysisService _spreadAnalysisService;
    private readonly ConsoleOutputWriter _output;
    private readonly IEnumerable<IOutputFormatter> _formatters;
    private readonly ILogger<StatusCommand> _logger;
    private readonly AppSettings _appSettings;

    public string Name => "status";
    public string Description => "Display current prices and system status";

    public StatusCommand(
        IPriceMonitoringService priceService,
        IAlertService alertService,
        ISpreadAnalysisService spreadAnalysisService,
        ConsoleOutputWriter output,
        IEnumerable<IOutputFormatter> formatters,
        ILogger<StatusCommand> logger,
        AppSettings appSettings)
    {
        _priceService = priceService;
        _alertService = alertService;
        _spreadAnalysisService = spreadAnalysisService;
        _output = output;
        _formatters = formatters;
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
        var errors = new List<string>();
        var validFormats = new[] { "table", "json" };

        if (context.HasOption("format") && !validFormats.Contains(context.GetOption("format"), StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"--format must be one of: {string.Join(", ", validFormats)}");
        }
        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        _output.WriteHeader("System Status");

        try
        {
            var assetFilter = context.GetOption("asset");
            var format = context.GetOption("format", "table");

            var formatter = _formatters.FirstOrDefault(f => f.FormatType.Equals(format, StringComparison.OrdinalIgnoreCase));
            if (formatter is null)
            {
                _output.WriteError($"Unsupported format: {format}. Available formats: {string.Join(", ", _formatters.Select(f => f.FormatType))}");
                return 1;
            }

            _output.WriteSection("Configuration");
            _output.WriteKeyValue("Monitoring Interval", $"{_appSettings.MonitoringIntervalSeconds}s");
            _output.WriteKeyValue("Alert Cooldown", $"{_appSettings.AlertCooldownMinutes}m");
            _output.WriteKeyValue("WebSocket Enabled", _appSettings.EnableWebSocket ? "Yes" : "No");
            _output.WriteKeyValue("Telegram Enabled", _appSettings.EnableTelegramNotifications ? "Yes" : "No");
            _output.WriteKeyValue("History Retention", $"{_appSettings.HistoryRetentionDays} days");
            _output.WriteKeyValue("Max Alerts per User", $"{_appSettings.MaxAlertsPerUser}");
            _output.WriteKeyValue("Default Price Change Threshold", $"{_appSettings.DefaultPriceChangeThreshold}%");
            _output.WriteKeyValue("Default Spread Threshold", $"{_appSettings.DefaultSpreadThreshold}%");

            _output.WriteSection("Current Prices");

            var prices = await _priceService.GetAllCurrentPricesAsync().ConfigureAwait(false);
            var displayPrices = new List<object>();

            foreach (var price in prices)
            {
                if (!string.IsNullOrEmpty(assetFilter) && !price.Asset.Equals(assetFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var spread = await _spreadAnalysisService.GetSpreadAnalysisAsync(price.Asset, price.Fiat).ConfigureAwait(false);

                displayPrices.Add(new
                {
                    price.Asset,
                    price.Fiat,
                    BuyPrice = price.BuyPrice.ToString("F8"),
                    SellPrice = price.SellPrice.ToString("F8"),
                    Spread = spread?.CurrentSpreadPercent.ToString("F2") + "%" ?? "N/A",
                    Change = price.BuyChangePercent.ToString("+0.00;-0.00;0") + "%",
                    Updated = price.UpdatedAt.GetTimeAgoString()
                });
            }

            if (!displayPrices.Any())
            {
                _output.WriteInfo("No prices available");
                return 0;
            }

            _output.WriteRaw(formatter.Format(displayPrices));

            _output.WriteBlankLine();
            _output.WriteSection("Active Alerts Summary");
            // Assuming UserId = 1 for CLI. In a real app, this would be dynamic.
            var userAlerts = await _alertService.GetUserAlertsAsync(1).ConfigureAwait(false);
            if (userAlerts.Any())
            {
                var enabledAlerts = userAlerts.Count(a => a.IsEnabled);
                var totalAlerts = userAlerts.Count();
                _output.WriteInfo($"Enabled: {enabledAlerts} / Total: {totalAlerts}");
            }
            else
            {
                _output.WriteInfo("No alerts configured.");
            }

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

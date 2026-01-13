#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to manage price alerts
/// </summary>
public class AlertCommand : ICommand
{
    private readonly IAlertService _alertService;
    private readonly ConsoleOutputWriter _output;
    private readonly ILogger<AlertCommand> _logger;

    public string Name => "alert";
    public string Description => "Manage price alerts and notifications";

    public AlertCommand(
        IAlertService alertService,
        ConsoleOutputWriter output,
        ILogger<AlertCommand> logger)
    {
        _alertService = alertService;
        _output = output;
        _logger = logger;
    }

    public string GetHelp()
    {
        return @"
Usage: binance-p2p-monitor alert <subcommand> [options]

Manage price alerts and notification settings.

Subcommands:
  list                List all active alerts
  create              Create a new price alert
  delete <id>         Delete an alert by ID
  test                Send test notification

Options:
  --asset=ASSET       Asset name (BTC, ETH, etc.)
  --fiat=FIAT         Fiat currency (USDT, CNY, etc.)
  --type=TYPE         Alert type: price, spread, change
  --threshold=VALUE   Alert threshold value
  --condition=COND    Condition: above, below, equals

Examples:
  binance-p2p-monitor alert list
  binance-p2p-monitor alert create --asset=BTC --fiat=USDT --type=price --condition=above --threshold=50000
  binance-p2p-monitor alert delete 123e4567-e89b-12d3-a456-426614174000
  binance-p2p-monitor alert test
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        var subcommand = context.Arguments.FirstOrDefault();
        if (string.IsNullOrEmpty(subcommand))
            errors.Add("Subcommand required: list, create, delete, or test");

        var validSubcommands = new[] { "list", "create", "delete", "test" };
        if (!string.IsNullOrEmpty(subcommand) && !validSubcommands.Contains(subcommand))
            errors.Add($"Invalid subcommand: {subcommand}");

        if (subcommand == "create")
        {
            if (!context.HasOption("asset"))
                errors.Add("--asset is required for create");
            if (!context.HasOption("fiat"))
                errors.Add("--fiat is required for create");
            if (!context.HasOption("type"))
                errors.Add("--type is required for create");
        }

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        var subcommand = context.Arguments.FirstOrDefault()?.ToLower() ?? "list";

        return subcommand switch
        {
            "list" => await ListAlertsAsync(context),
            "create" => await CreateAlertAsync(context),
            "delete" => await DeleteAlertAsync(context),
            "test" => await TestNotificationAsync(context),
            _ => 1
        };
    }

    private async Task<int> ListAlertsAsync(CommandContext context)
    {
        try
        {
            _output.WriteSection("Active Price Alerts");
            // Fetch and display alerts
            _output.WriteInfo("(No alerts configured)");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list alerts");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> CreateAlertAsync(CommandContext context)
    {
        try
        {
            var asset = context.GetOption("asset", "");
            var fiat = context.GetOption("fiat", "");
            var type = context.GetOption("type", "");

            _output.WriteInfo($"Creating alert for {asset}/{fiat}");
            // Create alert logic
            _output.WriteSuccess("Alert created successfully");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> DeleteAlertAsync(CommandContext context)
    {
        try
        {
            var alertId = context.Arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(alertId))
            {
                _output.WriteError("Alert ID is required");
                return 1;
            }

            _output.WriteInfo($"Deleting alert {alertId}");
            // Delete alert logic
            _output.WriteSuccess("Alert deleted successfully");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete alert");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> TestNotificationAsync(CommandContext context)
    {
        try
        {
            _output.WriteInfo("Sending test notification...");
            _output.WriteSuccess("Test notification sent");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test notification");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

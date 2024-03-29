#nullable enable
namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to manage price alerts
/// </summary>
public sealed class AlertCommand : ICommand
{
    private readonly IAlertService _alertService;
    private readonly ConsoleOutputWriter _output;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AlertCommand> _logger;

    public string Name => "alert";
    public string Description => "Manage price alerts and notifications";

    public AlertCommand(
        IAlertService alertService,
        ConsoleOutputWriter output,
        AppSettings appSettings,
        ILogger<AlertCommand> logger)
    {
        _alertService = alertService;
        _output = output;
        _appSettings = appSettings;
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
            if (!context.HasOption("threshold"))
                errors.Add("--threshold is required for create");
            if (!context.HasOption("condition"))
                errors.Add("--condition is required for create");

            if (context.HasOption("threshold") && !decimal.TryParse(context.GetOption("threshold"), out _))
                errors.Add("Invalid value for --threshold. Must be a number.");

            if (context.HasOption("type") && !Enum.TryParse<AlertType>(context.GetOption("type"), true, out _))
                errors.Add($"Invalid value for --type. Valid types are: {string.Join(", ", Enum.GetNames(typeof(AlertType)))}");

            if (context.HasOption("condition") && !Enum.TryParse<AlertCondition>(context.GetOption("condition"), true, out _))
                errors.Add($"Invalid value for --condition. Valid conditions are: {string.Join(", ", Enum.GetNames(typeof(AlertCondition)))}");
        }
        else if (subcommand == "delete")
        {
            var alertIdString = context.Arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(alertIdString))
            {
                errors.Add("Alert ID is required for delete.");
            }
            else if (!int.TryParse(alertIdString, out _))
            {
                errors.Add($"Invalid alert ID: {alertIdString}. Must be an integer.");
            }
        }
        else if (subcommand == "test")
        {
            var alertIdString = context.Arguments.Skip(1).FirstOrDefault();
            if (!string.IsNullOrEmpty(alertIdString) && !int.TryParse(alertIdString, out _))
            {
                errors.Add($"Invalid alert ID: {alertIdString}. Must be an integer for test if provided.");
            }
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
            var alerts = await _alertService.GetUserAlertsAsync(ApplicationConstants.DefaultCliUserId).ConfigureAwait(false);
            if (alerts.Any())
            {
                var rows = alerts.Select(a => new Dictionary<string, string>
                {
                    ["Id"] = a.Id.ToString(),
                    ["Asset"] = a.Asset,
                    ["Fiat"] = a.Fiat,
                    ["Type"] = a.AlertType.ToString(),
                    ["Threshold"] = a.Threshold.ToString("F2"),
                    ["Active"] = a.IsEnabled.ToString(),
                });
                _output.WriteTable(rows);
            }
            else
            {
                _output.WriteInfo("(No alerts configured)");
            }
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
            var asset = context.GetOption("asset", string.Empty);
            var fiat = context.GetOption("fiat", string.Empty);
            var typeString = context.GetOption("type", string.Empty);
            var thresholdString = context.GetOption("threshold", string.Empty);
            var conditionString = context.GetOption("condition", string.Empty);
            var notes = context.GetOption("notes", string.Empty);

            if (string.IsNullOrWhiteSpace(asset) || string.IsNullOrWhiteSpace(fiat) ||
                string.IsNullOrWhiteSpace(typeString) || string.IsNullOrWhiteSpace(thresholdString) ||
                string.IsNullOrWhiteSpace(conditionString))
            {
                _output.WriteError("Asset, Fiat, Type, Threshold, and Condition are required for creating an alert.");
                return 1;
            }

            if (!Enum.TryParse(typeString, true, out AlertType alertType))
            {
                _output.WriteError($"Invalid alert type: {typeString}. Valid types are: {string.Join(", ", Enum.GetNames(typeof(AlertType)))}");
                return 1;
            }

            if (!decimal.TryParse(thresholdString, out decimal threshold))
            {
                _output.WriteError($"Invalid threshold value: {thresholdString}. Must be a number.");
                return 1;
            }

            if (!Enum.TryParse(conditionString, true, out AlertCondition condition))
            {
                _output.WriteError($"Invalid condition: {conditionString}. Valid conditions are: {string.Join(", ", Enum.GetNames(typeof(AlertCondition)))}");
                return 1;
            }

            var alert = new PriceAlert
            {
                Asset = asset,
                Fiat = fiat,
                AlertType = alertType,
                Threshold = threshold,
                Condition = condition,
                UserId = ApplicationConstants.DefaultCliUserId,
                IsEnabled = true,
                Notes = notes
            };

            var alertId = await _alertService.CreateAlertAsync(alert).ConfigureAwait(false);
            _output.WriteSuccess($"Alert created successfully with ID: {alertId}");
            return 0;
        }
        catch (InvalidAlertException ex)
        {
            _logger.LogError(ex, "Invalid alert configuration");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
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
            var alertIdString = context.Arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(alertIdString))
            {
                _output.WriteError("Alert ID is required.");
                return 1;
            }

            if (!int.TryParse(alertIdString, out int alertId))
            {
                _output.WriteError($"Invalid alert ID: {alertIdString}. Must be an integer.");
                return 1;
            }

            _output.WriteInfo($"Attempting to delete alert {alertId}...");
            var deleted = await _alertService.DeleteAlertAsync(alertId).ConfigureAwait(false);
            if (deleted)
            {
                _output.WriteSuccess($"Alert {alertId} deleted successfully.");
                return 0;
            }
            else
            {
                _output.WriteError($"Alert {alertId} not found or could not be deleted.");
                return 1;
            }
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogError(ex, "Alert not found");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
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
            var alertIdString = context.Arguments.Skip(1).FirstOrDefault();
            if (!string.IsNullOrEmpty(alertIdString))
            {
                if (!int.TryParse(alertIdString, out int alertId))
                {
                    _output.WriteError($"Invalid alert ID: {alertIdString}. Must be an integer.");
                    return 1;
                }

                _output.WriteInfo($"Sending test notification for alert {alertId}...");
                var success = await _alertService.TestAlertAsync(alertId).ConfigureAwait(false);
                if (success)
                {
                    _output.WriteSuccess($"Test notification sent for alert {alertId}");
                    return 0;
                }
                else
                {
                    _output.WriteError($"Failed to send test notification for alert {alertId}");
                    return 1;
                }
            }
            else
            {
                _output.WriteInfo("Sending generic test notification to admin chat...");
                // Assuming telegramChatId needs to be parsed from string
                if (string.IsNullOrEmpty(_appSettings.TelegramAdminChatId) || !long.TryParse(_appSettings.TelegramAdminChatId, out long adminChatId))
                {
                    _output.WriteError("Telegram admin chat ID is not configured or is invalid in AppSettings.");
                    return 1;
                }
                
                await _alertService.SendNotificationAsync(adminChatId, "Test notification from Binance P2P Monitor.").ConfigureAwait(false);
                _output.WriteSuccess("Generic test notification sent to admin chat.");
                return 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test notification");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

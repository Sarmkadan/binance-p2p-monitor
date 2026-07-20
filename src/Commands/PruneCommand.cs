#nullable enable

using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Infrastructure;
using BinanceP2pMonitor.Services;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Command to prune old history records from the database
/// </summary>
public sealed class PruneCommand : ICommand
{
    private readonly IDatabaseCleanupService _databaseCleanupService;
    private readonly ConsoleOutputWriter _output;
    private readonly ILogger<PruneCommand> _logger;

    public string Name => "prune";
    public string Description => "Delete historical price data older than specified number of days";

    public PruneCommand(
        IDatabaseCleanupService databaseCleanupService,
        ConsoleOutputWriter output,
        ILogger<PruneCommand> logger)
    {
        _databaseCleanupService = databaseCleanupService ?? throw new ArgumentNullException(nameof(databaseCleanupService));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GetHelp()
    {
        return @" Prune usage: binance-p2p-monitor prune [options]
 Delete historical price data older than the specified number of days.

Options:
--days=DAYS Number of days to keep (records older than this will be deleted)
-h, --help Show this help message

Examples:
binance-p2p-monitor prune --days=30
binance-p2p-monitor prune --days=90
binance-p2p-monitor prune --help
";
    }

    public List<string> ValidateArguments(CommandContext context)
    {
        var errors = new List<string>();

        if (!context.HasOption("days"))
        {
            errors.Add("--days parameter is required");
        }
        else
        {
            if (!int.TryParse(context.GetOption("days"), out var days) || days <= 0)
            {
                errors.Add("Days must be a positive integer");
            }
        }

        return errors;
    }

    public async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            _output.WriteHeader("Database Prune");

            if (!context.HasOption("days"))
            {
                _output.WriteError("Parameter --days is required. Use --help for usage information.");
                return 1;
            }

            if (!int.TryParse(context.GetOption("days"), out var days) || days <= 0)
            {
                _output.WriteError("Invalid value for --days. Must be a positive integer.");
                return 1;
            }

            // Get initial count
            var initialCount = await _databaseCleanupService.GetTotalHistoryCountAsync().ConfigureAwait(false);
            _output.WriteInfo($"Preparing to delete records older than {days} days...");
            _output.WriteInfo($"Current total records: {initialCount}");
            _output.WriteBlankLine();

            // Confirmation prompt
            _output.WriteWarning("WARNING: This will permanently delete historical price data.");
            _output.WriteWarning("Are you sure you want to continue? (yes/no)");

            var confirmation = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (confirmation != "yes")
            {
                _output.WriteInfo("Prune operation cancelled.");
                return 0;
            }

            _output.WriteBlankLine();
            _output.WriteInfo($"Deleting records older than {days} days...");

            var deletedCount = await _databaseCleanupService.DeleteOldRecordsAsync(days).ConfigureAwait(false);

            var remainingCount = await _databaseCleanupService.GetTotalHistoryCountAsync().ConfigureAwait(false);

            _output.WriteSuccess($"Prune completed successfully!");
            _output.WriteInfo($"Records deleted: {deletedCount}");
            _output.WriteInfo($"Current total records: {remainingCount}");
            _output.WriteBlankLine();
            _output.WriteInfo("Prune operation finished.");

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Prune command failed");
            _output.WriteError($"Error: {ex.Message}");
            return 1;
        }
    }
}

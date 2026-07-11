#nullable enable

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for ExportCommand providing additional functionality
/// </summary>
public static class ExportCommandExtensions
{
    /// <summary>
    /// Validates if the output file path is writable before execution
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="context">The command context</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null</exception>
    public static List<string> ValidateOutputPath(this ExportCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        var errors = new List<string>();

        if (!context.HasOption("output"))
        {
            errors.Add("--output is required");
            return errors;
        }

        var outputPath = context.GetOption("output", string.Empty);

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            errors.Add("--output path cannot be empty");
            return errors;
        }

        var directory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
        {
            try
            {
                // Check if we have write permissions to the directory
                var testFile = Path.Combine(directory, $".write_test_{Guid.NewGuid()}");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch (UnauthorizedAccessException)
            {
                errors.Add($"No write permissions for directory: {directory}");
            }
            catch (IOException)
            {
                errors.Add($"Directory is not writable: {directory}");
            }
        }
        else if (!string.IsNullOrEmpty(directory))
        {
            errors.Add($"Output directory does not exist: {directory}");
        }

        return errors;
    }

    /// <summary>
    /// Gets the default output path based on current date and time
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="asset">Optional asset filter</param>
    /// <param name="fiat">Optional fiat filter</param>
    /// <returns>Default output file path</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="asset"/> or <paramref name="fiat"/> are null</exception>
    public static string GetDefaultOutputPath(this ExportCommand command, string? asset = null, string? fiat = null)
    {
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(fiat);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var assetPart = string.IsNullOrEmpty(asset) ? "all" : asset.ToUpperInvariant();
        var fiatPart = string.IsNullOrEmpty(fiat) ? "all" : fiat.ToUpperInvariant();

        return fiat is null
            ? $"exports/{assetPart}_prices_{timestamp}.csv"
            : $"exports/{assetPart}_{fiatPart}_prices_{timestamp}.csv";
    }

    /// <summary>
    /// Gets the format type from the command context with fallback to CSV
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="context">The command context</param>
    /// <returns>The format type (csv, json, etc.)</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null</exception>
    public static string GetFormat(this ExportCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        return context.GetOption("format", "csv");
    }

    /// <summary>
    /// Gets the number of days to export with validation
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="context">The command context</param>
    /// <returns>Number of days to export</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null</exception>
    public static int GetDaysToExport(this ExportCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        var daysString = context.GetOption("days", "7");

        if (!int.TryParse(daysString, out int days) || days <= 0)
        {
            return 7; // Default fallback
        }

        return Math.Min(days, 365); // Cap at 1 year for safety
    }

    /// <summary>
    /// Gets the asset filter from command context
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="context">The command context</param>
    /// <returns>Asset filter or empty string if not specified</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null</exception>
    public static string GetAssetFilter(this ExportCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        return context.GetOption("asset", string.Empty);
    }

    /// <summary>
    /// Gets the fiat filter from command context
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="context">The command context</param>
    /// <returns>Fiat filter or empty string if not specified</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null</exception>
    public static string GetFiatFilter(this ExportCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        return context.GetOption("fiat", string.Empty);
    }

    /// <summary>
    /// Validates that asset and fiat filters are used together when specified
    /// </summary>
    /// <param name="command">The ExportCommand instance</param>
    /// <param name="context">The command context</param>
    /// <returns>List of validation errors, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null</exception>
    public static List<string> ValidateAssetFiatPair(this ExportCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        var errors = new List<string>();

        var hasAsset = context.HasOption("asset");
        var hasFiat = context.HasOption("fiat");

        if ((hasAsset && !hasFiat) || (!hasAsset && hasFiat))
        {
            errors.Add("--asset and --fiat must be provided together if either is used for filtering.");
        }

        return errors;
    }
}
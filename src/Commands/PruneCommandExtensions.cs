#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for PruneCommand providing additional functionality
/// </summary>
public static class PruneCommandExtensions
{
    /// <summary>
    /// Splits the help text of PruneCommand into individual lines, without trailing blank lines
    /// </summary>
    /// <param name="command">The PruneCommand instance</param>
    /// <returns>Read-only list of help text lines</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetHelpLines(this PruneCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lines = command.GetHelp()
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .ToList();

        while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }

    /// <summary>
    /// Returns a one-line summary of the prune command in the form "name: description"
    /// </summary>
    /// <param name="command">The PruneCommand instance</param>
    /// <returns>Formatted summary line</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetSummaryLine(this PruneCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"{command.Name}: {command.Description}";
    }

    /// <summary>
    /// Attempts to read the --days option the same way PruneCommand does (a positive integer)
    /// </summary>
    /// <param name="command">The PruneCommand instance</param>
    /// <param name="context">The command context containing the options</param>
    /// <param name="days">The parsed number of days when the method returns <c>true</c>; otherwise 0</param>
    /// <returns>True if --days is present and parses to a positive integer, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="context"/> is null</exception>
    public static bool TryGetDays(this PruneCommand command, CommandContext context, out int days)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        if (int.TryParse(context.GetOption("days"), out days) && days > 0)
        {
            return true;
        }

        days = 0;
        return false;
    }

    /// <summary>
    /// Calculates the UTC cutoff date before which history records are considered old
    /// </summary>
    /// <param name="command">The PruneCommand instance</param>
    /// <param name="days">Number of days to keep; must be positive</param>
    /// <param name="utcNow">Reference time; defaults to <see cref="DateTime.UtcNow"/></param>
    /// <returns>The UTC timestamp that is <paramref name="days"/> before the reference time</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="days"/> is not positive</exception>
    public static DateTime GetCutoffUtc(this PruneCommand command, int days, DateTime? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);

        var reference = utcNow ?? DateTime.UtcNow;
        return reference.AddDays(-days);
    }
}

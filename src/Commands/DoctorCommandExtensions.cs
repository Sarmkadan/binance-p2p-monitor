#nullable enable

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for DoctorCommand providing additional functionality
/// </summary>
public static class DoctorCommandExtensions
{
    /// <summary>
    /// Validates that the doctor command was invoked without any arguments
    /// </summary>
    /// <param name="command">The DoctorCommand instance</param>
    /// <param name="context">The command context containing the arguments to validate</param>
    /// <returns>True if no validation errors were reported, otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="context"/> is null</exception>
    public static bool ValidateNoArguments(this DoctorCommand command, CommandContext context)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        return command.ValidateArguments(context).Count == 0;
    }

    /// <summary>
    /// Splits the help text of DoctorCommand into individual lines, without trailing blank lines
    /// </summary>
    /// <param name="command">The DoctorCommand instance</param>
    /// <returns>Read-only list of help text lines</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetHelpLines(this DoctorCommand command)
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
    /// Returns a one-line summary of the doctor command in the form "name: description"
    /// </summary>
    /// <param name="command">The DoctorCommand instance</param>
    /// <returns>Formatted summary line</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetSummaryLine(this DoctorCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"{command.Name}: {command.Description}";
    }
}

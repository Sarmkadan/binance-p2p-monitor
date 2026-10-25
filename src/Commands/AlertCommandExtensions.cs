#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using BinanceP2pMonitor.CLI;
using BinanceP2pMonitor.Constants;

namespace BinanceP2pMonitor.Commands;

/// <summary>
/// Extension methods for AlertCommand providing additional helpers for subcommand validation and argument parsing
/// </summary>
public static class AlertCommandExtensions
{
    private static readonly string[] Subcommands = { "list", "create", "delete", "test" };

    /// <summary>
    /// Gets the subcommands accepted by AlertCommand's argument validation
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <returns>Read-only list of subcommand names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetSubcommands(this AlertCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Subcommands;
    }

    /// <summary>
    /// Checks whether a subcommand name passes AlertCommand's validation (case-sensitive, as in ValidateArguments)
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <param name="subcommand">The subcommand name to check</param>
    /// <returns><c>true</c> if the subcommand is accepted; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool IsValidSubcommand(this AlertCommand command, string? subcommand)
    {
        ArgumentNullException.ThrowIfNull(command);

        return !string.IsNullOrEmpty(subcommand) && Subcommands.Contains(subcommand);
    }

    /// <summary>
    /// Splits the help text of AlertCommand into individual lines, without trailing blank lines
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <returns>Read-only list of help text lines</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetHelpLines(this AlertCommand command)
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
    /// Returns a one-line summary of the alert command in the form "name: description"
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <returns>Formatted summary line</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static string GetSummaryLine(this AlertCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return $"{command.Name}: {command.Description}";
    }

    /// <summary>
    /// Reads the alert ID that follows the subcommand (for delete and test), the same way AlertCommand does
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <param name="context">The command context containing the arguments</param>
    /// <param name="alertId">The parsed alert ID when the method returns <c>true</c>; otherwise 0</param>
    /// <returns><c>true</c> if an integer alert ID is present in the second argument; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> or <paramref name="context"/> is null</exception>
    public static bool TryGetAlertId(this AlertCommand command, CommandContext context, out int alertId)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        var alertIdString = context.Arguments.Skip(1).FirstOrDefault();
        if (int.TryParse(alertIdString, out alertId))
        {
            return true;
        }

        alertId = 0;
        return false;
    }

    /// <summary>
    /// Parses a --threshold value the same way AlertCommand does (a decimal number)
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <param name="value">The raw threshold text</param>
    /// <param name="threshold">The parsed threshold when the method returns <c>true</c>; otherwise 0</param>
    /// <returns><c>true</c> if the value parses as a decimal; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool TryParseThreshold(this AlertCommand command, string? value, out decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(command);

        return decimal.TryParse(value, out threshold);
    }

    /// <summary>
    /// Parses a --type value into an <see cref="AlertType"/> (case-insensitive)
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <param name="value">The raw type text</param>
    /// <param name="alertType">The parsed alert type when the method returns <c>true</c>; otherwise <see cref="AlertType.Unknown"/></param>
    /// <returns><c>true</c> if the value names a defined alert type; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool TryParseAlertType(this AlertCommand command, string? value, out AlertType alertType)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out alertType))
        {
            return true;
        }

        alertType = AlertType.Unknown;
        return false;
    }

    /// <summary>
    /// Parses a --condition value into an <see cref="AlertCondition"/> (case-insensitive)
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <param name="value">The raw condition text</param>
    /// <param name="condition">The parsed condition when the method returns <c>true</c>; otherwise <see cref="AlertCondition.Unknown"/></param>
    /// <returns><c>true</c> if the value names a defined condition; otherwise, <c>false</c></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static bool TryParseAlertCondition(this AlertCommand command, string? value, out AlertCondition condition)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse(value, true, out condition))
        {
            return true;
        }

        condition = AlertCondition.Unknown;
        return false;
    }

    /// <summary>
    /// Gets the names of all alert types, as listed in AlertCommand's error messages
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <returns>Read-only list of alert type names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetAlertTypeNames(this AlertCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Enum.GetNames<AlertType>();
    }

    /// <summary>
    /// Gets the names of all alert conditions, as listed in AlertCommand's error messages
    /// </summary>
    /// <param name="command">The AlertCommand instance</param>
    /// <returns>Read-only list of alert condition names</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null</exception>
    public static IReadOnlyList<string> GetAlertConditionNames(this AlertCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return Enum.GetNames<AlertCondition>();
    }
}

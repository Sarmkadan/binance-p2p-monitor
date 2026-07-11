#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BinanceP2pMonitor.CLI;

namespace BinanceP2pMonitor.Commands
{
    /// <summary>
    /// Extension methods for <see cref="VersionCommand"/>.
    /// </summary>
    public static class VersionCommandExtensions
    {
        /// <summary>
        /// Retrieves the version string of the assembly that contains <see cref="VersionCommand"/>.
        /// </summary>
        /// <param name="command">The version command instance.</param>
        /// <returns>The version string from the assembly, or "1.0.0" if not available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>null</c>.</exception>
        public static string GetVersionString(this VersionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var version = typeof(VersionCommand).Assembly
                .GetName()
                .Version?.ToString() ?? "1.0.0";

            return version;
        }

        /// <summary>
        /// Returns a dictionary containing the version information that <see cref="VersionCommand"/> prints.
        /// </summary>
        /// <param name="command">The version command instance.</param>
        /// <returns>A read-only dictionary with version information keys and values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, string> GetInfoDictionary(this VersionCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            var version = command.GetVersionString();

            var info = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Version"] = version,
                ["Author"] = "Vladyslav Zaiets",
                ["Website"] = "https://sarmkadan.com",
                [".NET Version"] = ".NET 10",
                ["License"] = "MIT"
            };

            return info;
        }

        /// <summary>
        /// Validates that the command was invoked without any arguments.
        /// </summary>
        /// <param name="command">The version command instance.</param>
        /// <param name="context">The command context containing arguments to validate.</param>
        /// <returns><c>true</c> when <see cref="VersionCommand.ValidateArguments"/> returns an empty list; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> or <paramref name="context"/> is <c>null</c>.</exception>
        public static bool ValidateNoArguments(this VersionCommand command, CommandContext context)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(context);

            var validationErrors = command.ValidateArguments(context);
            return validationErrors.Count == 0;
        }

        /// <summary>
        /// Executes the command and returns the exit code.
        /// This method simply forwards to <see cref="VersionCommand.ExecuteAsync"/> but provides a more expressive name.
        /// </summary>
        /// <param name="command">The version command instance.</param>
        /// <param name="context">The command context to execute with.</param>
        /// <returns>The exit code from <see cref="VersionCommand.ExecuteAsync"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> or <paramref name="context"/> is <c>null</c>.</exception>
        public static Task<int> PrintVersionInfoAsync(this VersionCommand command, CommandContext context)
        {
            ArgumentNullException.ThrowIfNull(command);
            ArgumentNullException.ThrowIfNull(context);

            return command.ExecuteAsync(context);
        }
    }
}
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
        public static string GetVersionString(this VersionCommand command)
        {
            // The same logic used in ExecuteAsync, but exposed as a reusable method.
            var version = typeof(VersionCommand).Assembly
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";

            return version;
        }

        /// <summary>
        /// Returns a dictionary containing the version information that <see cref="VersionCommand"/> prints.
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetInfoDictionary(this VersionCommand command)
        {
            var version = command.GetVersionString();

            // All static values are taken from the original ExecuteAsync implementation.
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
        /// Returns <c>true</c> when <see cref="VersionCommand.ValidateArguments"/> returns an empty list.
        /// </summary>
        public static bool ValidateNoArguments(this VersionCommand command, CommandContext context)
        {
            var validationErrors = command.ValidateArguments(context);
            return validationErrors.Count == 0;
        }

        /// <summary>
        /// Executes the command and returns the exit code.
        /// This method simply forwards to <see cref="VersionCommand.ExecuteAsync"/> but provides a more expressive name.
        /// </summary>
        public static Task<int> PrintVersionInfoAsync(this VersionCommand command, CommandContext context)
        {
            return command.ExecuteAsync(context);
        }
    }
}

#nullable enable

using BinanceP2pMonitor.CLI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Provides extension methods for testing CommandParser functionality.
/// </summary>
public static class CommandParserTestsExtensions
{
    /// <summary>
    /// Creates a pre-configured CommandParser instance for testing scenarios.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="loggerMock">Optional logger mock to use. If null, creates a new mock.</param>
    /// <param name="serviceProviderMock">Optional service provider mock to use. If null, creates a new mock.</param>
    /// <returns>Configured CommandParser instance</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerMock"/> is null and no logger can be created.</exception>
    public static CommandParser CreateCommandParser(
        this CommandParserTests test,
        ILogger<CommandParser>? loggerMock = null,
        IServiceProvider? serviceProviderMock = null)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));

        var logger = loggerMock ?? Substitute.For<ILogger<CommandParser>>();

        // serviceProviderMock parameter is intentionally ignored as CommandParser doesn't use IServiceProvider in its constructor
        return new CommandParser(logger);
    }

    /// <summary>
    /// Parses command arguments and returns the parsed context.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="args">Command line arguments to parse. Cannot be null.</param>
    /// <param name="commandParser">Command parser instance to use. Cannot be null.</param>
    /// <param name="serviceProvider">Service provider to pass to Parse method. Cannot be null.</param>
    /// <returns>Parsed CommandContext</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="args"/>, <paramref name="commandParser"/>, or <paramref name="serviceProvider"/> is null.</exception>
    public static CommandContext ParseWithContext(
        this CommandParserTests test,
        string[] args,
        CommandParser commandParser,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        ArgumentNullException.ThrowIfNull(commandParser, nameof(commandParser));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        return commandParser.Parse(args, serviceProvider);
    }

    /// <summary>
    /// Verifies that the parsed context has the expected command name.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="context">Parsed context to verify. Cannot be null.</param>
    /// <param name="expectedCommandName">Expected command name. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> or <paramref name="expectedCommandName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="expectedCommandName"/> is empty.</exception>
    public static void ShouldHaveCommandName(
        this CommandParserTests test,
        CommandContext context,
        string expectedCommandName)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentException.ThrowIfNullOrEmpty(expectedCommandName, nameof(expectedCommandName));

        context.CommandName.Should().Be(expectedCommandName,
            because: "the command name should match the expected value");
    }

    /// <summary>
    /// Verifies that the parsed context has the expected arguments in the specified order.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="context">Parsed context to verify. Cannot be null.</param>
    /// <param name="expectedArguments">Expected arguments in order. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> or <paramref name="expectedArguments"/> is null.</exception>
    public static void ShouldHaveArgumentsInOrder(
        this CommandParserTests test,
        CommandContext context,
        params string[] expectedArguments)
    {
        ArgumentNullException.ThrowIfNull(test, nameof(test));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(expectedArguments, nameof(expectedArguments));

        context.Arguments.Should().ContainInOrder(expectedArguments,
            because: "the positional arguments should match the expected values in order");
        context.Arguments.Should().HaveCount(expectedArguments.Length,
            because: "the number of positional arguments should match the expected count");
    }
}

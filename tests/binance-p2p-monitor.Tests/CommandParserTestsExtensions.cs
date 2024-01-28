#nullable enable

using BinanceP2pMonitor.CLI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public static class CommandParserTestsExtensions
{
    /// <summary>
    /// Creates a pre-configured CommandParser instance for testing scenarios.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="loggerMock">Optional logger mock to use. If null, creates a new mock.</param>
    /// <param name="serviceProviderMock">Optional service provider mock to use. If null, creates a new mock.</param>
    /// <returns>Configured CommandParser instance</returns>
    public static CommandParser CreateCommandParser(
        this CommandParserTests test,
        ILogger<CommandParser>? loggerMock = null,
        IServiceProvider? serviceProviderMock = null)
    {
        var logger = loggerMock ?? Substitute.For<ILogger<CommandParser>>();
        var serviceProvider = serviceProviderMock ?? Substitute.For<IServiceProvider>();
        return new CommandParser(logger);
    }

    /// <summary>
    /// Parses command arguments and returns the parsed context.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="args">Command line arguments</param>
    /// <param name="commandParser">Command parser instance to use</param>
    /// <param name="serviceProvider">Service provider to pass to Parse method</param>
    /// <returns>Parsed CommandContext</returns>
    public static CommandContext ParseWithContext(
        this CommandParserTests test,
        string[] args,
        CommandParser commandParser,
        IServiceProvider serviceProvider)
    {
        return commandParser.Parse(args, serviceProvider);
    }

    /// <summary>
    /// Verifies that the parsed context has the expected command name.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="context">Parsed context to verify</param>
    /// <param name="expectedCommandName">Expected command name</param>
    public static void ShouldHaveCommandName(
        this CommandParserTests test,
        CommandContext context,
        string expectedCommandName)
    {
        context.CommandName.Should().Be(expectedCommandName,
            because: "the command name should match the expected value");
    }

    /// <summary>
    /// Verifies that the parsed context has the expected arguments in the specified order.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="context">Parsed context to verify</param>
    /// <param name="expectedArguments">Expected arguments in order</param>
    public static void ShouldHaveArgumentsInOrder(
        this CommandParserTests test,
        CommandContext context,
        params string[] expectedArguments)
    {
        context.Arguments.Should().ContainInOrder(expectedArguments,
            because: "the positional arguments should match the expected values in order");
        context.Arguments.Should().HaveCount(expectedArguments.Length,
            because: "the number of positional arguments should match the expected count");
    }
}

#nullable enable

using BinanceP2pMonitor.CLI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class CommandParserTests
{
    private readonly ILogger<CommandParser> _mockLogger;
    private readonly IServiceProvider _mockServiceProvider;
    private readonly CommandParser _commandParser;

    public CommandParserTests()
    {
        _mockLogger = Substitute.For<ILogger<CommandParser>>();
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _commandParser = new CommandParser(_mockLogger);
    }

    [Fact]
    public void Parse_ShouldReturnHelpCommand_WhenNoArguments()
    {
        // Arrange
        string[] args = System.Array.Empty<string>();

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("help");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
        context.ServiceProvider.Should().Be(_mockServiceProvider);
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "help", 0, 0);
    }

    [Fact]
    public void Parse_ShouldParseCommandOnly()
    {
        // Arrange
        string[] args = { "monitor" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "monitor", 0, 0);
    }

    [Fact]
    public void Parse_ShouldParseCommandWithPositionalArguments()
    {
        // Arrange
        string[] args = { "monitor", "USDT", "UAH" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().ContainInOrder("USDT", "UAH");
        context.Arguments.Should().HaveCount(2);
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "monitor", 0, 0);
    }

    [Fact]
    public void Parse_ShouldParseCommandWithLongOptions()
    {
        // Arrange
        string[] args = { "monitor", "--asset=USDT", "--fiat=UAH" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().Contain(new Dictionary<string, string>
        {
            { "asset", "USDT" },
            { "fiat", "UAH" }
        });
        context.Flags.Should().BeEmpty();
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "monitor", 2, 0);
    }

    [Fact]
    public void Parse_ShouldParseCommandWithFlags()
    {
        // Arrange
        string[] args = { "monitor", "-v", "-d" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().Contain(new Dictionary<string, string>
        {
            { "v", "true" },
            { "d", "true" }
        });
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "monitor", 0, 2);
    }

    [Fact]
    public void Parse_ShouldParseCommandWithShortOptions()
    {
        // Arrange
        string[] args = { "monitor", "-a", "USDT", "-f", "UAH" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().Contain(new Dictionary<string, string>
        {
            { "a", "USDT" },
            { "f", "UAH" }
        });
        context.Flags.Should().BeEmpty();
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "monitor", 2, 0);
    }

    [Fact]
    public void Parse_ShouldParseMixedArguments()
    {
        // Arrange
        string[] args = { "monitor", "BTC", "EUR", "--limit=10", "-v", "-o", "json" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().ContainInOrder("BTC", "EUR");
        context.Arguments.Should().HaveCount(2);
        context.Options.Should().Contain(new Dictionary<string, string>
        {
            { "limit", "10" },
            { "o", "json" }
        });
        context.Flags.Should().Contain(new Dictionary<string, string>
        {
            { "v", "true" }
        });
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "monitor", 2, 1);
    }

    [Fact]
    public void Parse_ShouldHandleOptionValuesWithSpaces()
    {
        // Arrange
        string[] args = { "alert", "--message="Hello World"", "-c", "USDT/UAH > 10" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("alert");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().Contain(new Dictionary<string, string>
        {
            { "message", ""Hello World"" },
            { "c", "USDT/UAH > 10" }
        });
        context.Flags.Should().BeEmpty();
    }
    
    [Fact]
    public void Parse_ShouldHandleDuplicateOptions_LastOneWins()
    {
        // Arrange
        string[] args = { "command", "--option=first", "--option=second", "-f", "third", "-f", "fourth" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("command");
        context.Options.Should().ContainKey("option").WhoseValue.Should().Be("second");
        context.Options.Should().ContainKey("f").WhoseValue.Should().Be("fourth");
        _mockLogger.Received(1).LogDebug(
            "Parsed command: {Command} with {OptionCount} options and {FlagCount} flags",
            "command", 2, 0);
    }

    [Fact]
    public void Parse_ShouldDistinguishBetweenFlagsAndPositionalArgumentsStartingWithDash()
    {
        // Arrange: -p is a flag, but -123 is a positional argument
        string[] args = { "command", "-p", "value", "-123" };

        // Act
        var context = _commandParser.Parse(args, _mockServiceProvider);

        // Assert
        context.CommandName.Should().Be("command");
        context.Options.Should().ContainKey("p").WhoseValue.Should().Be("value");
        context.Arguments.Should().ContainSingle().Which.Should().Be("-123");
    }
}

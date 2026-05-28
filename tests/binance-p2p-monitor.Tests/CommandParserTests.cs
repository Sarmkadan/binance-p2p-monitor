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
        string[] args = System.Array.Empty<string>();

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("help");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
        context.ServiceProvider.Should().Be(_mockServiceProvider);
    }

    [Fact]
    public void Parse_ShouldParseCommandOnly()
    {
        string[] args = { "monitor" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldParseCommandWithPositionalArguments()
    {
        string[] args = { "monitor", "USDT", "UAH" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().ContainInOrder("USDT", "UAH");
        context.Arguments.Should().HaveCount(2);
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldParseCommandWithLongOptions()
    {
        string[] args = { "monitor", "--asset=USDT", "--fiat=UAH" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().Contain(new Dictionary<string, string>
        {
            { "asset", "USDT" },
            { "fiat", "UAH" }
        });
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldParseCommandWithFlags()
    {
        string[] args = { "monitor", "-v", "-d" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().Contain(new Dictionary<string, string>
        {
            { "v", "true" },
            { "d", "true" }
        });
    }

    [Fact]
    public void Parse_ShouldParseCommandWithShortOptions()
    {
        string[] args = { "monitor", "-a", "USDT", "-f", "UAH" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().Contain(new Dictionary<string, string>
        {
            { "a", "USDT" },
            { "f", "UAH" }
        });
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldParseMixedArguments()
    {
        string[] args = { "monitor", "BTC", "EUR", "--limit=10", "-v", "-o", "json" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

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
    }

    [Fact]
    public void Parse_ShouldHandleOptionValuesWithSpaces()
    {
        string[] args = { "alert", "--message=hello world", "-c", "USDT/UAH > 10" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("alert");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().ContainKey("message");
        context.Options.Should().ContainKey("c");
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ShouldHandleDuplicateOptions_LastOneWins()
    {
        string[] args = { "command", "--option=first", "--option=second", "-f", "third", "-f", "fourth" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("command");
        context.Options.Should().ContainKey("option").WhoseValue.Should().Be("second");
        context.Options.Should().ContainKey("f").WhoseValue.Should().Be("fourth");
    }

    [Fact]
    public void Parse_ShouldDistinguishBetweenFlagsAndPositionalArgumentsStartingWithDash()
    {
        // -p is a flag with a value, but -123 is a positional argument (starts with digit after dash)
        string[] args = { "command", "-p", "value", "-123" };

        var context = _commandParser.Parse(args, _mockServiceProvider);

        context.CommandName.Should().Be("command");
        context.Options.Should().ContainKey("p").WhoseValue.Should().Be("value");
        context.Arguments.Should().ContainSingle().Which.Should().Be("-123");
    }
}

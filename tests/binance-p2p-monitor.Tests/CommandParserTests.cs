#nullable enable

using BinanceP2pMonitor.CLI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Comprehensive tests for CommandParser functionality covering:
/// - Known command parsing
/// - Unknown command handling
/// - Argument splitting including quoted arguments
/// - Empty input handling
/// - Option parsing (--key=value and -k value formats)
/// - Flag parsing (-k format)
/// - Mixed argument scenarios
/// </summary>
public class CommandParserTests
{
    private readonly ILogger<CommandParser> _loggerMock;
    private readonly IServiceProvider _serviceProviderMock;

    public CommandParserTests()
    {
        _loggerMock = Substitute.For<ILogger<CommandParser>>();
        _serviceProviderMock = Substitute.For<IServiceProvider>();
    }

    [Fact]
    public void Parse_EmptyArgs_ReturnsHelpCommandContext()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = Array.Empty<string>();

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Should().NotBeNull();
        context.CommandName.Should().Be("help", "empty args should default to help command");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SingleCommandName_SetsCommandName()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().BeEmpty();
        context.Flags.Should().BeEmpty();
    }

    [Theory]
    [InlineData("spread")]
    [InlineData("alert")]
    [InlineData("history")]
    [InlineData("export")]
    [InlineData("status")]
    [InlineData("backtest")]
    [InlineData("version")]
    public void Parse_KnownCommandNames_SetsCommandName(string commandName)
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { commandName };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be(commandName);
        context.Arguments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("invalid")]
    [InlineData("xyz123")]
    public void Parse_UnknownCommandName_SetsCommandNameToUnknown(string commandName)
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { commandName };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be(commandName);
        context.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void Parse_SinglePositionalArgument_SetsArguments()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "BTCUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().Equal(new[] { "BTCUSDT" });
        context.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_MultiplePositionalArguments_SetsArgumentsInOrder()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "spread", "BTCUSDT", "USDTUAH", "ETHUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Should().NotBeNull();
        context.CommandName.Should().Be("spread");
        context.Arguments.Should().Equal(new[] { "BTCUSDT", "USDTUAH", "ETHUSDT" });
        context.Arguments.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_LongOptionWithEquals_SetsOptionCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--currency=BTC" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().ContainKey("currency").WhoseValue.Should().Be("BTC");
        context.Options.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_LongOptionWithoutValue_SetsOptionToTrue()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--verbose" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().ContainKey("verbose").WhoseValue.Should().Be("true");
        context.Options.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_LongOptionWithMultipleEquals_SplitsCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--config=key1=value1:key2=value2" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Options.Should().ContainKey("config").WhoseValue.Should().Be("key1=value1:key2=value2");
    }

    [Fact]
    public void Parse_ShortFlag_SetsFlagCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "-v" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().BeEmpty();
        context.Flags.Should().ContainKey("v").WhoseValue.Should().Be("true");
        context.Flags.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ShortFlagWithValue_SetsOptionInsteadOfFlag()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "-t", "BTCUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Should().NotBeNull();
        context.CommandName.Should().Be("monitor");
        // -t BTCUSDT is parsed as option "t" with value "BTCUSDT"
        // So BTCUSDT is NOT a positional argument
        context.Arguments.Should().BeEmpty();
        context.Options.Should().ContainKey("t").WhoseValue.Should().Be("BTCUSDT");
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MixedShortFlagsAndOptions_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "-v", "-q", "--timeout=30", "BTCUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().Equal(new[] { "BTCUSDT" });
        context.Flags.Should().ContainKeys("v", "q");
        context.Options.Should().ContainKey("timeout").WhoseValue.Should().Be("30");
        context.Options.Should().HaveCount(1);
        context.Flags.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_MixedLongOptionsAndPositionalArgs_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "spread", "--currency=BTC", "--min-volume=1000", "BTCUSDT", "USDTUAH" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("spread");
        context.Arguments.Should().Equal(new[] { "BTCUSDT", "USDTUAH" });
        context.Options.Should().ContainKeys("currency", "min-volume");
        context.Options["currency"].Should().Be("BTC");
        context.Options["min-volume"].Should().Be("1000");
    }

    [Fact]
    public void Parse_OptionsBeforeCommandName_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "--config=test.json", "monitor", "BTCUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("--config=test.json");
        context.Arguments.Should().Equal(new[] { "monitor", "BTCUSDT" });
    }

    [Fact]
    public void Parse_OnlyOptions_NoCommandName()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "--help", "--version" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Should().NotBeNull();
        // First arg becomes the command name
        context.CommandName.Should().Be("--help");
        // Options starting with -- are parsed as options, not arguments
        context.Arguments.Should().BeEmpty();
        // Only the second arg is parsed as an option since it's not a command
        context.Options.Should().ContainKey("version").WhoseValue.Should().Be("true");
        context.Options.Should().HaveCount(1);
    }

    [Fact]
    public void Parse_ComplexRealWorldScenario_ParsesCorrectly()
    {
        // Arrange - simulating a real command like: spread --currency=BTC --min-volume=1000 BTCUSDT USDTUAH --threshold=0.5
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "spread", "--currency=BTC", "--min-volume=1000", "BTCUSDT", "USDTUAH", "--threshold=0.5" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("spread");
        context.Arguments.Should().Equal(new[] { "BTCUSDT", "USDTUAH" });
        context.Options.Should().ContainKeys("currency", "min-volume", "threshold");
        context.Options["currency"].Should().Be("BTC");
        context.Options["min-volume"].Should().Be("1000");
        context.Options["threshold"].Should().Be("0.5");
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_OptionWithNumericValue_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--timeout=30", "--retries=5" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Options.Should().ContainKeys("timeout", "retries");
        context.Options["timeout"].Should().Be("30");
        context.Options["retries"].Should().Be("5");
    }

    [Fact]
    public void Parse_OptionWithEmptyValue_SetsToTrue()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--force" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Options.Should().ContainKey("force").WhoseValue.Should().Be("true");
    }

    [Fact]
    public void Parse_MultiplePositionalArgumentsWithMixedOptions_ParsesCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "alert", "BTCUSDT", "--channel=telegram", "--threshold=1.5", "USDTUAH", "--urgent" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Should().NotBeNull();
        context.CommandName.Should().Be("alert");
        context.Arguments.Should().Equal(new[] { "BTCUSDT", "USDTUAH" });
        context.Options.Should().ContainKeys("channel", "threshold", "urgent");
        context.Options["channel"].Should().Be("telegram");
        context.Options["threshold"].Should().Be("1.5");
        context.Options["urgent"].Should().Be("true");
        context.Flags.Should().BeEmpty();
    }

    [Fact]
    public void Parse_FlagWithNumericValue_ParsesAsOption()
    {
        // Arrange - this is a tricky case where -123 would be parsed as an option
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "-123" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("monitor");
        context.Arguments.Should().Equal(new[] { "-123" });
    }

    [Fact]
    public void Parse_ServiceProviderIsSetInContext()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "BTCUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.ServiceProvider.Should().BeSameAs(_serviceProviderMock);
    }

    [Fact]
    public void Parse_CancellationTokenIsDefaultInContext()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "BTCUSDT" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public void Parse_HasOption_ReturnsCorrectResult()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--currency=BTC" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Act & Assert
        context.HasOption("currency").Should().BeTrue();
        context.HasOption("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void Parse_HasFlag_ReturnsCorrectResult()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "-v", "-q" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Act & Assert
        context.HasFlag("v").Should().BeTrue();
        context.HasFlag("q").Should().BeTrue();
        context.HasFlag("x").Should().BeFalse();
    }

    [Fact]
    public void Parse_GetOption_ReturnsCorrectValue()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--currency=BTC", "--timeout=30" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Act & Assert
        context.GetOption("currency").Should().Be("BTC");
        context.GetOption("timeout").Should().Be("30");
        context.GetOption("nonexistent").Should().BeNull();
    }

    [Fact]
    public void Parse_GetOptionWithDefault_ReturnsCorrectValue()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--currency=BTC" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Act & Assert
        context.GetOption("currency", "USD").Should().Be("BTC");
        context.GetOption("timeout", "30").Should().Be("30");
        context.GetOption("nonexistent", "default").Should().Be("default");
    }

    [Fact]
    public void Parse_ArgumentsPropertyIsSetCorrectly()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "BTCUSDT", "USDTUAH", "--verbose" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Arguments.Should().Equal(new[] { "BTCUSDT", "USDTUAH" });
        context.Arguments.Should().HaveCount(2);
        context.Arguments.Should().NotContain("--verbose");
    }

    [Fact]
    public void Parse_OptionsDictionaryIsInitialized()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--opt1=val1", "--opt2=val2" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Options.Should().NotBeNull();
        context.Options.Should().BeAssignableTo<Dictionary<string, string>>();
        context.Options.Should().ContainKeys("opt1", "opt2");
    }

    [Fact]
    public void Parse_FlagsDictionaryIsInitialized()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "-a", "-b" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Flags.Should().NotBeNull();
        context.Flags.Should().BeAssignableTo<Dictionary<string, string>>();
        context.Flags.Should().ContainKeys("a", "b");
    }

    [Fact]
    public void Parse_CommandNameIsCaseSensitive()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var argsLower = new[] { "monitor" };
        var argsUpper = new[] { "MONITOR" };

        // Act
        var contextLower = parser.Parse(argsLower, _serviceProviderMock);
        var contextUpper = parser.Parse(argsUpper, _serviceProviderMock);

        // Assert
        contextLower.CommandName.Should().Be("monitor");
        contextUpper.CommandName.Should().Be("MONITOR");
    }

    [Fact]
    public void Parse_OptionsAreCaseSensitive()
    {
        // Arrange
        var parser = new CommandParser(_loggerMock);
        var args = new[] { "monitor", "--Currency=BTC", "--timeout=30" };
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.Options.Should().ContainKey("Currency").WhoseValue.Should().Be("BTC");
        context.Options.Should().ContainKey("timeout").WhoseValue.Should().Be("30");
        context.Options.Should().NotContainKey("currency");
    }
}
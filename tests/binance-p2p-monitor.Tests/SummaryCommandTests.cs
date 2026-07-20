#nullable enable

using BinanceP2pMonitor.CLI;
using BinanceP2pMonitor.Commands;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Formatters;
using BinanceP2pMonitor.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Tests for SummaryCommand to verify:
/// - Command registration and parsing
/// - Help text generation
/// - Argument validation
/// - Command factory integration
/// </summary>
public class SummaryCommandTests
{
    private readonly ILogger<SummaryCommand> _loggerMock;
    private readonly IHistoryRepository _historyRepositoryMock;
    private readonly IPriceRepository _priceRepositoryMock;
    private readonly AppSettings _appSettingsMock;
    private readonly IEnumerable<IOutputFormatter> _formattersMock;
    private readonly IServiceProvider _serviceProviderMock;

    public SummaryCommandTests()
    {
        _loggerMock = Substitute.For<ILogger<SummaryCommand>>();
        _historyRepositoryMock = Substitute.For<IHistoryRepository>();
        _priceRepositoryMock = Substitute.For<IPriceRepository>();
        _appSettingsMock = new AppSettings
        {
            MonitoredAssets = new List<string> { "BTC", "ETH" },
            MonitoredFiats = new List<string> { "USDT", "USDC" }
        };
        _formattersMock = new List<IOutputFormatter> { new TableOutputFormatter(), new JsonOutputFormatter() };
        _serviceProviderMock = Substitute.For<IServiceProvider>();
    }

    [Fact]
    public void SummaryCommand_HasCorrectNameAndDescription()
    {
        // Arrange
        var command = new SummaryCommand(
            _historyRepositoryMock,
            _priceRepositoryMock,
            _appSettingsMock,
            _formattersMock,
            _loggerMock);

        // Assert
        command.Name.Should().Be("summary");
        command.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void SummaryCommand_GetHelp_ReturnsNonEmptyHelpText()
    {
        // Arrange
        var command = new SummaryCommand(
            _historyRepositoryMock,
            _priceRepositoryMock,
            _appSettingsMock,
            _formattersMock,
            _loggerMock);

        // Act
        var helpText = command.GetHelp();

        // Assert
        helpText.Should().NotBeNullOrEmpty();
        helpText.Should().Contain("summary");
        helpText.Should().Contain("--format");
        helpText.Should().Contain("daily price summary");
    }

    [Fact]
    public void ValidateArguments_WithValidFormat_ReturnsEmptyList()
    {
        // Arrange
        var command = new SummaryCommand(
            _historyRepositoryMock,
            _priceRepositoryMock,
            _appSettingsMock,
            _formattersMock,
            _loggerMock);

        var context = new CommandContext
        {
            Options = new Dictionary<string, string> { { "format", "table" } }
        };

        // Act
        var errors = command.ValidateArguments(context);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateArguments_WithInvalidFormat_ReturnsError()
    {
        // Arrange
        var command = new SummaryCommand(
            _historyRepositoryMock,
            _priceRepositoryMock,
            _appSettingsMock,
            _formattersMock,
            _loggerMock);

        var context = new CommandContext
        {
            Options = new Dictionary<string, string> { { "format", "invalid" } }
        };

        // Act
        var errors = command.ValidateArguments(context);

        // Assert
        errors.Should().ContainSingle();
        errors[0].Should().Contain("table");
        errors[0].Should().Contain("json");
    }

    [Fact]
    public void CommandParser_RecognizesSummaryCommand()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<CommandParser>>();
        var parser = new CommandParser(loggerMock);
        var args = new[] { "summary" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("summary");
    }

    [Theory]
    [InlineData("summary")]
    [InlineData("SUMMARY")]
    public void CommandParser_RecognizesSummaryCommandCaseInsensitive(string commandName)
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<CommandParser>>();
        var parser = new CommandParser(loggerMock);
        var args = new[] { commandName };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be(commandName);
    }

    [Fact]
    public void SummaryCommand_WithFormatOption_CommandContextHasCorrectOptions()
    {
        // Arrange
        var loggerMock = Substitute.For<ILogger<CommandParser>>();
        var parser = new CommandParser(loggerMock);
        var args = new[] { "summary", "--format=json" };

        // Act
        var context = parser.Parse(args, _serviceProviderMock);

        // Assert
        context.CommandName.Should().Be("summary");
        context.HasOption("format").Should().BeTrue();
        context.GetOption("format").Should().Be("json");
    }
}
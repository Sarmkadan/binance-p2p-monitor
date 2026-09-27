#nullable enable

using BinanceP2pMonitor.CLI;
using BinanceP2pMonitor.Commands;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Infrastructure;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class AlertCommandTests
{
    private readonly IAlertService _alertServiceMock;
    private readonly ConsoleOutputWriter _outputMock;
    private readonly AppSettings _appSettingsMock;
    private readonly ILogger<AlertCommand> _loggerMock;
    private readonly IServiceProvider _serviceProviderMock;

    public AlertCommandTests()
    {
        _alertServiceMock = Substitute.For<IAlertService>();
        _outputMock = Substitute.For<ConsoleOutputWriter>();
        _appSettingsMock = new AppSettings
        {
            TelegramAdminChatId = "123456789",
            EnableTelegramNotifications = true
        };
        _loggerMock = Substitute.For<ILogger<AlertCommand>>();
        _serviceProviderMock = Substitute.For<IServiceProvider>();

        _serviceProviderMock.GetService(typeof(IAlertService)).Returns(_alertServiceMock);
        _serviceProviderMock.GetService(typeof(ConsoleOutputWriter)).Returns(_outputMock);
        _serviceProviderMock.GetService(typeof(AppSettings)).Returns(_appSettingsMock);
        _serviceProviderMock.GetService(typeof(ILogger<AlertCommand>)).Returns(_loggerMock);
    }

    [Fact]
    public void Constructor_PropertiesAreCorrect()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        command.Name.Should().Be("alert");
        command.Description.Should().Be("Manage price alerts and notifications");
    }

    [Fact]
    public void GetHelp_ReturnsExpectedHelpText()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        var help = command.GetHelp();
        help.Should().NotBeNullOrEmpty();
        help.Should().Contain("alert");
        help.Should().Contain("list");
        help.Should().Contain("create");
        help.Should().Contain("delete");
        help.Should().Contain("test");
    }

    [Fact]
    public void ValidateArguments_ListSubcommand_NoErrors()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        var context = new CommandContext { Arguments = new[] { "list" } };
        var errors = command.ValidateArguments(context);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateArguments_CreateSubcommand_ValidOptions_NoErrors()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        var context = new CommandContext
        {
            Arguments = new[] { "create" },
            Options = new Dictionary<string, string>
            {
                { "asset", "BTC" },
                { "fiat", "USDT" },
                { "type", "price" },
                { "threshold", "50000" },
                { "condition", "above" }
            }
        };
        var errors = command.ValidateArguments(context);
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateArguments_CreateSubcommand_MissingAssetOption_ReturnsError()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        var context = new CommandContext
        {
            Arguments = new[] { "create" },
            Options = new Dictionary<string, string>
            {
                { "fiat", "USDT" },
                { "type", "price" },
                { "threshold", "50000" },
                { "condition", "above" }
            }
        };
        var errors = command.ValidateArguments(context);
        errors.Should().ContainSingle()
            .Which.Should().Contain("--asset is required for create");
    }

    [Fact]
    public void ValidateArguments_CreateSubcommand_InvalidThreshold_ReturnsError()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        var context = new CommandContext
        {
            Arguments = new[] { "create" },
            Options = new Dictionary<string, string>
            {
                { "asset", "BTC" },
                { "fiat", "USDT" },
                { "type", "price" },
                { "threshold", "invalid" },
                { "condition", "above" }
            }
        };
        var errors = command.ValidateArguments(context);
        errors.Should().ContainSingle()
            .Which.Should().Contain("Invalid value for --threshold. Must be a number.");
    }

    [Fact]
    public void ValidateArguments_CreateSubcommand_InvalidCondition_ReturnsError()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        var context = new CommandContext
        {
            Arguments = new[] { "create" },
            Options = new Dictionary<string, string>
            {
                { "asset", "BTC" },
                { "fiat", "USDT" },
                { "type", "price" },
                { "threshold", "50000" },
                { "condition", "invalid" }
            }
        };
        var errors = command.ValidateArguments(context);
        errors.Should().ContainSingle()
            .Which.Should().Contain("Invalid value for --condition");
    }

    [Fact]
    public async Task ExecuteAsync_ListSubcommand_ReturnsSuccess()
    {
        var command = new AlertCommand(_alertServiceMock, _outputMock, _appSettingsMock, _loggerMock);
        _alertServiceMock.GetUserAlertsAsync(Arg.Any<int>()).Returns(new List<BinanceP2pMonitor.Models.PriceAlert>());
        var context = new CommandContext
        {
            Arguments = new[] { "list" },
            ServiceProvider = _serviceProviderMock
        };
        var result = await command.ExecuteAsync(context);
        result.Should().Be(0);
        _outputMock.Received(1).WriteSection(Arg.Any<string>());
        _outputMock.Received(1).WriteInfo(Arg.Any<string>());
    }
}
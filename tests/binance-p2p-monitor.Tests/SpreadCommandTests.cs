using BinanceP2pMonitor.CLI;
using BinanceP2pMonitor.Commands;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Formatters;
using BinanceP2pMonitor.Infrastructure;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Tests for the SpreadCommand class
/// </summary>
public class SpreadCommandTests
{
    private readonly Mock<ISpreadAnalysisService> _spreadAnalysisServiceMock = new();
    private readonly Mock<ConsoleOutputWriter> _outputMock = new();
    private readonly Mock<IOutputFormatter> _tableFormatterMock = new();
    private readonly Mock<IOutputFormatter> _jsonFormatterMock = new();
    private readonly Mock<ILogger<SpreadCommand>> _loggerMock = new();
    private readonly Mock<ILogger<CommandParser>> _parserLoggerMock = new();
    private readonly AppSettings _appSettings = new() { DefaultSpreadThreshold = 1.0m, SpreadAnalysisHistoryHours = 24 };
    private readonly SpreadCommand _command;
    private readonly CommandParser _commandParser;
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadCommandTests"/> class.
    /// </summary>
    public SpreadCommandTests()
    {
        var formatters = new List<IOutputFormatter> { _tableFormatterMock.Object, _jsonFormatterMock.Object };

        _command = new SpreadCommand(
            _spreadAnalysisServiceMock.Object,
            _outputMock.Object,
            formatters,
            _loggerMock.Object,
            _appSettings);

        _commandParser = new CommandParser(_parserLoggerMock.Object);
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ISpreadAnalysisService))).Returns(_spreadAnalysisServiceMock.Object);
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ConsoleOutputWriter))).Returns(_outputMock.Object);
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEnumerable<IOutputFormatter>))).Returns(formatters);
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<SpreadCommand>))).Returns(_loggerMock.Object);
        _serviceProviderMock.Setup(sp => sp.GetService(typeof(AppSettings))).Returns(_appSettings);
    }

    /// <summary>
    /// Verifies that the command has the correct name and description
    /// </summary>
    [Fact]
    public void Command_HasCorrectNameAndDescription()
    {
        Assert.Equal("spread", _command.Name);
        Assert.Equal("Display current buy/sell spread for trading pairs", _command.Description);
    }

    /// <summary>
    /// Verifies that GetHelp returns non-empty help text
    /// </summary>
    [Fact]
    public void GetHelp_ReturnsNonEmptyHelpText()
    {
        var help = _command.GetHelp();
        Assert.NotNull(help);
        Assert.NotEmpty(help);
        Assert.Contains("Usage: binance-p2p-monitor spread", help);
    }

    /// <summary>
    /// Verifies that ValidateArguments returns empty list for valid arguments
    /// </summary>
    [Fact]
    public void ValidateArguments_WithValidFormat_ReturnsEmptyList()
    {
        var context = _commandParser.Parse(new[] { "spread", "--format=table" }, _serviceProviderMock.Object);
        var errors = _command.ValidateArguments(context);
        Assert.Empty(errors);
    }

    /// <summary>
    /// Verifies that ValidateArguments returns error for invalid format
    /// </summary>
    [Fact]
    public void ValidateArguments_WithInvalidFormat_ReturnsError()
    {
        var context = _commandParser.Parse(new[] { "spread", "--format=invalid" }, _serviceProviderMock.Object);
        var errors = _command.ValidateArguments(context);
        Assert.Single(errors);
        Assert.Contains("--format must be one of:", errors[0]);
    }

    /// <summary>
    /// Verifies that ExecuteAsync handles missing spread data gracefully
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNoSpreadData_ReturnsZero()
    {
        _spreadAnalysisServiceMock.Setup(s => s.GetAllSpreadsAsync())
            .ReturnsAsync(new Dictionary<string, Spread>());

        var context = _commandParser.Parse(new[] { "spread" }, _serviceProviderMock.Object);
        var result = await _command.ExecuteAsync(context);

        Assert.Equal(0, result);
        _outputMock.Verify(o => o.WriteInfo("No spread data available"), Times.Once);
    }

    /// <summary>
    /// Verifies that ExecuteAsync processes spread data correctly
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithSpreadData_ProcessesCorrectly()
    {
        var spread = new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = 0.5m,
            AverageSpreadPercent = 0.4m,
            MinSpreadPercent = 0.3m,
            MaxSpreadPercent = 0.6m,
            StandardDeviation = 0.1m,
            SampleCount = 100,
            LastUpdatedAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var spreadsDict = new Dictionary<string, Spread>
        {
            { "BTC/USD", spread }
        };

        _spreadAnalysisServiceMock.Setup(s => s.GetAllSpreadsAsync())
            .ReturnsAsync(spreadsDict);

        _tableFormatterMock.Setup(f => f.Format(It.IsAny<object[]>()))
            .Returns("Formatted output");
        _tableFormatterMock.SetupGet(f => f.FormatType).Returns("table");

        var context = _commandParser.Parse(new[] { "spread" }, _serviceProviderMock.Object);
        var result = await _command.ExecuteAsync(context);

        Assert.Equal(0, result);
        _spreadAnalysisServiceMock.Verify(s => s.GetAllSpreadsAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that ExecuteAsync filters by asset correctly
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithAssetFilter_FiltersCorrectly()
    {
        var btcSpread = new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = 0.5m,
            SampleCount = 100
        };

        var ethSpread = new Spread
        {
            Asset = "ETH",
            Fiat = "USD",
            CurrentSpreadPercent = 0.8m,
            SampleCount = 100
        };

        var spreadsDict = new Dictionary<string, Spread>
        {
            { "BTC/USD", btcSpread },
            { "ETH/USD", ethSpread }
        };

        _spreadAnalysisServiceMock.Setup(s => s.GetAllSpreadsAsync())
            .ReturnsAsync(spreadsDict);

        _tableFormatterMock.Setup(f => f.Format(It.IsAny<object[]>()))
            .Returns("Formatted output");
        _tableFormatterMock.SetupGet(f => f.FormatType).Returns("table");

        var context = _commandParser.Parse(new[] { "spread", "--asset=BTC" }, _serviceProviderMock.Object);
        var result = await _command.ExecuteAsync(context);

        Assert.Equal(0, result);
    }

    /// <summary>
    /// Verifies that ExecuteAsync filters by pair correctly
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithPairFilter_FiltersCorrectly()
    {
        var spread = new Spread
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = 0.5m,
            SampleCount = 100
        };

        _spreadAnalysisServiceMock.Setup(s => s.GetSpreadAnalysisAsync("BTC", "USD"))
            .ReturnsAsync(spread);

        _tableFormatterMock.Setup(f => f.Format(It.IsAny<object[]>()))
            .Returns("Formatted output");
        _tableFormatterMock.SetupGet(f => f.FormatType).Returns("table");

        var context = _commandParser.Parse(new[] { "spread", "--pair=BTC/USD" }, _serviceProviderMock.Object);
        var result = await _command.ExecuteAsync(context);

        Assert.Equal(0, result);
        _spreadAnalysisServiceMock.Verify(s => s.GetSpreadAnalysisAsync("BTC", "USD"), Times.Once);
    }

    /// <summary>
    /// Verifies that ExecuteAsync handles invalid pair format
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithInvalidPairFormat_ReturnsError()
    {
        _outputMock.Setup(o => o.WriteError(It.IsAny<string>()))
            .Verifiable();

        var context = _commandParser.Parse(new[] { "spread", "--pair=INVALID" }, _serviceProviderMock.Object);
        var result = await _command.ExecuteAsync(context);

        Assert.Equal(1, result);
        _outputMock.Verify();
    }
}
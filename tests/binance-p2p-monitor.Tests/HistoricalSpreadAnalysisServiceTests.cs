#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Events;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class HistoricalSpreadAnalysisServiceTests
{
    private readonly IHistoryRepository _mockHistoryRepository;
    private readonly ISpreadAnalysisService _mockSpreadAnalysisService;
    private readonly IEventBus _mockEventBus;
    private readonly AppSettings _appSettings;
    private readonly ILogger<HistoricalSpreadAnalysisService> _mockLogger;
    private readonly HistoricalSpreadAnalysisService _historicalSpreadAnalysisService;

    public HistoricalSpreadAnalysisServiceTests()
    {
        _mockHistoryRepository = Substitute.For<IHistoryRepository>();
        _mockSpreadAnalysisService = Substitute.For<ISpreadAnalysisService>();
        _mockEventBus = Substitute.For<IEventBus>();
        _appSettings = new AppSettings();
        _mockLogger = Substitute.For<ILogger<HistoricalSpreadAnalysisService>>();
        _historicalSpreadAnalysisService = new HistoricalSpreadAnalysisService(
            _mockHistoryRepository,
            _mockSpreadAnalysisService,
            _mockEventBus,
            _appSettings,
            _mockLogger);
    }

    private static List<PriceHistory> GetSampleHistoryData(string asset, string fiat)
    {
        return new List<PriceHistory>
        {
            new() { RecordedAt = DateTime.UtcNow.AddHours(-5), SpreadPercentage = 1.0m, Asset = asset, Fiat = fiat },
            new() { RecordedAt = DateTime.UtcNow.AddHours(-4), SpreadPercentage = 1.2m, Asset = asset, Fiat = fiat },
            new() { RecordedAt = DateTime.UtcNow.AddHours(-3), SpreadPercentage = 1.1m, Asset = asset, Fiat = fiat },
            new() { RecordedAt = DateTime.UtcNow.AddHours(-2), SpreadPercentage = 1.3m, Asset = asset, Fiat = fiat },
            new() { RecordedAt = DateTime.UtcNow.AddHours(-1), SpreadPercentage = 1.5m, Asset = asset, Fiat = fiat }
        };
    }

    [Fact]
    public async Task AnalyzeHistoricalSpreadAsync_ShouldReturnNull_WhenNoHistory()
    {
        // Arrange
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<PriceHistory>());

        // Act
        var result = await _historicalSpreadAnalysisService.AnalyzeHistoricalSpreadAsync("USDT", "UAH").ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AnalyzeHistoricalSpreadAsync_ShouldReturnReport_WhenHistoryExists()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var history = GetSampleHistoryData(asset, fiat);
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, Arg.Any<int>()).Returns(history);
        _mockSpreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat).Returns(new Spread { CurrentSpreadPercent = 1.4m });

        // Act
        var report = await _historicalSpreadAnalysisService.AnalyzeHistoricalSpreadAsync(asset, fiat).ConfigureAwait(false);

        // Assert
        report.Should().NotBeNull();
        report!.Asset.Should().Be(asset);
        report.Fiat.Should().Be(fiat);
        report.SampleCount.Should().Be(history.Count);
        report.Mean.Should().BeApproximately(1.22m, 0.01m);
        report.StandardDeviation.Should().BeApproximately(0.17m, 0.02m);
        report.CurrentSpread.Should().Be(1.4m);
    }

    [Fact]
    public async Task DetectStatisticalAlertsAsync_ShouldReturnAnomalies_WhenZScoreExceedsThreshold()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var history = GetSampleHistoryData(asset, fiat);
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, Arg.Any<int>()).Returns(history);
        // Make current spread high enough to trigger an anomaly
        _mockSpreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat).Returns(new Spread { CurrentSpreadPercent = 2.5m });

        var pairs = new List<(string Asset, string Fiat)> { (asset, fiat) };
        var zScoreThreshold = 2.0m;

        // Act
        var anomalies = await _historicalSpreadAnalysisService.DetectStatisticalAlertsAsync(pairs, zScoreThreshold).ConfigureAwait(false);

        // Assert
        anomalies.Should().ContainSingle();
        anomalies.First().Asset.Should().Be(asset);
        anomalies.First().Fiat.Should().Be(fiat);
        anomalies.First().ZScore.Should().BeGreaterThanOrEqualTo(zScoreThreshold);
        await _mockEventBus.Received(1).PublishAsync(Arg.Any<SpreadAlertTriggeredEvent>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
    }

    [Fact]
    public async Task DetectStatisticalAlertsAsync_ShouldNotReturnAnomalies_WhenZScoreIsBelowThreshold()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var history = GetSampleHistoryData(asset, fiat);
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, Arg.Any<int>()).Returns(history);
        // Current spread within normal range
        _mockSpreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat).Returns(new Spread { CurrentSpreadPercent = 1.2m });

        var pairs = new List<(string Asset, string Fiat)> { (asset, fiat) };
        var zScoreThreshold = 2.0m;

        // Act
        var anomalies = await _historicalSpreadAnalysisService.DetectStatisticalAlertsAsync(pairs, zScoreThreshold).ConfigureAwait(false);

        // Assert
        anomalies.Should().BeEmpty();
        await _mockEventBus.DidNotReceive().PublishAsync(Arg.Any<SpreadAlertTriggeredEvent>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(0, 1.0)] // Min
    [InlineData(50, 1.2)] // Median
    [InlineData(100, 1.5)] // Max
    public async Task GetSpreadPercentileAsync_ShouldReturnCorrectPercentile(decimal percentile, decimal expectedValue)
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var history = GetSampleHistoryData(asset, fiat);
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, Arg.Any<int>()).Returns(history);

        // Act
        var result = await _historicalSpreadAnalysisService.GetSpreadPercentileAsync(asset, fiat, percentile).ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public async Task GetSpreadPercentileAsync_ShouldThrowArgumentOutOfRangeException_ForInvalidPercentile()
    {
        // Arrange
        var asset = "USDT";
        var fiat = "UAH";
        var history = GetSampleHistoryData(asset, fiat);
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, Arg.Any<int>()).Returns(history);

        // Act
        Func<Task> action = async () => await _historicalSpreadAnalysisService.GetSpreadPercentileAsync(asset, fiat, 101).ConfigureAwait(false);

        // Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("Percentile must be between 0 and 100 (Parameter 'percentile')");
    }

    [Fact]
    public async Task GetRollingWindowAveragesAsync_ShouldReturnEmpty_WhenNoHistory()
    {
        // Arrange
        _mockHistoryRepository.GetHistoryByAssetAndFiatAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(new List<PriceHistory>());

        // Act
        var result = await _historicalSpreadAnalysisService.GetRollingWindowAveragesAsync("USDT", "UAH").ConfigureAwait(false);

        // Assert
        result.Should().BeEmpty();
    }
}

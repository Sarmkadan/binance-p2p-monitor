#nullable enable

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Events;
using BinanceP2pMonitor.Integration;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class IntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;

    public IntegrationTests(TestApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MonitoringWorkflow_ShouldStorePricesAndTriggerSpreadAnalysis()
    {
        // Arrange
        var host = _factory.CreateHost();
        var mockWebSocketService = _factory.MockWebSocketService;
        var mockTelegramBotClientWrapper = _factory.MockTelegramBotClientWrapper;
        var appSettings = _factory.TestAppSettings;

        // Start the hosted service (MonitoringHostedService)
        // This will connect WebSocket (mocked) and start listening for events
        var hostedServices = host.Services.GetServices<IHostedService>();
        var monitoringService = hostedServices.OfType<MonitoringHostedService>().FirstOrDefault();
        monitoringService.Should().NotBeNull();

        await monitoringService!.StartAsync(CancellationToken.None);

        // Get services from the host to verify actions
        var priceRepository = host.Services.GetRequiredService<IPriceRepository>();
        var spreadAnalysisService = host.Services.GetRequiredService<ISpreadAnalysisService>();
        var alertService = host.Services.GetRequiredService<IAlertService>();
        var dbContext = host.Services.GetRequiredService<DatabaseContext>();

        // Ensure the database is clean before test
        dbContext.Price.RemoveRange(dbContext.Price);
        await dbContext.SaveChangesAsync();

        // 1. Simulate an initial price update
        var initialPriceEvent = new PriceUpdatedEvent
        (
            Asset: "USDT",
            Fiat: "UAH",
            TradeType: TradeType.Buy,
            Offers: new List<TradeOffer>
            {
                new TradeOffer { Price = 38.0m, Available = 1000m, Min = 100, Max = 10000, Nickname = "UserA" },
                new TradeOffer { Price = 38.1m, Available = 2000m, Min = 100, Max = 20000, Nickname = "UserB" }
            }
        );
        await mockWebSocketService.TriggerPriceUpdate(initialPriceEvent);

        // Give some time for async operations to complete
        await Task.Delay(100);

        // Assert initial price storage
        var storedPrices = await priceRepository.GetAllActiveAsync();
        storedPrices.Should().ContainSingle(p => p.Asset == "USDT" && p.Fiat == "UAH");
        storedPrices.First().BuyPrice.Should().Be(38.1m); // Highest buy
        storedPrices.First().SellPrice.Should().Be(38.0m); // Lowest sell

        // Assert initial spread analysis
        var initialSpread = await spreadAnalysisService.GetSpreadAnalysisAsync("USDT", "UAH");
        initialSpread.Should().NotBeNull();
        initialSpread!.CurrentSpreadPercent.Should().BeApproximately(-0.26m, 0.01m); // ((38.0 - 38.1) / 38.1) * 100

        // 2. Simulate a price update that triggers an alert (e.g., high spread)
        // Adjust appSettings threshold to make it easier to trigger
        appSettings.DefaultSpreadThreshold = -0.5m; // Set a low threshold to ensure positive spreads are detected
        appSettings.PriceAlertConditions.Add(new PriceAlertCondition
        {
            Asset = "USDT",
            Fiat = "UAH",
            Threshold = 0.0m, // Alert if spread is positive
            AlertType = AlertType.Spread,
            ComparisonType = AlertCondition.GreaterThan
        });
        
        var alertTriggeringPriceEvent = new PriceUpdatedEvent
        (
            Asset: "USDT",
            Fiat: "UAH",
            TradeType: TradeType.Buy,
            Offers: new List<TradeOffer>
            {
                new TradeOffer { Price = 38.0m, Available = 1000m, Min = 100, Max = 10000, Nickname = "UserX" },
            },
            SellOffers: new List<TradeOffer>
            {
                new TradeOffer { Price = 38.5m, Available = 2000m, Min = 100, Max = 20000, Nickname = "UserY" }
            }
        );
        await mockWebSocketService.TriggerPriceUpdate(alertTriggeringPriceEvent);

        await Task.Delay(200); // Give time for alert processing

        // Assert spread analysis update after second event
        var updatedSpread = await spreadAnalysisService.GetSpreadAnalysisAsync("USDT", "UAH");
        updatedSpread.Should().NotBeNull();
        updatedSpread!.CurrentSpreadPercent.Should().BeApproximately(1.31m, 0.01m); // ((38.5 - 38.0) / 38.0) * 100

        // Assert Telegram alert was attempted
        await mockTelegramBotClientWrapper.Received(1).SendTextMessageAsync(
            long.Parse(appSettings.TelegramAdminChatId),
            Arg.Is<string>(s => s.Contains("Price Alert: USDT/UAH") && s.Contains("Reason: Spread (1.31%)")),
            Telegram.Bot.Types.Enums.ParseMode.Html,
            Arg.Any<IEnumerable<MessageEntity>>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<Telegram.Bot.Types.ReplyMarkups.IReplyMarkup>(),
            Arg.Any<CancellationToken>());

        // Stop the hosted service
        await monitoringService.StopAsync(CancellationToken.None);
        await host.StopAsync(); // Stop the host
        host.Dispose(); // Dispose the host
    }

    [Fact]
    public async Task MainUseCase_TelegramAlertForHighSpread_ShouldWork()
    {
        // Arrange
        var factory = new TestApplicationFactory(); // Create a new factory for this test to ensure isolation
        var host = factory.CreateHost();
        var mockWebSocketService = factory.MockWebSocketService;
        var mockTelegramBotClientWrapper = factory.MockTelegramBotClientWrapper;
        var appSettings = factory.TestAppSettings;

        // Configure alert condition for this specific test
        appSettings.PriceAlertConditions.Clear();
        appSettings.PriceAlertConditions.Add(new PriceAlertCondition
        {
            Asset = "BTC",
            Fiat = "USD",
            Threshold = 0.5m, // Alert if spread is > 0.5%
            AlertType = AlertType.Spread,
            ComparisonType = AlertCondition.GreaterThan
        });

        var hostedServices = host.Services.GetServices<IHostedService>();
        var monitoringService = hostedServices.OfType<MonitoringHostedService>().FirstOrDefault();
        monitoringService.Should().NotBeNull();
        await monitoringService!.StartAsync(CancellationToken.None);

        var priceRepository = host.Services.GetRequiredService<IPriceRepository>();
        var dbContext = host.Services.GetRequiredService<DatabaseContext>();

        // Ensure the database is clean before test
        dbContext.Price.RemoveRange(dbContext.Price);
        await dbContext.SaveChangesAsync();

        // Simulate a price event with a spread that should trigger an alert
        var alertTriggeringPriceEvent = new PriceUpdatedEvent
        (
            Asset: "BTC",
            Fiat: "USD",
            TradeType: TradeType.Buy,
            Offers: new List<TradeOffer>
            {
                new TradeOffer { Price = 60000m, Available = 10m, Min = 100, Max = 100000, Nickname = "Buyer1" },
            },
            SellOffers: new List<TradeOffer>
            {
                new TradeOffer { Price = 60500m, Available = 5m, Min = 100, Max = 50000, Nickname = "Seller1" }
            }
        );
        await mockWebSocketService.TriggerPriceUpdate(alertTriggeringPriceEvent);

        await Task.Delay(200); // Allow time for event processing and alert sending

        // Verify price was stored
        var storedPrice = await priceRepository.GetLatestByAssetAndFiatAsync("BTC", "USD");
        storedPrice.Should().NotBeNull();
        storedPrice!.BuyPrice.Should().Be(60500m); // Highest buy
        storedPrice.SellPrice.Should().Be(60000m); // Lowest sell

        // Verify Telegram alert was sent
        await mockTelegramBotClientWrapper.Received(1).SendTextMessageAsync(
            long.Parse(appSettings.TelegramAdminChatId),
            Arg.Is<string>(s => s.Contains("Price Alert: BTC/USD") && s.Contains("Reason: Spread")),
            Telegram.Bot.Types.Enums.ParseMode.Html,
            Arg.Any<IEnumerable<MessageEntity>>(),
            Arg.Any<bool?>(),
            Arg.Any<bool?>(),
            Arg.Any<int?>(),
            Arg.Any<bool?>(),
            Arg.Any<Telegram.Bot.Types.ReplyMarkups.IReplyMarkup>(),
            Arg.Any<CancellationToken>());

        await monitoringService.StopAsync(CancellationToken.None);
        await host.StopAsync();
        host.Dispose();
    }
}

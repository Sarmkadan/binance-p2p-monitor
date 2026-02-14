#nullable enable

using BinanceP2pMonitor;
using BinanceP2pMonitor.Caching;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Events;
using BinanceP2pMonitor.Integration;
using BinanceP2pMonitor.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Data.Common;

namespace BinanceP2pMonitor.Tests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    public AppSettings TestAppSettings { get; private set; } = null!;
    public ITelegramBotClientWrapper MockTelegramBotClientWrapper { get; private set; } = null!;
    public MockWebSocketService MockWebSocketService { get; private set; } = null!;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Setup AppSettings for testing
        TestAppSettings = new AppSettings
        {
            DatabaseConnectionString = "DataSource=:memory:", // In-memory SQLite
            TelegramBotToken = "test_telegram_token",
            TelegramAdminChatId = "123456789",
            SpreadAnalysisHistoryHours = 1, // Shorter history for faster tests
            DefaultSpreadThreshold = 0.1m,
            P2PApiBaseUrl = "http://localhost:5000",
            P2PSocketBaseUrl = "ws://localhost:5000"
        };

        MockTelegramBotClientWrapper = Substitute.For<ITelegramBotClientWrapper>();
        MockWebSocketService = new MockWebSocketService();

        builder.ConfigureServices(services =>
        {
            // Replace AppSettings
            services.AddSingleton(TestAppSettings);

            // Replace DatabaseContext with in-memory SQLite
            var dbConnection = new SqliteConnection("DataSource=:memory:");
            dbConnection.Open();
            services.AddSingleton<DbConnection>(dbConnection);

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlite(dbConnection);
            });

            // Ensure DatabaseContext is initialized with test connection
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<DatabaseContext>();
                db.Database.EnsureDeleted(); // Clear any previous in-memory data
                db.Database.EnsureCreated(); // Create schema
            }

            // Mock IWebSocketService
            services.AddSingleton<IWebSocketService>(MockWebSocketService);

            // Mock ITelegramBotClientWrapper
            services.AddSingleton<ITelegramBotClientWrapper>(MockTelegramBotClientWrapper);

            // Mock ICache for rate limiting if needed
            services.AddSingleton<ICache>(Substitute.For<ICache>());

            // Optionally, mock other services or use concrete implementations
            // Depending on the scope of the integration test
            services.AddSingleton<ILogger<MonitoringHostedService>>(Substitute.For<ILogger<MonitoringHostedService>>());
            services.AddSingleton<ILogger<PriceMonitoringService>>(Substitute.For<ILogger<PriceMonitoringService>>());
            services.AddSingleton<ILogger<SpreadAnalysisService>>(Substitute.For<ILogger<SpreadAnalysisService>>());
            services.AddSingleton<ILogger<AlertService>>(Substitute.For<ILogger<AlertService>>());
            services.AddSingleton<ILogger<TelegramNotificationClient>>(Substitute.For<ILogger<TelegramNotificationClient>>());
            services.AddSingleton<ILogger<DatabaseContext>>(Substitute.For<ILogger<DatabaseContext>>());

            // Add hosted services for integration testing
            services.AddHostedService<MonitoringHostedService>();
        });

        // The base CreateHost will now use our configured builder
        return base.CreateHost(builder);
    }

    /// <summary>
    /// Mock IWebSocketService to manually trigger price updates
    /// </summary>
    public class MockWebSocketService : IWebSocketService
    {
        public event Func<PriceUpdatedEvent, Task>? OnPriceUpdated;
        public Task ConnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task DisconnectAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task TriggerPriceUpdate(PriceUpdatedEvent priceUpdate)
        {
            if (OnPriceUpdated != null)
            {
                await OnPriceUpdated.Invoke(priceUpdate);
            }
        }
    }
}

#nullable enable
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BinanceP2pMonitor.Backtesting;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Extensions;
using BinanceP2pMonitor.Integration;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Data;

namespace BinanceP2pMonitor;

sealed class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices(ConfigureServices)
            .Build();

        try
        {
            // Initialize database
            var dbContext = host.Services.GetRequiredService<DatabaseContext>();
            dbContext.Initialize();

            // Register commands after host is built
            var commandFactory = host.Services.GetRequiredService<CommandFactory>();
            commandFactory.RegisterCommand("monitor", typeof(MonitorCommand));
            commandFactory.RegisterCommand("status", typeof(StatusCommand));
            commandFactory.RegisterCommand("help", typeof(HelpCommand));
            commandFactory.RegisterCommand("alert", typeof(AlertCommand));
            commandFactory.RegisterCommand("summary", typeof(SummaryCommand));
            commandFactory.RegisterCommand("history", typeof(HistoryCommand));
            commandFactory.RegisterCommand("export", typeof(ExportCommand));
            commandFactory.RegisterCommand("version", typeof(VersionCommand));
            commandFactory.RegisterCommand("backtest", typeof(BacktestCommand));
            commandFactory.RegisterCommand("spread", typeof(SpreadCommand));
            commandFactory.RegisterCommand("compare", typeof(CompareCommand));
            commandFactory.RegisterCommand("doctor", typeof(DoctorCommand));
            commandFactory.RegisterCommand("prune", typeof(PruneCommand));

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Application terminated with error");
            throw;
        }
        finally
        {
            host.Dispose();
        }
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Register configuration
        var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>()
            ?? throw new InvalidOperationException("AppSettings section not found in configuration");
        services.AddSingleton(appSettings);

        // Register database
        services.AddSingleton<DatabaseContext>();
        services.AddScoped<IDbConnection>(_ => new SqliteConnection(appSettings.DatabaseConnectionString));

        // Register repositories
        services.AddScoped<IPriceRepository, PriceRepository>();
        services.AddScoped<ITradeOfferRepository, TradeOfferRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IHistoryRepository, HistoryRepository>();

        // Register services
        services.AddScoped<IDatabaseCleanupService, DatabaseCleanupService>();

        // Register services
        services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<ISpreadAnalysisService, SpreadAnalysisService>();
        services.AddScoped<IPriceHistoryService, PriceHistoryService>();
        services.AddScoped<IWebSocketService, WebSocketService>();
        services.AddScoped<IHistoricalSpreadAnalysisService, HistoricalSpreadAnalysisService>();

        // Register caching
        services.AddSingleton<ICache, MemoryCache>();

        // Register event bus
        services.AddSingleton<IEventBus, EventBus>();

        // Register CLI infrastructure
        services.AddSingleton<CommandParser>();
        services.AddSingleton<CommandFactory>();
        services.AddSingleton<ConsoleOutputWriter>();

        // Register HTTP client and integration services
        services.AddHttpClient();
        services.AddSingleton<BinanceP2pMonitor.Integration.HttpClientFactory>();
        services.AddSingleton<ITelegramNotificationClient, TelegramNotificationClient>();
        services.AddSingleton<IWebhookNotificationClient, WebhookNotificationClient>();

        // Register rate limiter
        services.AddSingleton(new RateLimiter(100, TimeSpan.FromMinutes(1)));

        // Register output formatters
        services.AddSingleton<IOutputFormatter, JsonOutputFormatter>();
        services.AddSingleton<IOutputFormatter, TableOutputFormatter>();
        services.AddSingleton<IOutputFormatter, CsvOutputFormatter>();
        services.AddSingleton<IOutputFormatter, MarkdownOutputFormatter>();

        // Register infrastructure utilities
        services.AddSingleton<ConfigurationValidator>();
        services.AddSingleton<DataExporter>();
        services.AddSingleton<PerformanceMetrics>();

        // Register backtesting
        services.AddBacktesting();

        // Register hosted services
        services.AddHostedService<MonitoringHostedService>();
        services.AddHostedService<StatisticsCollectorWorker>();
        services.AddHostedService<DatabaseCleanupWorker>();
        services.AddHostedService<DailySummaryService>();

        // Register logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
    }
}

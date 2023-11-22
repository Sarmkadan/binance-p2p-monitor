// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Repositories;
using BinanceP2pMonitor.Data;

namespace BinanceP2pMonitor;

class Program
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
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                var appSettings = context.Configuration.GetSection("AppSettings").Get<AppSettings>()
                    ?? throw new InvalidOperationException("AppSettings section not found in configuration");
                services.AddSingleton(appSettings);

                // Register database
                services.AddSingleton<DatabaseContext>();
                services.AddScoped<IDbConnection>(_ => new SQLiteConnection(appSettings.DatabaseConnectionString));

                // Register repositories
                services.AddScoped<IPriceRepository, PriceRepository>();
                services.AddScoped<ITradeOfferRepository, TradeOfferRepository>();
                services.AddScoped<IAlertRepository, AlertRepository>();
                services.AddScoped<IHistoryRepository, HistoryRepository>();

                // Register services
                services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
                services.AddScoped<IAlertService, AlertService>();
                services.AddScoped<ISpreadAnalysisService, SpreadAnalysisService>();
                services.AddScoped<IPriceHistoryService, PriceHistoryService>();
                services.AddScoped<IWebSocketService, WebSocketService>();

                // Register hosted service
                services.AddHostedService<MonitoringHostedService>();

                // Register logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();

        try
        {
            // Initialize database
            var dbContext = host.Services.GetRequiredService<DatabaseContext>();
            dbContext.Initialize();

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
}

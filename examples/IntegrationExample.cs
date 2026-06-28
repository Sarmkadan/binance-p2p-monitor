using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BinanceP2pMonitor;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Configuration;

// Integration example: Wiring into an existing ASP.NET Core or Worker service DI container
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Add binance-p2p-monitor services to your own project
        services.AddSingleton(new AppSettings { DatabaseConnectionString = "Data Source=app.db" });
        services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
        
        // ... Register all other required dependencies found in Program.cs ...
        
        // Example: Register a hosted service from the library
        services.AddHostedService<MonitoringHostedService>();
    }
}

using Microsoft.Extensions.DependencyInjection;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Configuration;

// Basic usage example: Configuring the required services
var services = new ServiceCollection();

// Setup minimal configuration
var appSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=binance_p2p.db"
};
services.AddSingleton(appSettings);

// Register necessary services
services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
// ... register other required services (Repositories, etc.)

var serviceProvider = services.BuildServiceProvider();

// Resolve and use a service
var monitoringService = serviceProvider.GetRequiredService<IPriceMonitoringService>();
Console.WriteLine("Price monitoring service initialized.");

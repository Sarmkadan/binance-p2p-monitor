using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Infrastructure;

// Advanced usage example: Custom configuration and error handling
var services = new ServiceCollection();

// Advanced configuration
var appSettings = new AppSettings
{
    DatabaseConnectionString = "Data Source=advanced_monitor.db",
    // Add additional settings here
};
services.AddSingleton(appSettings);

services.AddLogging(builder => builder.AddConsole());

// Register services with advanced options
services.AddScoped<IPriceMonitoringService, PriceMonitoringService>();
services.AddSingleton<IRateLimiter>(new RateLimiter(50, TimeSpan.FromMinutes(1)));

var serviceProvider = services.BuildServiceProvider();

try
{
    var monitoringService = serviceProvider.GetRequiredService<IPriceMonitoringService>();
    // Perform complex operations
    Console.WriteLine("Advanced monitoring service started with custom rate limiting.");
}
catch (Exception ex)
{
    // Handle configuration or service errors
    Console.WriteLine($"Error initializing monitoring service: {ex.Message}");
}

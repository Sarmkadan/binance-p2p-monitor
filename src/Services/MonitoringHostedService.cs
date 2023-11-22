// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Background service for continuous price monitoring and alert processing
/// </summary>
public class MonitoringHostedService : BackgroundService
{
    private readonly IPriceMonitoringService _priceMonitoringService;
    private readonly IPriceHistoryService _historyService;
    private readonly AppSettings _settings;
    private readonly ILogger<MonitoringHostedService> _logger;
    private DateTime _lastCleanupTime;

    public MonitoringHostedService(
        IPriceMonitoringService priceMonitoringService,
        IPriceHistoryService historyService,
        AppSettings settings,
        ILogger<MonitoringHostedService> logger)
    {
        _priceMonitoringService = priceMonitoringService ?? throw new ArgumentNullException(nameof(priceMonitoringService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lastCleanupTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Executes the monitoring background service
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitoring hosted service starting");

        try
        {
            // Start the price monitoring service
            var monitoringTask = _priceMonitoringService.StartMonitoringAsync(stoppingToken);

            // Main monitoring loop
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Get and update all monitored prices
                    var prices = await _priceMonitoringService.GetAllCurrentPricesAsync();

                    foreach (var price in prices)
                    {
                        try
                        {
                            await _priceMonitoringService.UpdatePriceAsync(price);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error updating price for {Asset}/{Fiat}", price.Asset, price.Fiat);
                        }
                    }

                    // Perform periodic cleanup
                    if ((DateTime.UtcNow - _lastCleanupTime).TotalHours >= 1)
                    {
                        await PerformCleanupAsync();
                        _lastCleanupTime = DateTime.UtcNow;
                    }

                    // Wait for next monitoring interval
                    await Task.Delay(_settings.MonitoringIntervalSeconds * 1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Monitoring operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitoring service loop");
                    await Task.Delay(5000, stoppingToken); // Brief delay before retry
                }
            }

            await monitoringTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in monitoring hosted service");
            throw;
        }
    }

    /// <summary>
    /// Performs periodic cleanup and maintenance
    /// </summary>
    private async Task PerformCleanupAsync()
    {
        try
        {
            _logger.LogInformation("Starting periodic cleanup");

            if (_settings.EnableAutoCleanup)
            {
                // Clean up old history records
                var success = await _historyService.CleanupOldHistoryAsync(_settings.HistoryRetentionDays);

                if (success)
                {
                    var totalCount = await _historyService.GetHistoryCountAsync();
                    _logger.LogInformation("Cleanup completed. Total history records: {Count}", totalCount);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
        }
    }

    /// <summary>
    /// Handles graceful shutdown
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Monitoring hosted service stopping");
        await _priceMonitoringService.StopMonitoringAsync();
        await base.StopAsync(cancellationToken);
    }
}

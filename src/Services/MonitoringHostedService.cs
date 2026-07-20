#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Events;
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
    private readonly IEventBus _eventBus;

    private DateTime _lastCleanupTime;
    private readonly DateTime _serviceStartTime;
    private DateTime _lastSuccessfulFetch;

    public MonitoringHostedService(
        IPriceMonitoringService priceMonitoringService,
        IPriceHistoryService historyService,
        AppSettings settings,
        ILogger<MonitoringHostedService> logger,
        IEventBus eventBus)
    {
        _priceMonitoringService = priceMonitoringService ?? throw new ArgumentNullException(nameof(priceMonitoringService));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _lastCleanupTime = DateTime.UtcNow;
        _serviceStartTime = DateTime.UtcNow;
        _lastSuccessfulFetch = DateTime.UtcNow;

        // Subscribe to HeartbeatEvent for logging
        _eventBus.Subscribe<HeartbeatEvent>(HandleHeartbeatAsync);
    }

    /// <summary>
    /// Executes the monitoring background service
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Monitoring hosted service starting");

        try
        {
            // Start the price monitoring service (this will setup WebSocket subscriptions)
            await _priceMonitoringService.StartMonitoringAsync(stoppingToken).ConfigureAwait(false);

            // Main monitoring loop for periodic tasks like cleanup and heartbeat publishing
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Perform periodic cleanup
                    if ((DateTime.UtcNow - _lastCleanupTime).TotalHours >= 1)
                    {
                        await PerformCleanupAsync().ConfigureAwait(false);
                        _lastCleanupTime = DateTime.UtcNow;
                    }

                    // Publish heartbeat event
                    await PublishHeartbeatAsync().ConfigureAwait(false);

                    // Wait for next monitoring interval. This interval now primarily governs cleanup frequency
                    // and keeps the hosted service alive.
                    await Task.Delay(TimeSpan.FromSeconds(_settings.MonitoringIntervalSeconds), stoppingToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Monitoring operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in monitoring service loop (periodic tasks)");
                    await Task.Delay(5000, stoppingToken).ConfigureAwait(false); // Brief delay before retry
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in monitoring hosted service startup");
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
                var success = await _historyService.CleanupOldHistoryAsync(_settings.HistoryRetentionDays).ConfigureAwait(false);

                if (success)
                {
                    var totalCount = await _historyService.GetHistoryCountAsync().ConfigureAwait(false);
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
    /// Publishes a HeartbeatEvent containing uptime and the timestamp of the last successful fetch.
    /// </summary>
    private async Task PublishHeartbeatAsync()
    {
        var uptime = DateTime.UtcNow - _serviceStartTime;
        var heartbeat = new HeartbeatEvent(uptime, _lastSuccessfulFetch);
        await _eventBus.PublishAsync(heartbeat).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles a HeartbeatEvent by logging its details.
    /// </summary>
    private Task HandleHeartbeatAsync(HeartbeatEvent ev, CancellationToken ct)
    {
        _logger.LogInformation(
            "Heartbeat – Uptime: {Uptime:g}, LastSuccessfulFetch: {LastFetch:u}",
            ev.Uptime,
            ev.LastSuccessfulFetch);

        // No asynchronous work required; return a completed task.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles graceful shutdown
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Monitoring hosted service stopping");
        await _priceMonitoringService.StopMonitoringAsync().ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}

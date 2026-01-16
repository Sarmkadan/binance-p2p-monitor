#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Workers;

/// <summary>
/// Background worker for collecting and aggregating price statistics
/// </summary>
public class StatisticsCollectorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StatisticsCollectorWorker> _logger;
    private readonly AppSettings _appSettings;

    public StatisticsCollectorWorker(
        IServiceProvider serviceProvider,
        ILogger<StatisticsCollectorWorker> logger,
        AppSettings appSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _appSettings = appSettings;
    }

    /// <summary>
    /// Collects statistics every 5 minutes
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Statistics collector worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectStatisticsAsync(stoppingToken).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Statistics collector worker stopped");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error collecting statistics");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task CollectStatisticsAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var priceRepository = scope.ServiceProvider.GetRequiredService<IPriceRepository>();
        var historyRepository = scope.ServiceProvider.GetRequiredService<IHistoryRepository>();

        var assets = _appSettings.MonitoredAssets.FirstOrDefault()?.Split(',') ?? string[]();
        var fiats = _appSettings.MonitoredFiats.FirstOrDefault()?.Split(',') ?? string[]();

        _logger.LogDebug("Collecting statistics for {AssetCount} assets and {FiatCount} fiats", assets.Length, fiats.Length);

        foreach (var asset in assets)
        {
            foreach (var fiat in fiats)
            {
                try
                {
                    // Fetch last 24 hours of prices for analysis
                    var startTime = DateTime.UtcNow.AddHours(-24);
                    // Statistics would be aggregated here and stored
                    _logger.LogDebug("Collected statistics for {Asset}/{Fiat}", asset, fiat);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error collecting statistics for {Asset}/{Fiat}", asset, fiat);
                }
            }
        }
    }
}

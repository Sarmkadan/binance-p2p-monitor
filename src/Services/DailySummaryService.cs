#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Integration;
using BinanceP2pMonitor.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Background service that sends a daily price summary via Telegram at a configurable UTC hour.
/// Configure the send time with <c>AppSettings.DailySummaryHourUtc</c> (0–23).
/// Set it to <c>-1</c> to disable this service entirely.
/// </summary>
public class DailySummaryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _settings;
    private readonly ILogger<DailySummaryService> _logger;

    public DailySummaryService(
        IServiceProvider serviceProvider,
        AppSettings settings,
        ILogger<DailySummaryService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_settings.DailySummaryHourUtc < 0)
        {
            _logger.LogInformation("Daily summary service disabled (DailySummaryHourUtc = -1)");
            return;
        }

        if (!_settings.EnableTelegramNotifications)
        {
            _logger.LogInformation("Daily summary service disabled (Telegram notifications are off)");
            return;
        }

        _logger.LogInformation("Daily summary service started. Summary will be sent at {Hour:D2}:00 UTC",
            _settings.DailySummaryHourUtc);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun(_settings.DailySummaryHourUtc);
            _logger.LogDebug("Next daily summary in {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await SendDailySummaryAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending daily price summary");
            }
        }

        _logger.LogInformation("Daily summary service stopped");
    }

    private async Task SendDailySummaryAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var historyRepository = scope.ServiceProvider.GetRequiredService<IHistoryRepository>();
        var priceRepository = scope.ServiceProvider.GetRequiredService<IPriceRepository>();
        var telegram = scope.ServiceProvider.GetRequiredService<TelegramNotificationClient>();

        var lines = new System.Text.StringBuilder();
        lines.AppendLine("<b>📊 Daily Price Summary</b>");
        lines.AppendLine($"<i>{DateTime.UtcNow:yyyy-MM-dd} · 24 h window · UTC</i>");
        lines.AppendLine();

        bool hasData = false;

        foreach (var asset in _settings.MonitoredAssets)
        {
            foreach (var fiat in _settings.MonitoredFiats)
            {
                try
                {
                    var history = (await historyRepository
                        .GetHistoryByAssetAndFiatAsync(asset, fiat, 24)
                        .ConfigureAwait(false)).ToList();

                    if (history.Count == 0)
                        continue;

                    hasData = true;

                    var buyPrices = history.Select(h => h.BuyPrice).ToList();
                    var sellPrices = history.Select(h => h.SellPrice).ToList();
                    var allPrices = buyPrices.Concat(sellPrices).ToList();

                    var minPrice = allPrices.Min();
                    var maxPrice = allPrices.Max();
                    var avgPrice = allPrices.Average();

                    var latest = await priceRepository
                        .GetLatestByAssetAndFiatAsync(asset, fiat)
                        .ConfigureAwait(false);

                    var currentBuy = latest?.BuyPrice ?? buyPrices.Last();
                    var currentSell = latest?.SellPrice ?? sellPrices.Last();

                    lines.AppendLine($"<b>{asset}/{fiat}</b>");
                    lines.AppendLine($"  Min: {minPrice:F2}  Max: {maxPrice:F2}  Avg: {avgPrice:F2}");
                    lines.AppendLine($"  Current — Buy: {currentBuy:F2}  Sell: {currentSell:F2}");
                    lines.AppendLine();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to collect summary data for {Asset}/{Fiat}", asset, fiat);
                }
            }
        }

        if (!hasData)
        {
            _logger.LogInformation("No price history available for daily summary — skipping Telegram message");
            return;
        }

        var message = lines.ToString();

        if (!long.TryParse(_settings.TelegramAdminChatId, out var chatId))
        {
            _logger.LogWarning("Invalid TelegramAdminChatId — cannot send daily summary");
            return;
        }

        await telegram.SendMessageAsync(chatId, message, ct).ConfigureAwait(false);
        _logger.LogInformation("Daily price summary sent");
    }

    /// <summary>
    /// Returns the <see cref="TimeSpan"/> to wait until the next occurrence of <paramref name="targetHour"/> UTC.
    /// </summary>
    private static TimeSpan GetDelayUntilNextRun(int targetHour)
    {
        var now = DateTime.UtcNow;
        var next = new DateTime(now.Year, now.Month, now.Day, targetHour, 0, 0, DateTimeKind.Utc);

        if (next <= now)
            next = next.AddDays(1);

        return next - now;
    }
}

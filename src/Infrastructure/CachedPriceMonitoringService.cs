#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Decorator that adds caching to price monitoring service
/// </summary>
public class CachedPriceMonitoringService : IPriceMonitoringService
{
    private readonly IPriceMonitoringService _innerService;
    private readonly ICache _cache;
    private readonly ILogger<CachedPriceMonitoringService> _logger;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

    public CachedPriceMonitoringService(
        IPriceMonitoringService innerService,
        ICache cache,
        ILogger<CachedPriceMonitoringService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Price?> GetCurrentPriceAsync(string asset, string fiat)
    {
        var cacheKey = $"price_{asset}_{fiat}";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token => await _innerService.GetCurrentPriceAsync(asset, fiat),
            _cacheDuration);
    }

    public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync()
    {
        var cacheKey = "all_prices";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token => (await _innerService.GetAllCurrentPricesAsync()).ToList() as IEnumerable<Price>,
            _cacheDuration) ?? Enumerable.Empty<Price>();
    }

    public async Task<bool> UpdatePriceAsync(Price price)
    {
        var result = await _innerService.UpdatePriceAsync(price);
        if (result)
        {
            await _cache.RemoveAsync($"price_{price.Asset}_{price.Fiat}");
            await _cache.RemoveAsync("all_prices");
            _logger.LogDebug("Cache invalidated for {Asset}/{Fiat}", price.Asset, price.Fiat);
        }
        return result;
    }

    public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours)
    {
        var cacheKey = $"avg_price_{asset}_{fiat}_{hours}h";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token => await _innerService.GetAveragePriceAsync(asset, fiat, hours),
            TimeSpan.FromMinutes(5));
    }

    public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold)
    {
        return await _innerService.GetPricesWithSignificantChangeAsync(changePercentThreshold);
    }

    public async Task<Dictionary<string, decimal>> GetSpreadAnalysisAsync(string asset, string fiat)
    {
        var cacheKey = $"spread_{asset}_{fiat}";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async token => await _innerService.GetSpreadAnalysisAsync(asset, fiat),
            _cacheDuration) ?? new Dictionary<string, decimal>();
    }

    public async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        await _innerService.StartMonitoringAsync(cancellationToken);
    }

    public async Task StopMonitoringAsync()
    {
        await _cache.ClearAsync();
        await _innerService.StopMonitoringAsync();
    }
}

#nullable enable
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Exceptions;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Decorator that adds caching to price monitoring service
/// </summary>
public class CachedPriceMonitoringService : IPriceMonitoringService
{
	private const string PriceCacheKeyFormat = "price_{0}_{1}";
	private const string AllPricesCacheKey = "all_prices";
	private const string AveragePriceCacheKeyFormat = "avg_price_{0}_{1}_{2}h";
	private const string SpreadCacheKeyFormat = "spread_{0}_{1}";

	private const string AssetNullOrWhitespaceMessage = "Asset cannot be null or whitespace";
	private const string FiatNullOrWhitespaceMessage = "Fiat cannot be null or whitespace";
	private const string PriceAssetAndFiatMustBeSpecifiedMessage = "Price asset and fiat must be specified";
	private const string HoursMustBePositiveMessage = "Hours must be positive";
	private const string ThresholdCannotBeNegativeMessage = "Threshold cannot be negative";
	private const string FailedToGetCachedPriceMessage = "Failed to get cached price";
	private const string FailedToGetAllCachedPricesMessage = "Failed to get all cached prices";
	private const string FailedToUpdateCachedPriceMessage = "Failed to update cached price";
	private const string FailedToGetCachedAveragePriceMessage = "Failed to get cached average price";
	private const string FailedToGetPricesWithSignificantChangeMessage = "Failed to get prices with significant change";
	private const string FailedToGetCachedSpreadAnalysisMessage = "Failed to get cached spread analysis";
	private const string FailedToStartMonitoringMessage = "Failed to start monitoring";
	private const string FailedToStopMonitoringMessage = "Failed to stop monitoring";
	private const string CacheInvalidatedMessage = "Cache invalidated for {Asset}/{Fiat}";

	private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan AveragePriceCacheDuration = TimeSpan.FromMinutes(5);

	private readonly IPriceMonitoringService _innerService;
	private readonly ICache _cache;
	private readonly ILogger<CachedPriceMonitoringService> _logger;
	private readonly TimeSpan _cacheDuration = DefaultCacheDuration;

	/// <summary>
	/// Initializes a new instance of the <see cref="CachedPriceMonitoringService"/> class.
	/// </summary>
	/// <param name="innerService">The price monitoring service to decorate.</param>
	/// <param name="cache">The cache used to store price data.</param>
	/// <param name="logger">The logger used to record cache activity.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="innerService"/>, <paramref name="cache"/>, or <paramref name="logger"/> is <see langword="null"/>.
	/// </exception>
	public CachedPriceMonitoringService(
		IPriceMonitoringService innerService,
		ICache cache,
		ILogger<CachedPriceMonitoringService> logger)
	{
		_innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
		_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <inheritdoc/>
	public async Task<Price?> GetCurrentPriceAsync(string asset, string fiat)
	{
		ArgumentNullException.ThrowIfNull(asset);
		ArgumentNullException.ThrowIfNull(fiat);

		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException(AssetNullOrWhitespaceMessage, nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException(FiatNullOrWhitespaceMessage, nameof(fiat));

		try
		{
			var cacheKey = string.Format(PriceCacheKeyFormat, asset, fiat);
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => await _innerService.GetCurrentPriceAsync(asset, fiat).ConfigureAwait(false),
				_cacheDuration);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToGetCachedPriceMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync()
	{
		try
		{
			var cacheKey = AllPricesCacheKey;
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => (await _innerService.GetAllCurrentPricesAsync().ConfigureAwait(false)).ToList() as IEnumerable<Price>,
				_cacheDuration) ?? Enumerable.Empty<Price>();
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToGetAllCachedPricesMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task<bool> UpdatePriceAsync(Price price)
	{
		if (price is null)
			throw new ArgumentNullException(nameof(price));
		if (string.IsNullOrWhiteSpace(price.Asset) || string.IsNullOrWhiteSpace(price.Fiat))
			throw new ArgumentException(PriceAssetAndFiatMustBeSpecifiedMessage);

		try
		{
			var result = await _innerService.UpdatePriceAsync(price).ConfigureAwait(false);
			if (result)
			{
				await _cache.RemoveAsync(string.Format(PriceCacheKeyFormat, price.Asset, price.Fiat)).ConfigureAwait(false);
				await _cache.RemoveAsync(AllPricesCacheKey).ConfigureAwait(false);
				_logger.LogDebug(CacheInvalidatedMessage, price.Asset, price.Fiat);
			}
			return result;
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToUpdateCachedPriceMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours)
	{
		ArgumentNullException.ThrowIfNull(asset);
		ArgumentNullException.ThrowIfNull(fiat);

		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException(AssetNullOrWhitespaceMessage, nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException(FiatNullOrWhitespaceMessage, nameof(fiat));
		if (hours <= 0)
			throw new ArgumentException(HoursMustBePositiveMessage, nameof(hours));

		try
		{
			var cacheKey = string.Format(AveragePriceCacheKeyFormat, asset, fiat, hours);
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => await _innerService.GetAveragePriceAsync(asset, fiat, hours),
				AveragePriceCacheDuration);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToGetCachedAveragePriceMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold)
	{
		if (changePercentThreshold < 0)
			throw new ArgumentException(ThresholdCannotBeNegativeMessage, nameof(changePercentThreshold));

		try
		{
			return await _innerService.GetPricesWithSignificantChangeAsync(changePercentThreshold).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToGetPricesWithSignificantChangeMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat)
	{
		ArgumentNullException.ThrowIfNull(asset);
		ArgumentNullException.ThrowIfNull(fiat);

		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException(AssetNullOrWhitespaceMessage, nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException(FiatNullOrWhitespaceMessage, nameof(fiat));

		try
		{
			var cacheKey = string.Format(SpreadCacheKeyFormat, asset, fiat);
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => await _innerService.GetSpreadAnalysisAsync(asset, fiat),
				_cacheDuration);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToGetCachedSpreadAnalysisMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task StartMonitoringAsync(CancellationToken cancellationToken)
	{
		try
		{
			await _innerService.StartMonitoringAsync(cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new ApiException(FailedToStartMonitoringMessage, ex);
		}
	}

	/// <inheritdoc/>
	public async Task StopMonitoringAsync()
	{
		try
		{
			await _cache.ClearAsync().ConfigureAwait(false);
			await _innerService.StopMonitoringAsync().ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException(FailedToStopMonitoringMessage, ex);
		}
	}
}

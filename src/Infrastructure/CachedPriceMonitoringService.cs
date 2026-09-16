#nullable enable
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Exceptions;

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
			throw new ArgumentException("Asset cannot be null or whitespace", nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException("Fiat cannot be null or whitespace", nameof(fiat));

		try
		{
			var cacheKey = $"price_{asset}_{fiat}";
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => await _innerService.GetCurrentPriceAsync(asset, fiat).ConfigureAwait(false),
				_cacheDuration);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to get cached price", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync()
	{
		try
		{
			var cacheKey = "all_prices";
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => (await _innerService.GetAllCurrentPricesAsync().ConfigureAwait(false)).ToList() as IEnumerable<Price>,
				_cacheDuration) ?? Enumerable.Empty<Price>();
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to get all cached prices", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<bool> UpdatePriceAsync(Price price)
	{
		if (price is null)
			throw new ArgumentNullException(nameof(price));
		if (string.IsNullOrWhiteSpace(price.Asset) || string.IsNullOrWhiteSpace(price.Fiat))
			throw new ArgumentException("Price asset and fiat must be specified");

		try
		{
			var result = await _innerService.UpdatePriceAsync(price).ConfigureAwait(false);
			if (result)
			{
				await _cache.RemoveAsync($"price_{price.Asset}_{price.Fiat}").ConfigureAwait(false);
				await _cache.RemoveAsync("all_prices").ConfigureAwait(false);
				_logger.LogDebug("Cache invalidated for {Asset}/{Fiat}", price.Asset, price.Fiat);
			}
			return result;
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to update cached price", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours)
	{
		ArgumentNullException.ThrowIfNull(asset);
		ArgumentNullException.ThrowIfNull(fiat);

		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException("Asset cannot be null or whitespace", nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException("Fiat cannot be null or whitespace", nameof(fiat));
		if (hours <= 0)
			throw new ArgumentException("Hours must be positive", nameof(hours));

		try
		{
			var cacheKey = $"avg_price_{asset}_{fiat}_{hours}h";
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => await _innerService.GetAveragePriceAsync(asset, fiat, hours),
				TimeSpan.FromMinutes(5));
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to get cached average price", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold)
	{
		if (changePercentThreshold < 0)
			throw new ArgumentException("Threshold cannot be negative", nameof(changePercentThreshold));

		try
		{
			return await _innerService.GetPricesWithSignificantChangeAsync(changePercentThreshold).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to get prices with significant change", ex);
		}
	}

	/// <inheritdoc/>
	public async Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat)
	{
		ArgumentNullException.ThrowIfNull(asset);
		ArgumentNullException.ThrowIfNull(fiat);

		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException("Asset cannot be null or whitespace", nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException("Fiat cannot be null or whitespace", nameof(fiat));

		try
		{
			var cacheKey = $"spread_{asset}_{fiat}";
			return await _cache.GetOrCreateAsync(
				cacheKey,
				async token => await _innerService.GetSpreadAnalysisAsync(asset, fiat),
				_cacheDuration);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to get cached spread analysis", ex);
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
			throw new ApiException("Failed to start monitoring", ex);
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
			throw new DataAccessException("Failed to stop monitoring", ex);
		}
	}
}

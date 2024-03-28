#nullable enable
using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Repositories;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service for monitoring P2P prices in real-time
/// </summary>
public class PriceMonitoringService : IPriceMonitoringService
{
	private readonly IPriceRepository _priceRepository;
	private readonly IPriceHistoryService _historyService;
	private readonly IAlertService _alertService;
	private readonly ISpreadAnalysisService _spreadAnalysisService;
	private readonly IEventBus _eventBus;
	private readonly IWebSocketService _webSocketService;
	private readonly AppSettings _settings;
	private readonly ILogger<PriceMonitoringService> _logger;
	private bool _isMonitoring;

	public PriceMonitoringService(
		IPriceRepository priceRepository,
		IPriceHistoryService historyService,
		IAlertService alertService,
		ISpreadAnalysisService spreadAnalysisService,
		IEventBus eventBus,
		IWebSocketService webSocketService,
		AppSettings settings,
		ILogger<PriceMonitoringService> logger)
	{
		_priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
		_historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
		_alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
		_spreadAnalysisService = spreadAnalysisService ?? throw new ArgumentNullException(nameof(spreadAnalysisService));
		_eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
		_webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Gets current price for a trading pair
	/// </summary>
	public async Task<Price?> GetCurrentPriceAsync(string asset, string fiat)
	{
		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException("Asset cannot be null or whitespace", nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException("Fiat cannot be null or whitespace", nameof(fiat));

		try
		{
			return await _priceRepository.GetLatestByAssetAndFiatAsync(asset, fiat).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to retrieve current price", ex);
		}
	}

	/// <summary>
	/// Gets all current prices
	/// </summary>
	public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync()
	{
		try
		{
			return await _priceRepository.GetAllActiveAsync().ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to retrieve all prices", ex);
		}
	}

	/// <summary>
	/// Updates a price and checks for alerts
	/// </summary>
	public async Task<bool> UpdatePriceAsync(Price price)
	{
		if (price is null)
			throw new ArgumentNullException(nameof(price));
		if (string.IsNullOrWhiteSpace(price.Asset) || string.IsNullOrWhiteSpace(price.Fiat))
			throw new ArgumentException("Price must have valid asset and fiat");
		if (price.BuyPrice <= 0 || price.SellPrice <= 0)
			throw new InvalidPriceException("Price values must be positive");

		try
		{
var added = await _priceRepository.AddAsync(price).ConfigureAwait(false);
			if (added > 0)
			{
				// Record history and check alerts
				await _historyService.RecordPriceAsync(price).ConfigureAwait(false);
				var triggeredAlerts = await _alertService.CheckTriggersAsync(price).ConfigureAwait(false);

				// Publish PriceUpdatedEvent
				var priceUpdatedEvent = new PriceUpdatedEvent
				{
					Asset = price.Asset,
					Fiat = price.Fiat,
					BuyPrice = price.BuyPrice,
					SellPrice = price.SellPrice,
				};
				await _eventBus.PublishAsync(priceUpdatedEvent).ConfigureAwait(false);

				_logger.LogInformation("Updated price {Asset}/{Fiat}: Buy={Buy:F8}, Sell={Sell:F8}",
					price.Asset, price.Fiat, price.BuyPrice, price.SellPrice);

				return true;
			}

			return false;
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to update price", ex);
		}
	}

	/// <summary>
	/// Gets average price over specified hours
	/// </summary>
	public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours)
	{
		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException("Asset cannot be null or whitespace", nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException("Fiat cannot be null or whitespace", nameof(fiat));
		if (hours <= 0)
			throw new ArgumentException("Hours must be positive", nameof(hours));

		try
		{
			return await _priceRepository.GetAveragePriceAsync(asset, fiat, hours).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to calculate average price", ex);
		}
	}

	/// <summary>
	/// Gets prices with significant change
	/// </summary>
	public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold)
	{
		if (changePercentThreshold < 0)
			throw new ArgumentException("Threshold cannot be negative", nameof(changePercentThreshold));

		try
		{
			var prices = await GetAllCurrentPricesAsync().ConfigureAwait(false);

			return prices.Where(p =>
				Math.Abs(p.BuyChangePercent) > changePercentThreshold ||
				Math.Abs(p.SellChangePercent) > changePercentThreshold);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to retrieve prices with significant change", ex);
		}
	}

	/// <summary>
	/// Analyzes spread for a trading pair
	/// </summary>
	public async Task<Spread?> GetSpreadAnalysisAsync(string asset, string fiat)
	{
		if (string.IsNullOrWhiteSpace(asset))
			throw new ArgumentException("Asset cannot be null or whitespace", nameof(asset));
		if (string.IsNullOrWhiteSpace(fiat))
			throw new ArgumentException("Fiat cannot be null or whitespace", nameof(fiat));

		try
		{
			return await _spreadAnalysisService.GetSpreadAnalysisAsync(asset, fiat).ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to analyze spread", ex);
		}
	}

	/// <summary>
	/// Starts the monitoring service
	/// </summary>
	public async Task StartMonitoringAsync(CancellationToken cancellationToken)
	{
		if (_isMonitoring)
			return;

		if (!_settings.EnableWebSocket)
		{
			_logger.LogWarning("WebSocket monitoring is disabled in settings. Monitoring will not start.");
			return;
		}

		_isMonitoring = true;
		_logger.LogInformation("Price monitoring service starting via WebSocket");

		try
		{
			_webSocketService.OnPriceUpdate += _webSocketService_OnPriceUpdate;
			await _webSocketService.ConnectAsync().ConfigureAwait(false);

			foreach (var asset in _settings.MonitoredAssets)
			{
				foreach (var fiat in _settings.MonitoredFiats)
				{
					await _webSocketService.SubscribeToPairAsync(asset, fiat).ConfigureAwait(false);
				}
			}
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			_isMonitoring = false;
			_logger.LogError(ex, "Error starting price monitoring service");
			throw new ApiException("Failed to start price monitoring", ex);
		}
	}

	/// <summary>
	/// Stops the monitoring service
	/// </summary>
	public async Task StopMonitoringAsync()
	{
		if (!_isMonitoring)
			return;

		_isMonitoring = false;
		_logger.LogInformation("Price monitoring service stopping");

		try
		{
			_webSocketService.OnPriceUpdate -= _webSocketService_OnPriceUpdate;
			await _webSocketService.DisconnectAsync().ConfigureAwait(false);
		}
		catch (Exception ex) when (ex is not BinanceP2pException)
		{
			throw new DataAccessException("Failed to stop monitoring service", ex);
		}
	}

	private async void _webSocketService_OnPriceUpdate(object? sender, PriceUpdateEventArgs e)
	{
		try
		{
			var previousPrice = await _priceRepository.GetLatestByAssetAndFiatAsync(e.Asset, e.Fiat).ConfigureAwait(false);

			var price = new Price
			{
				Asset = e.Asset,
				Fiat = e.Fiat,
				BuyPrice = e.BuyPrice,
				SellPrice = e.SellPrice,
				Timestamp = e.UpdateTime,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				BuyChangePercent = previousPrice != null && previousPrice.BuyPrice > 0 ? ((e.BuyPrice - previousPrice.BuyPrice) / previousPrice.BuyPrice) * 100 : 0,
				SellChangePercent = previousPrice != null && previousPrice.SellPrice > 0 ? ((e.SellPrice - previousPrice.SellPrice) / previousPrice.SellPrice) * 100 : 0,
			};

			await UpdatePriceAsync(price).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing WebSocket price update for {Asset}/{Fiat}", e.Asset, e.Fiat);
		}
	}
}

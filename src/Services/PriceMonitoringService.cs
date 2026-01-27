#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    private readonly AppSettings _settings;
    private readonly ILogger<PriceMonitoringService> _logger;
    private bool _isMonitoring;

    public PriceMonitoringService(
        IPriceRepository priceRepository,
        IPriceHistoryService historyService,
        IAlertService alertService,
        AppSettings settings,
        ILogger<PriceMonitoringService> logger)
    {
        _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current price for a trading pair
    /// </summary>
    public async Task<Price?> GetCurrentPriceAsync(string asset, string fiat)
    {
        try
        {
            return await _priceRepository.GetLatestByAssetAndFiatAsync(asset, fiat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current price for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets all current prices
    /// </summary>
    public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync()
    {
        try
        {
            return await _priceRepository.GetAllActiveAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all prices");
            throw;
        }
    }

    /// <summary>
    /// Updates a price and checks for alerts
    /// </summary>
    public async Task<bool> UpdatePriceAsync(Price price)
    {
        try
        {
            if (price is null || !price.IsValid())
                throw new InvalidPriceException("Invalid price data");

            var added = await _priceRepository.AddAsync(price);
            if (added > 0)
            {
                // Record history and check alerts
                await _historyService.RecordPriceAsync(price);
                var triggeredAlerts = await _alertService.CheckTriggersAsync(price);

                _logger.LogInformation("Updated price {Asset}/{Fiat}: Buy={Buy:F8}, Sell={Sell:F8}",
                    price.Asset, price.Fiat, price.BuyPrice, price.SellPrice);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price for {Asset}/{Fiat}", price?.Asset, price?.Fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets average price over specified hours
    /// </summary>
    public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours)
    {
        try
        {
            return await _priceRepository.GetAveragePriceAsync(asset, fiat, hours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average price for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Gets prices with significant change
    /// </summary>
    public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync(decimal changePercentThreshold)
    {
        try
        {
            var prices = await GetAllCurrentPricesAsync();

            return prices.Where(p =>
                Math.Abs(p.BuyChangePercent) > changePercentThreshold ||
                Math.Abs(p.SellChangePercent) > changePercentThreshold);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices with significant change");
            throw;
        }
    }

    /// <summary>
    /// Analyzes spread for a trading pair
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetSpreadAnalysisAsync(string asset, string fiat)
    {
        // Fix: Validate input parameters
        if (string.IsNullOrWhiteSpace(asset))
            throw new ArgumentException($"Parameter '{nameof(asset)}' cannot be null or empty", nameof(asset));
            
        if (string.IsNullOrWhiteSpace(fiat))
            throw new ArgumentException($"Parameter '{nameof(fiat)}' cannot be null or empty", nameof(fiat));

        try
        {
            var price = await GetCurrentPriceAsync(asset, fiat);
            if (price is null)
                return new Dictionary<string, decimal>();

            var spread = price.CalculateSpread();
            var avgPrice = await GetAveragePriceAsync(asset, fiat, 24);

            return new Dictionary<string, decimal>
            {
                { "CurrentSpread", spread },
                { "BuyPrice", price.BuyPrice },
                { "SellPrice", price.SellPrice },
                { "SpreadPercentage", spread }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spread for {Asset}/{Fiat}", asset, fiat);
            throw;
        }
    }

    /// <summary>
    /// Starts the monitoring service
    /// </summary>
    public async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        if (_isMonitoring)
            return;

        _isMonitoring = true;
        _logger.LogInformation("Price monitoring service started");

        try
        {
            while (!cancellationToken.IsCancellationRequested && _isMonitoring)
            {
                await Task.Delay(_settings.MonitoringIntervalSeconds * 1000, cancellationToken);
                // Monitoring logic will be called by background service
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Price monitoring cancelled");
        }
        finally
        {
            _isMonitoring = false;
        }
    }

    /// <summary>
    /// Stops the monitoring service
    /// </summary>
    public async Task StopMonitoringAsync()
    {
        _isMonitoring = false;
        _logger.LogInformation("Price monitoring service stopped");
        await Task.CompletedTask;
    }
}

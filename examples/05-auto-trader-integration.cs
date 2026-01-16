#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using BinanceP2pMonitor.Events;

namespace BinanceP2pMonitor.Examples;

/// <summary>
/// Integration example for automated trading systems.
/// Demonstrates how to subscribe to price events and execute trades.
/// This is a framework - actual trade execution would use exchange API.
/// </summary>
class AutoTraderIntegrationExample
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args).Build();

        try
        {
            var priceService = host.Services.GetRequiredService<IPriceMonitoringService>();
            var alertService = host.Services.GetRequiredService<IAlertService>();
            var eventBus = host.Services.GetRequiredService<IEventBus>();
            var trader = new SimpleAutoTrader();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║       Auto-Trader Integration Example              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            // Set up trading rules
            await SetUpTradingRulesAsync(alertService).ConfigureAwait(false);

            // Subscribe to price updates for trading decisions
            eventBus.Subscribe<PriceUpdatedEvent>(async @event =>
            {
                await trader.OnPriceUpdateAsync(@event.Price).ConfigureAwait(false);
            });

            // Subscribe to alerts for automatic trade execution
            eventBus.Subscribe<AlertTriggeredEvent>(async @event =>
            {
                await trader.OnAlertTriggeredAsync(@event).ConfigureAwait(false);
            });

            // Start monitoring
            var subscription = await priceService.MonitorPriceAsync(
                "BTC", "USD",
                price => { /* Handled by event bus */ },
                interval: TimeSpan.FromSeconds(30));

            Console.WriteLine("\n[✓] Auto-trader listening to market events...");
            Console.WriteLine("Press Ctrl+C to stop.\n");

            while (true)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            host.Dispose();
        }
    }

    static async Task SetUpTradingRulesAsync(IAlertService alertService)
    {
        // Buy signal: BTC drops 3%
        await alertService.CreateAlertAsync(new PriceAlert
        {
            Asset = "BTC",
            Fiat = "USD",
            AlertType = Constants.AlertType.PriceChange,
            LowerThreshold = -3.0m,
            UserId = "autotrader",
            CooldownMinutes = 30,
            IsActive = true
        });

        Console.WriteLine("[✓] Trading rules configured:");
        Console.WriteLine("    - Buy signal: BTC/USD drops 3%");
        Console.WriteLine("    - Cooldown: 30 minutes between trades");
    }
}

/// <summary>
/// Simple automated trading strategy implementation.
/// In production, this would connect to exchange APIs.
/// </summary>
class SimpleAutoTrader
{
    private decimal _lastBuyPrice = 0;
    private int _positionSize = 0;
    private const decimal TargetProfit = 1.5m;  // 1.5% profit target

    public async Task OnPriceUpdateAsync(Price price)
    {
        if (_positionSize > 0)
        {
            // Check if we should sell
            var profitPercent = ((price.Bid - _lastBuyPrice) / _lastBuyPrice) * 100;

            if (profitPercent >= TargetProfit)
            {
                await ExecutorSellAsync(price).ConfigureAwait(false);
            }
        }

        // Log for monitoring
        if (price.Timestamp.Second % 10 == 0)
        {
            var status = _positionSize > 0
                ? $"Holding {_positionSize} BTC @ {_lastBuyPrice:F2}"
                : "No position";
            Console.WriteLine($"[{price.Timestamp:HH:mm:ss}] {status} | " +
                $"Current: {price.Bid:F2}");
        }
    }

    public async Task OnAlertTriggeredAsync(dynamic alert)
    {
        if (alert.Asset == "BTC" && alert.Fiat == "USD")
        {
            Console.WriteLine($"[TRADE SIGNAL] BTC alert triggered!");
            // Execute buy in production
            await ExecuteBuyAsync().ConfigureAwait(false);
        }
    }

    private async Task ExecuteBuyAsync()
    {
        // In production: Call exchange API to place buy order
        Console.WriteLine("[BUY ORDER] Executing buy signal...");
        _positionSize = 1;  // Simplified: always buy 1 BTC
        // _lastBuyPrice would be set from actual order fill
        await Task.Delay(100).ConfigureAwait(false);
    }

    private async Task ExecutorSellAsync(Price price)
    {
        // In production: Call exchange API to place sell order
        Console.WriteLine($"[SELL ORDER] Executing profit target at {price.Bid:F2}");
        var profit = ((price.Bid - _lastBuyPrice) / _lastBuyPrice) * 100;
        Console.WriteLine($"[PROFIT] Realized gain: {profit:F2}%");
        _positionSize = 0;
        await Task.Delay(100).ConfigureAwait(false);
    }
}

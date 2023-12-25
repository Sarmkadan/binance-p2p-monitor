#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Examples;

/// <summary>
/// Real-time price monitoring with console output.
/// Demonstrates basic WebSocket price feed and display formatting.
/// Usage: dotnet run --project examples/01-real-time-monitor.cs
/// </summary>
class RealTimeMonitorExample
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args).Build();

        try
        {
            var priceService = host.Services.GetRequiredService<IPriceMonitoringService>();
            var formatter = new ConsoleFormatter();

            var assets = new[] { "BTC", "ETH", "BNB" };
            var fiats = new[] { "USD", "EUR" };

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Binance P2P Real-Time Price Monitor            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝");
            Console.WriteLine();

            var subscriptions = new List<string>();

            // Create one subscription per asset
            foreach (var asset in assets)
            {
                foreach (var fiat in fiats)
                {
                    var subscription = await priceService.MonitorPriceAsync(
                        asset, fiat,
                        price => formatter.DisplayPrice(price),
                        interval: TimeSpan.FromSeconds(30));

                    subscriptions.Add(subscription);
                }
            }

            Console.WriteLine("\n[✓] Monitoring started. Press Ctrl+C to stop.\n");

            // Keep monitoring until Ctrl+C
            while (true)
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
        finally
        {
            host.Dispose();
        }
    }
}

/// <summary>
/// Formats price data for console display.
/// </summary>
class ConsoleFormatter
{
    public void DisplayPrice(Price price)
    {
        var timestamp = price.Timestamp.ToString("HH:mm:ss");
        var spread = ((price.Ask - price.Bid) / price.Ask) * 100;
        var arrow = price.Bid > 0 ? "→" : "○";

        Console.WriteLine(
            $"[{timestamp}] {arrow} {price.Asset}/{price.Fiat,-3} " +
            $"Bid: {price.Bid:F2,-12} " +
            $"Ask: {price.Ask:F2,-12} " +
            $"Spread: {spread:F2}%");
    }
}

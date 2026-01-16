#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Examples;

/// <summary>
/// Identify and analyze buy/sell spread anomalies.
/// Useful for detecting market conditions and trading opportunities.
/// Higher spreads indicate less liquidity or higher volatility.
/// </summary>
class SpreadAnalyzerExample
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args).Build();

        try
        {
            var spreadService = host.Services.GetRequiredService<ISpreadAnalysisService>();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║        Binance P2P Spread Analyzer                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            var assets = new[] { "BTC", "ETH", "BNB", "XRP" };
            var fiat = "USD";

            // Analyze each asset
            var spreads = await spreadService.AnalyzeAssetsAsync(assets, fiat).ConfigureAwait(false);

            DisplayAnalysis(spreads.ToList());

            // Get historical average spread
            Console.WriteLine("\n[Calculating 24-hour average spreads...]\n");

            foreach (var asset in assets)
            {
                var avgSpread = await spreadService.CalculateAverageSpreadAsync(asset, fiat, hours: 24).ConfigureAwait(false);
                Console.WriteLine(
                    $"{asset}/{fiat,-3}: " +
                    $"24h Avg Spread = {avgSpread:F2}%");
            }

            Console.WriteLine("\n[✓] Analysis complete.");
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

    static void DisplayAnalysis(List<Spread> spreads)
    {
        Console.WriteLine($"{"Asset",-8} {"Fiat",-6} {"Buy Price",-15} {"Sell Price",-15} " +
            $"{"Spread %",-12} {"Avg Price",-15}");
        Console.WriteLine(new string('─', 85));

        // Sort by spread percentage (descending)
        var sorted = spreads.OrderByDescending(s => s.SpreadPercentage).ToList();

        foreach (var spread in sorted)
        {
            var spreadColor = GetSpreadColor(spread.SpreadPercentage);
            Console.WriteLine(
                $"{spread.Asset,-8} " +
                $"{spread.Fiat,-6} " +
                $"{spread.BuyPrice:F2,-15} " +
                $"{spread.SellPrice:F2,-15} " +
                $"{spreadColor}{spread.SpreadPercentage:F2}%{ResetColor(),-12} " +
                $"{spread.AveragePrice:F2,-15}");
        }

        // Statistics
        Console.WriteLine(new string('─', 85));
        Console.WriteLine($"\nStatistics:");
        Console.WriteLine($"  Max Spread:  {spreads.Max(s => s.SpreadPercentage):F2}%");
        Console.WriteLine($"  Min Spread:  {spreads.Min(s => s.SpreadPercentage):F2}%");
        Console.WriteLine($"  Avg Spread:  {spreads.Average(s => s.SpreadPercentage):F2}%");

        // Identify anomalies
        var avgSpread = spreads.Average(s => s.SpreadPercentage);
        var anomalies = spreads.Where(s => s.SpreadPercentage > avgSpread * 1.5).ToList();

        if (anomalies.Any())
        {
            Console.WriteLine($"\n[!] Anomalies detected ({anomalies.Count}):");
            foreach (var spread in anomalies)
            {
                var ratio = spread.SpreadPercentage / avgSpread;
                Console.WriteLine(
                    $"    {spread.Asset}/{spread.Fiat}: {spread.SpreadPercentage:F2}% " +
                    $"({ratio:F1}x average)");
            }
        }
    }

    static string GetSpreadColor(decimal spread)
    {
        return spread switch
        {
            < 1.0m => "[32m",     // Green (low)
            < 2.0m => "[33m",     // Yellow (medium)
            < 3.0m => "[91m",     // Light red (high)
            _ => "[31m"           // Red (very high)
        };
    }

    static string ResetColor() => "[0m";
}

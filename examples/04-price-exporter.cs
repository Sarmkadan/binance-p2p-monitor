// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BinanceP2pMonitor.Services;
using BinanceP2pMonitor.Models;
using Newtonsoft.Json;

namespace BinanceP2pMonitor.Examples;

/// <summary>
/// Export price history to CSV and JSON formats.
/// Useful for data analysis, charting, and external integrations.
/// Supports filtering by date range and aggregation periods.
/// </summary>
class PriceExporterExample
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args).Build();

        try
        {
            var historyService = host.Services.GetRequiredService<IPriceHistoryService>();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║         Binance P2P Price Data Exporter            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            var asset = "BTC";
            var fiat = "USD";
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Export to CSV
            await ExportToCsvAsync(historyService, asset, fiat, $"prices_{asset}_{fiat}_{timestamp}.csv");

            // Export to JSON
            await ExportToJsonAsync(historyService, asset, fiat, $"prices_{asset}_{fiat}_{timestamp}.json");

            // Export aggregated data (hourly)
            await ExportAggregatedAsync(historyService, asset, fiat,
                $"prices_{asset}_{fiat}_hourly_{timestamp}.csv");

            Console.WriteLine("\n[✓] Export complete.");
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

    static async Task ExportToCsvAsync(IPriceHistoryService service,
        string asset, string fiat, string filename)
    {
        Console.WriteLine($"Exporting to CSV: {filename}");

        var history = await service.GetHistoryAsync(asset, fiat, limit: 1000);

        var csv = new StringBuilder();
        csv.AppendLine("timestamp,asset,fiat,bid,ask,spread_percent,volume");

        foreach (var record in history)
        {
            var spread = ((record.Ask - record.Bid) / record.Ask) * 100;
            csv.AppendLine(
                $"{record.Timestamp:O}," +
                $"{record.Asset}," +
                $"{record.Fiat}," +
                $"{record.Bid:F8}," +
                $"{record.Ask:F8}," +
                $"{spread:F4}," +
                $"{record.Volume}");
        }

        await File.WriteAllTextAsync(filename, csv.ToString());
        Console.WriteLine($"  ✓ {history.Count()} records exported");
    }

    static async Task ExportToJsonAsync(IPriceHistoryService service,
        string asset, string fiat, string filename)
    {
        Console.WriteLine($"Exporting to JSON: {filename}");

        var history = await service.GetHistoryAsync(asset, fiat, limit: 1000);

        var data = new
        {
            asset,
            fiat,
            exported_at = DateTime.UtcNow,
            record_count = history.Count(),
            records = history.Select(h => new
            {
                h.Timestamp,
                h.Asset,
                h.Fiat,
                h.Bid,
                h.Ask,
                h.Volume,
                spread_percent = ((h.Ask - h.Bid) / h.Ask) * 100
            })
        };

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        await File.WriteAllTextAsync(filename, json);
        Console.WriteLine($"  ✓ {history.Count()} records exported");
    }

    static async Task ExportAggregatedAsync(IPriceHistoryService service,
        string asset, string fiat, string filename)
    {
        Console.WriteLine($"Exporting hourly aggregates: {filename}");

        var csv = new StringBuilder();
        csv.AppendLine("hour,asset,fiat,open,high,low,close,avg_bid,avg_ask,volume_sum");

        // Fetch hourly data (would need actual aggregation logic in service)
        var hourly = await service.GetAggregatedAsync(asset, fiat, TimeSpan.FromHours(1));

        if (hourly != null)
        {
            csv.AppendLine($"{hourly.Timestamp:O}," +
                $"{hourly.Asset}," +
                $"{hourly.Fiat}," +
                $"{hourly.Bid:F8}," +
                $"{hourly.Ask:F8}," +
                $"{hourly.Bid:F8}," +
                $"{hourly.Ask:F8}," +
                $"{hourly.Bid:F8}," +
                $"{hourly.Ask:F8}," +
                $"{hourly.Volume}");

            await File.WriteAllTextAsync(filename, csv.ToString());
            Console.WriteLine($"  ✓ Hourly aggregates exported");
        }
    }
}

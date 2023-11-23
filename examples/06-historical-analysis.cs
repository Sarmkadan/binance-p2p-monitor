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
/// Analyze historical price trends and statistics.
/// Useful for backtesting strategies and understanding market behavior.
/// Demonstrates time-series analysis capabilities.
/// </summary>
class HistoricalAnalysisExample
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args).Build();

        try
        {
            var historyService = host.Services.GetRequiredService<IPriceHistoryService>();

            Console.WriteLine("╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║     Historical Price Analysis Tool                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            var asset = "BTC";
            var fiat = "USD";

            // Fetch historical data
            var history = await historyService.GetHistoryAsync(asset, fiat, limit: 1000);

            if (!history.Any())
            {
                Console.WriteLine($"[!] No historical data for {asset}/{fiat}");
                return;
            }

            var analyzer = new PriceAnalyzer(history.ToList());

            Console.WriteLine($"Analysis for: {asset}/{fiat}");
            Console.WriteLine($"Data points: {history.Count()}");
            Console.WriteLine($"Time span: {analyzer.TimeSpan.TotalHours:F1} hours\n");

            // Display statistics
            DisplayStatistics(analyzer);

            // Display trends
            DisplayTrends(analyzer);

            // Display volatility
            DisplayVolatility(analyzer);

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

    static void DisplayStatistics(PriceAnalyzer analyzer)
    {
        Console.WriteLine("╔ Price Statistics ═══════════════════════════════════╗");
        Console.WriteLine($"║ Current Price:        {analyzer.CurrentPrice:F2,-40} ║");
        Console.WriteLine($"║ Highest Price:        {analyzer.MaxPrice:F2,-40} ║");
        Console.WriteLine($"║ Lowest Price:         {analyzer.MinPrice:F2,-40} ║");
        Console.WriteLine($"║ Average Price:        {analyzer.AvgPrice:F2,-40} ║");
        Console.WriteLine($"║ Median Price:         {analyzer.MedianPrice:F2,-40} ║");
        Console.WriteLine($"║ Price Range:          {analyzer.PriceRange:F2,-40} ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");
    }

    static void DisplayTrends(PriceAnalyzer analyzer)
    {
        var trend = analyzer.GetTrend();
        var trendPercent = analyzer.GetTrendPercent();

        var arrow = trendPercent > 0 ? "📈" : "📉";
        var color = trendPercent > 0 ? "[32m" : "[31m";

        Console.WriteLine("╔ Trend Analysis ═════════════════════════════════════╗");
        Console.WriteLine($"║ {arrow} Overall Trend:        {color}{trendPercent:+0.00;-0.00}%[0m");
        Console.WriteLine($"║ High 1h:               {analyzer.GetHighLast(hours: 1):F2}");
        Console.WriteLine($"║ Low 1h:                {analyzer.GetLowLast(hours: 1):F2}");
        Console.WriteLine($"║ Change (1h):           {analyzer.GetChangeLast(hours: 1):+0.00;-0.00}%");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");
    }

    static void DisplayVolatility(PriceAnalyzer analyzer)
    {
        var volatility = analyzer.GetVolatility();
        var volatilityLevel = volatility switch
        {
            < 0.5m => "Very Low",
            < 1.0m => "Low",
            < 2.0m => "Medium",
            < 3.0m => "High",
            _ => "Very High"
        };

        Console.WriteLine("╔ Volatility Analysis ════════════════════════════════╗");
        Console.WriteLine($"║ Volatility (σ):        {volatility:F4}");
        Console.WriteLine($"║ Volatility Level:      {volatilityLevel,-36} ║");
        Console.WriteLine($"║ Max Daily Change:      {analyzer.GetMaxDailyChange():F2}%");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝\n");
    }
}

/// <summary>
/// Statistical analysis of price history.
/// </summary>
class PriceAnalyzer
{
    private readonly List<PriceHistory> _history;

    public PriceAnalyzer(List<PriceHistory> history)
    {
        _history = history.OrderBy(h => h.Timestamp).ToList();
    }

    public decimal CurrentPrice => _history.Last().Bid;
    public decimal MaxPrice => _history.Max(h => h.Ask);
    public decimal MinPrice => _history.Min(h => h.Bid);
    public decimal AvgPrice => _history.Average(h => (h.Bid + h.Ask) / 2);
    public decimal MedianPrice
    {
        get
        {
            var sorted = _history.Select(h => (h.Bid + h.Ask) / 2).OrderBy(p => p).ToList();
            var count = sorted.Count;
            return count % 2 == 0
                ? (sorted[count / 2 - 1] + sorted[count / 2]) / 2
                : sorted[count / 2];
        }
    }
    public decimal PriceRange => MaxPrice - MinPrice;
    public TimeSpan TimeSpan => _history.Last().Timestamp - _history.First().Timestamp;

    public string GetTrend()
    {
        var firstHalf = _history.Take(_history.Count / 2).Average(h => h.Bid);
        var secondHalf = _history.Skip(_history.Count / 2).Average(h => h.Bid);

        return secondHalf > firstHalf ? "Uptrend" : "Downtrend";
    }

    public decimal GetTrendPercent()
    {
        var first = _history.First().Bid;
        var last = _history.Last().Bid;
        return ((last - first) / first) * 100;
    }

    public decimal GetVolatility()
    {
        var prices = _history.Select(h => (h.Bid + h.Ask) / 2).ToList();
        var avg = prices.Average();
        var variance = prices.Average(p => (p - avg) * (p - avg));
        return (decimal)Math.Sqrt((double)variance);
    }

    public decimal GetHighLast(int hours)
    {
        var cutoff = DateTime.UtcNow.AddHours(-hours);
        return _history.Where(h => h.Timestamp >= cutoff).Max(h => h.Ask);
    }

    public decimal GetLowLast(int hours)
    {
        var cutoff = DateTime.UtcNow.AddHours(-hours);
        return _history.Where(h => h.Timestamp >= cutoff).Min(h => h.Bid);
    }

    public decimal GetChangeLast(int hours)
    {
        var cutoff = DateTime.UtcNow.AddHours(-hours);
        var recent = _history.Where(h => h.Timestamp >= cutoff).ToList();

        if (recent.Count < 2) return 0;

        var first = recent.First().Bid;
        var last = recent.Last().Bid;
        return ((last - first) / first) * 100;
    }

    public decimal GetMaxDailyChange()
    {
        var changes = new List<decimal>();

        for (int i = 1; i < _history.Count; i++)
        {
            var change = (((_history[i].Ask - _history[i - 1].Bid) / _history[i - 1].Bid) * 100);
            changes.Add(Math.Abs(change));
        }

        return changes.Any() ? changes.Max() : 0;
    }
}

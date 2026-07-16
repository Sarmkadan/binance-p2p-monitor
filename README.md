// entire file content ...

## Spread

The `Spread` type represents spread analysis data between buy and sell prices. It stores various metrics such as current spread percentage, average spread percentage, minimum and maximum spread percentages, sample count, and standard deviation. Here is an example of creating and using a `Spread` instance:

```csharp
using BinanceP2pMonitor.Models;

var spread = new Spread
{
    Asset = "USDT-BTC",
    Fiat = "USDT",
    CurrentSpreadPercent = 0.23m,
    AverageSpreadPercent = 0.15m,
    MinSpreadPercent = 0.05m,
    MaxSpreadPercent = 0.30m,
    SampleCount = 100,
    LastUpdatedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    StandardDeviation = 0.01m,
    PercentileRank = 0.5m
};

Console.WriteLine($"Current spread: {spread.CurrentSpreadPercent:F4}%");
Console.WriteLine($"Average spread: {spread.AverageSpreadPercent:F4}%");
Console.WriteLine($"Is high spread: {spread.IsHighSpread(1.5m)}");
Console.WriteLine($"Is low spread: {spread.IsLowSpread(0.3m)}");
Console.WriteLine($"Variance from average: {spread.GetVarianceFromAverage():F4}%");
Console.WriteLine($"Is normal: {spread.IsNormal()}");
Console.WriteLine($"Is valid: {spread.IsValid()}");
Console.WriteLine($"Risk level: {spread.GetRiskLevel()}");
```

## SpreadStatisticsReport

`SpreadStatisticsReport` is an immutable data object that summarizes statistical analysis of spread percentages over a configurable time window. It contains raw metrics (mean, median, variance, etc.) as well as helper methods to interpret the current spread in context.

```csharp
using System;
using BinanceP2pMonitor.Models;

var report = new SpreadStatisticsReport
{
    Asset = "BTC",
    Fiat = "USDT",
    TimeWindowHours = 24,
    SampleCount = 500,
    Mean = 0.12m,
    Median = 0.10m,
    StandardDeviation = 0.03m,
    Variance = 0.0009m,
    MinSpread = 0.05m,
    MaxSpread = 0.20m,
    Percentile5 = 0.06m,
    Percentile95 = 0.18m,
    CurrentSpread = 0.15m,
    ZScore = 1.0m,
    TrendSlope = 0.02m,
    AnalyzedAt = DateTime.UtcNow,
    IsAnomalous = false
};

Console.WriteLine($"Trend: {report.GetTrendLabel()}");
Console.WriteLine($"Critical? {report.IsCritical()}");
Console.WriteLine($"Above average? {report.IsAboveAverage()}");
```

## Market

The `Market` type represents a trading pair on Binance P2P with comprehensive market data and monitoring capabilities. It tracks buy/sell prices, trading volume, offer counts, activity status, and provides methods for spread calculation and price validation.

```csharp
using BinanceP2pMonitor.Models;

var market = new Market
{
    Id = 1,
    Asset = "USDT-BTC",
    Fiat = "USDT",
    IsActive = true,
    IsMonitored = true,
    Description = "Bitcoin to Tether trading pair",
    LastBuyPrice = 50000.50m,
    LastSellPrice = 50010.25m,
    TotalOffers = 42,
    DailyVolume = 1500000,
    CreatedAt = DateTime.UtcNow.AddDays(-30),
    UpdatedAt = DateTime.UtcNow,
    LastPriceUpdateAt = DateTime.UtcNow.AddMinutes(-5),
    MonitoringPriority = 1,
    MonitoringPriority = 1
};

Console.WriteLine($"Pair ID: {market.GetPairId()}");
Console.WriteLine($"Current spread: {market.CalculateSpread():F4}%");
Console.WriteLine($"Price stale? {market.IsPriceStale(TimeSpan.FromMinutes(10))}");
Console.WriteLine($"Valid market? {market.IsValid()}");
Console.WriteLine($"Activity level: {market.GetActivityLevel()}");

// Update prices when new data arrives
market.UpdatePrices(50050.75m, 50060.50m);
```

## BacktestOptions

// ... rest of content ...

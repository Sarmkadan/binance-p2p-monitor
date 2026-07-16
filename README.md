// entire file content ...
// ... goes in between

## BacktestOptions

The `BacktestOptions` class represents a set of configuration options for backtesting a trading strategy. It allows you to customize various parameters such as initial equity, position sizing, entry and exit thresholds, risk management, and more. Here is an example of creating and validating a `BacktestOptions` instance:

```csharp
using BinanceP2pMonitor.Backtesting;

var backtestOptions = new BacktestOptions
{
    InitialEquity = 10_000m,
    PositionSizeFraction = 0.05m,
    EntrySpreadThreshold = 1.5m,
    ExitSpreadThreshold = 0.5m,
    StopLossPercent = 3.0m,
    TakeProfitPercent = 6.0m,
    TransactionCostPercent = 0.3m,
    MaxConcurrentPositions = 5,
    LookbackPeriod = 100,
    MonteCarloIterations = 5_000,
    ConfidenceLevel = 0.99m,
    RandomSeed = 42,
    VolatilityScaleFactor = 1.2m
};

backtestOptions.Validate();
```

## ConsoleOutputWriter

// ... rest of content ...

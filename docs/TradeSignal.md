# TradeSignal

`TradeSignal` is an immutable record type that encapsulates the complete result of a trading strategy simulation or backtest. It aggregates core trade metrics, Monte Carlo simulation outcomes, and equity curve data into a single, self-contained object designed for downstream analysis, reporting, and comparison of strategy performance.

## API

### `TradeSignal`
A sealed record representing the full output of a trade signal evaluation, including backtest results, Monte Carlo projections, and equity curve history.

**Members:**

- `public required BacktestTrade Backtest`  
  The primary backtest trade result containing asset, fiat, period, and equity information for the deterministic simulation run.

- `public required int Iterations`  
  The number of Monte Carlo simulation iterations executed. Must be a positive integer.

- `public required decimal MedianFinalEquity`  
  The median final equity value across all Monte Carlo paths. Represents the central tendency of projected outcomes.

- `public required decimal MeanFinalEquity`  
  The arithmetic mean of final equity values across all Monte Carlo paths.

- `public required decimal LowerConfidenceBound`  
  The lower bound of the confidence interval for final equity (typically 95% confidence, 5th percentile).

- `public required decimal UpperConfidenceBound`  
  The upper bound of the confidence interval for final equity (typically 95% confidence, 95th percentile).

- `public required decimal ValueAtRisk`  
  The Value at Risk (VaR) estimate at the configured confidence level, expressed as a positive decimal representing potential loss.

- `public required decimal ConditionalValueAtRisk`  
  The Conditional Value at Risk (CVaR / Expected Shortfall), representing the average loss beyond the VaR threshold.

- `public required decimal ProbabilityOfProfit`  
  The proportion of Monte Carlo paths that resulted in a final equity greater than the initial equity, expressed as a decimal between 0 and 1.

- `public required decimal MedianMaxDrawdownPercent`  
  The median of the maximum drawdown percentages observed across all Monte Carlo paths.

- `public required IReadOnlyList<MonteCarloPathSummary> Paths`  
  A read-only collection of individual Monte Carlo path summaries. Each entry contains the final equity and maximum drawdown for a single simulated path.

- `public required IReadOnlyList<EquityCurvePoint> EquityCurve`  
  A read-only collection of equity curve points representing the equity value at each time step during the deterministic backtest.

### `BacktestTrade`
A sealed record describing the outcome of a single deterministic backtest run.

**Members:**

- `public required string Asset`  
  The traded asset symbol (e.g., `"USDT"`, `"BTC"`).

- `public required string Fiat`  
  The fiat currency used for quoting (e.g., `"ARS"`, `"VES"`).

- `public required DateTime PeriodStart`  
  The start timestamp of the backtest period, inclusive.

- `public required DateTime PeriodEnd`  
  The end timestamp of the backtest period, inclusive.

- `public required decimal InitialEquity`  
  The equity value at the beginning of the backtest period.

- `public required decimal FinalEquity`  
  The equity value at the end of the backtest period.

### `EquityCurvePoint`
A sealed record representing a single point on the equity curve.

*(Members are not specified in the provided public surface; the type is referenced via `IReadOnlyList<EquityCurvePoint>`.)*

### `MonteCarloPathSummary`
A sealed record summarizing the outcome of a single Monte Carlo simulation path.

*(Members are not specified in the provided public surface; the type is referenced via `IReadOnlyList<MonteCarloPathSummary>`.)*

## Usage

### Example 1: Evaluating a Signal and Displaying Key Metrics

```csharp
TradeSignal signal = strategy.Evaluate(asset: "USDT", fiat: "ARS", start: DateTime.UtcNow.AddDays(-30));

Console.WriteLine($"Backtest: {signal.Backtest.InitialEquity:F2} -> {signal.Backtest.FinalEquity:F2}");
Console.WriteLine($"Monte Carlo (n={signal.Iterations}):");
Console.WriteLine($"  Median Final Equity: {signal.MedianFinalEquity:F2}");
Console.WriteLine($"  Mean Final Equity:   {signal.MeanFinalEquity:F2}");
Console.WriteLine($"  95% CI:              [{signal.LowerConfidenceBound:F2}, {signal.UpperConfidenceBound:F2}]");
Console.WriteLine($"  VaR (95%):           {signal.ValueAtRisk:F2}");
Console.WriteLine($"  CVaR (95%):          {signal.ConditionalValueAtRisk:F2}");
Console.WriteLine($"  Probability of Profit: {signal.ProbabilityOfProfit:P1}");
Console.WriteLine($"  Median Max Drawdown:   {signal.MedianMaxDrawdownPercent:P1}");
```

### Example 2: Comparing Multiple Signals and Selecting the Best

```csharp
IReadOnlyList<TradeSignal> signals = monitor.GetAllSignals();

TradeSignal? best = signals
    .Where(s => s.ProbabilityOfProfit >= 0.6m)
    .Where(s => s.MedianMaxDrawdownPercent <= 0.15m)
    .MaxBy(s => s.MedianFinalEquity / s.Backtest.InitialEquity);

if (best is not null)
{
    Console.WriteLine($"Selected signal for {best.Backtest.Asset}/{best.Backtest.Fiat}");
    Console.WriteLine($"Expected return: {(best.MedianFinalEquity / best.Backtest.InitialEquity - 1):P1}");
    Console.WriteLine($"Worst-case path count below initial: " +
        $"{best.Paths.Count(p => p.FinalEquity < best.Backtest.InitialEquity)}");
}
```

## Notes

- All `required` properties must be initialized during construction; the compiler enforces this at build time. Omission of any required member results in a compilation error.
- `Iterations` is expected to be a positive integer. A value of zero or negative may produce degenerate statistics (e.g., empty `Paths` collection, zero-initialized aggregates). Consumers should validate this value before relying on Monte Carlo outputs.
- `ProbabilityOfProfit` is a decimal in the range `[0, 1]`. Values outside this range indicate a calculation error in the simulation engine.
- `ValueAtRisk` and `ConditionalValueAtRisk` are expressed as positive loss amounts. A VaR of `500` means a loss of 500 units of fiat currency at the configured confidence level.
- `MedianMaxDrawdownPercent` is expressed as a decimal fraction (e.g., `0.25` for 25%). It is always non-negative; a value of `0` indicates no drawdown occurred in the median path.
- The `Paths` collection has a length equal to `Iterations` when the simulation completes successfully. An empty collection with a non-zero `Iterations` value signals an execution failure.
- `EquityCurve` points are ordered chronologically. The first point corresponds to `PeriodStart` and the last to `PeriodEnd`.
- All record types are sealed and immutable. Once constructed, no property values can be modified. This guarantees thread-safety for concurrent read access without synchronization.
- The `IReadOnlyList<T>` collections provide a stable snapshot of simulation results. The underlying implementation is typically a list or array; callers should not assume the ability to cast to a mutable collection type.

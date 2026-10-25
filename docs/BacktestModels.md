# BacktestModels

`src/Backtesting/BacktestModels.cs` defines the immutable data types that the backtesting pipeline produces and consumes: enums describing signal direction and close reason, the trade and equity-curve records produced by a simulation, the Monte Carlo result types, the summary record, and the top-level `BacktestResult`. All types live in the `BinanceP2pMonitor.Backtesting` namespace. The file contains no logic beyond two derived properties; the values are computed by `BacktestingEngine` (see [BacktestingEngine](BacktestingEngine.md)).

## Overview

| Type | Kind | Produced by |
|------|------|-------------|
| `SignalDirection` | enum | `GenerateSignalsAsync` |
| `CloseReason` | enum | `RunBacktestAsync` (exits) |
| `TradeSignal` | record | `GenerateSignalsAsync` |
| `BacktestTrade` | record | `RunBacktestAsync` |
| `EquityCurvePoint` | record | `RunBacktestAsync` |
| `MonteCarloPathSummary` | record | Monte Carlo simulation (one per path) |
| `MonteCarloSimulationResult` | class | Monte Carlo simulation (aggregate) |
| `BacktestSummary` | record | `BacktestingEngine.CreateSummaryReport` |
| `BacktestResult` | class | `RunBacktestAsync` / `RunBacktestWithMonteCarloAsync` |

Records are `sealed`. `BacktestResult` and `MonteCarloSimulationResult` are `sealed` classes with `init`-only properties, so they are set once at construction.

## Enums

### SignalDirection
Directional intent of a generated trade signal.

| Member | Meaning in the current engine |
|--------|-------------------------------|
| `None` | Declared, but never emitted by the engine. |
| `Long` | The only value emitted. Used for every signal from `ExtractSignals`. |
| `Short` | Declared as reserved for future strategy extensions. Never emitted. |

### CloseReason
Reason a simulated position was closed. In `RunBacktestAsync`, the exit checks are applied in this priority order to each open position on each bar: `StopLoss`, then `TakeProfit`, then `SpreadExit`. The first matching condition wins.

| Member | Condition in the engine |
|--------|-------------------------|
| `StopLoss` | Mid-price change since entry is `<= -StopLossPercent`. |
| `TakeProfit` | Mid-price change since entry is `>= TakeProfitPercent`. |
| `SpreadExit` | Current bar spread is `< ExitSpreadThreshold`. |
| `EndOfData` | Position still open after the last bar; force-closed at the last mid-price. |
| `MaxHoldingPeriod` | Declared, but never produced by the current engine. |

## Signal

### TradeSignal
A strategy signal generated at a specific bar.

Positional parameters (all `init`, value equality):

*   **`DateTime Timestamp`**: UTC `RecordedAt` of the bar that produced the signal.
*   **`string Asset`**: Crypto asset symbol.
*   **`string Fiat`**: Fiat denomination.
*   **`SignalDirection Direction`**: Always `Long` in the current implementation.
*   **`decimal MidPrice`**: Mid-price of the bar.
*   **`decimal SpreadPercent`**: Bar spread, rounded to 4 decimal places.
*   **`string Reason`**: Human-readable text in the form `Spread {spread:F2}% > entry threshold {EntrySpreadThreshold}% and MA {maSpread:F2}%`.

A bar produces a signal only when its spread is greater than `EntrySpreadThreshold` **and** greater than the mean spread of the preceding `LookbackPeriod` bars.

## Trades

### BacktestTrade
A completed round-trip trade. Trades are appended in the order they close, and `Id` is assigned sequentially from 1 in that order. Positions force-closed at end of data also receive an `Id`.

Positional parameters:

*   **`int Id`**: Sequential identifier within the run, starting at 1.
*   **`string Asset`** / **`string Fiat`**: Traded pair.
*   **`DateTime EntryTime`**: `RecordedAt` of the entry bar.
*   **`DateTime ExitTime`**: `RecordedAt` of the exit bar.
*   **`decimal EntryPrice`** / **`decimal ExitPrice`**: Mid-prices at entry and exit.
*   **`decimal PositionValue`**: Fiat value allocated at entry (`equity × PositionSizeFraction`).
*   **`decimal GrossPnL`**: `PositionValue × priceChange%`, rounded to 8 decimal places. Excludes costs.
*   **`decimal NetPnL`**: `GrossPnL − entry cost − exit cost`, rounded to 8 decimal places. Each cost is `PositionValue × TransactionCostPercent / 100`.
*   **`decimal ReturnPercent`**: `NetPnL / PositionValue × 100`, rounded to 6 decimal places. `0` when `PositionValue` is not positive.
*   **`CloseReason CloseReason`**: Why the position was closed.

Computed members:

*   **`bool IsWinner`**: `NetPnL > 0`. A trade with exactly zero net PnL is counted as a loser.
*   **`TimeSpan HoldingPeriod`**: `ExitTime − EntryTime`.

## Equity curve

### EquityCurvePoint
One observation of portfolio state.

*   **`DateTime Timestamp`**: UTC `RecordedAt` of the observation.
*   **`decimal Equity`**: Portfolio equity in fiat, rounded to 4 decimal places.
*   **`decimal DrawdownPercent`**: `(peak − equity) / peak × 100`, rounded to 6 decimal places.

The curve is sampled as follows, which differs from the XML comment (see [Known discrepancies](#known-discrepancies)):

1. One initial point at the first bar, with equity `InitialEquity` and drawdown `0`.
2. One point per bar from index `LookbackPeriod` to the end of the history, after entry and exit processing for that bar.
3. One final point at the last bar, after end-of-data force-close.

Because step 3 reuses the last bar's timestamp, the final point may share a timestamp with the preceding point.

## Monte Carlo

### MonteCarloPathSummary
Summary of one simulated path. A path draws one trade return per historical trade, sampled with replacement from the `ReturnPercent` values. It adds zero-mean Gaussian noise whose standard deviation is the standard deviation of those historical returns multiplied by `VolatilityScaleFactor`. Each perturbed return is compounded on the current equity using `PositionSizeFraction`. Seeded runs (`RandomSeed`) are reproducible.

*   **`decimal FinalEquity`**: Equity at the end of the path, rounded to 2 decimal places.
*   **`decimal MaxDrawdownPercent`**: Largest peak-to-trough drawdown seen along the path, rounded to 4 decimal places.
*   **`decimal TotalReturnPercent`**: `(FinalEquity − InitialEquity) / InitialEquity × 100`, rounded to 4 decimal places.
*   **`decimal SharpeRatio`**: Sharpe ratio computed from the path's perturbed trade returns, rounded to 4 decimal places.

### MonteCarloSimulationResult
Aggregate statistics over all paths. It is a `sealed` class with `required` properties, so every field must be set at construction. Values are rounded as shown; percentile indices are derived from `ConfidenceLevel` and `MonteCarloIterations`.

| Property | Type | Definition |
|----------|------|-----------|
| `Iterations` | `int` | Number of paths generated. |
| `MedianFinalEquity` | `decimal` | Middle element of the sorted final equities (`[n / 2]`). |
| `MeanFinalEquity` | `decimal` | Arithmetic mean of final equities. |
| `LowerConfidenceBound` | `decimal` | Sorted final equity at index `floor(tail × n)`, where `tail = (1 − ConfidenceLevel) / 2`. |
| `UpperConfidenceBound` | `decimal` | Sorted final equity at index `min(ceil((1 − tail) × n) − 1, n − 1)`. |
| `ValueAtRisk` | `decimal` | `InitialEquity − finalEquity[varIdx]`, floored at 0, where `varIdx = max(0, floor((1 − ConfidenceLevel) × n) − 1)`. |
| `ConditionalValueAtRisk` | `decimal` | Mean of `InitialEquity − finalEquity[k]` for `k < varIdx`, floored at 0. `0` when `varIdx == 0`. |
| `ProbabilityOfProfit` | `decimal` | Fraction of paths whose final equity is strictly greater than `InitialEquity`. |
| `MedianMaxDrawdownPercent` | `decimal` | Median of the per-path `MaxDrawdownPercent` values. |
| `Paths` | `IReadOnlyList<MonteCarloPathSummary>` | One entry per path, in generation order. |

When there are no historical trades, the engine returns an empty result: the confidence bounds equal `InitialEquity`, and VaR, CVaR and `ProbabilityOfProfit` are `0`.

## Summary

### BacktestSummary
Compact view of a result for tabular display. Created by the static `BacktestingEngine.CreateSummaryReport(BacktestResult)`.

*   **`int TotalTrades`**: Number of completed trades.
*   **`decimal WinRate`**: Copied from `BacktestResult.WinRate` (fraction, 0–1).
*   **`decimal AverageProfit`**: Mean `NetPnL` across all trades, rounded to 4 decimal places.
*   **`decimal MaxDrawdown`**: Copied from `BacktestResult.MaxDrawdownPercent`.

If there are no trades, every field is `0`.

## Result

### BacktestResult
The complete output of one run. It is a `sealed` class with `required` properties except `MonteCarloResult`.

#### Identity and period
*   **`string Asset`** / **`string Fiat`**: Simulated pair.
*   **`DateTime PeriodStart`** / **`DateTime PeriodEnd`**: `RecordedAt` of the first and last bars of the history.
*   **`DateTime CalculatedAt`**: UTC time the result was assembled.
*   **`double PeriodDays`** (computed): `(PeriodEnd − PeriodStart).TotalDays`.

#### Equity and returns
*   **`decimal InitialEquity`**: Starting equity from `BacktestOptions`.
*   **`decimal FinalEquity`**: Last equity point, rounded to 2 decimal places.
*   **`decimal TotalReturnPercent`**: `(FinalEquity − InitialEquity) / InitialEquity × 100`, rounded to 4 decimal places. Computed from the last curve point.
*   **`decimal AnnualisedReturnPercent`**: `((FinalEquity / InitialEquity)^(365 / days) − 1) × 100`, where `days` is the span between the first and last bars. `0` when `days ≤ 0` or `InitialEquity ≤ 0`.

#### Trade statistics
*   **`int TotalTrades`**: Count of `Trades`.
*   **`decimal WinRate`**: Winning trades divided by total trades, rounded to 6 decimal places. `0` with no trades.
*   **`decimal AverageWin`**: Mean `NetPnL` of winning trades, rounded to 4 decimal places.
*   **`decimal AverageLoss`**: Absolute mean `NetPnL` of losing trades, rounded to 4 decimal places. Always non-negative.
*   **`decimal ProfitFactor`**: Sum of winning `NetPnL` divided by absolute sum of losing `NetPnL`. Capped at `999.9999`. Equals `999.9999` when there are wins and no losses, and `0` when there are no wins and no losses.

#### Risk-adjusted metrics
*   **`decimal SharpeRatio`**: Mean of trade `ReturnPercent` values divided by their population standard deviation, multiplied by `√N`. `0` for fewer than 2 trades or zero deviation.
*   **`decimal SortinoRatio`**: Mean return divided by downside deviation (root of mean squared negative returns, dividing by the count of negative returns), multiplied by `√N`. If there are no negative returns, it is `99.9999` when the mean is positive and `0` otherwise.
*   **`decimal MaxDrawdownPercent`**: Maximum `DrawdownPercent` over the equity curve, rounded to 4 decimal places.
*   **`decimal CalmarRatio`**: `AnnualisedReturnPercent / MaxDrawdownPercent`, rounded to 4 decimal places. `0` when the drawdown is not positive.

#### Collections and optional output
*   **`IReadOnlyList<BacktestTrade> Trades`**: All completed trades in closing order.
*   **`IReadOnlyList<EquityCurvePoint> EquityCurve`**: Equity samples as described in [Equity curve](#equity-curve).
*   **`MonteCarloSimulationResult? MonteCarloResult`**: `null` for `RunBacktestAsync`. Populated by `RunBacktestWithMonteCarloAsync`.

## Relationships

```
BacktestOptions ──► BacktestingEngine ──► BacktestResult
                                           ├─ Trades: BacktestTrade[] (CloseReason)
                                           ├─ EquityCurve: EquityCurvePoint[]
                                           └─ MonteCarloResult?: MonteCarloSimulationResult
                                                                   └─ Paths: MonteCarloPathSummary[]
BacktestResult ──► CreateSummaryReport ──► BacktestSummary

GenerateSignalsAsync ──► TradeSignal[] (SignalDirection.Long)
```

## Known discrepancies

The XML doc comments in `BacktestModels.cs` and `IBacktestingService.cs` do not match the current engine in these places. This document follows the code. The comments were not changed.

1. **Equity curve sampling.** `EquityCurvePoint`'s summary and `BacktestResult.EquityCurve` say the curve is sampled at trade events. The engine samples at every bar from `LookbackPeriod` onward, plus the initial and final points.
2. **Path ordering.** `MonteCarloSimulationResult.Paths` is documented as ordered by final equity ascending. Paths are stored in generation order. Only an internal array of final equities is sorted.
3. **Unused enum members.** `SignalDirection.None`, `SignalDirection.Short` and `CloseReason.MaxHoldingPeriod` are declared but never produced.
4. **Signal exits.** `IBacktestingService.GenerateSignalsAsync` says the output includes "implied exits". The engine emits entry (`Long`) signals only.

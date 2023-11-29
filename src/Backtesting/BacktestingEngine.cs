#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Backtesting;

/// <summary>
/// Core engine that simulates a spread-momentum trading strategy over historical P2P
/// price data and optionally follows it with a bootstrap Monte Carlo simulation.
/// </summary>
/// <remarks>
/// <para>
/// <b>Strategy</b> — At each bar the engine checks open positions for stop-loss,
/// take-profit, or spread-compression exits before evaluating a new entry. An entry
/// fires when the current spread exceeds <see cref="BacktestOptions.EntrySpreadThreshold"/>
/// <em>and</em> is above its own rolling-average over the lookback window (momentum
/// confirmation). Position sizing is fixed-fractional: each position consumes a
/// configurable fraction of available equity.
/// </para>
/// <para>
/// <b>Monte Carlo</b> — The empirical trade-return sequence is bootstrapped with
/// replacement; each sampled return is perturbed with zero-mean Gaussian noise
/// scaled by the empirical standard deviation and
/// <see cref="BacktestOptions.VolatilityScaleFactor"/>. Statistics (VaR, CVaR,
/// confidence intervals, probability of profit) are aggregated across all paths.
/// </para>
/// </remarks>
public sealed class BacktestingEngine : IBacktestingService
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IHistoricalSpreadAnalysisService _spreadAnalysis;
    private readonly AppSettings _settings;
    private readonly ILogger<BacktestingEngine> _logger;

    /// <summary>
    /// Initialises a new <see cref="BacktestingEngine"/> with the required dependencies.
    /// </summary>
    public BacktestingEngine(
        IHistoryRepository historyRepository,
        IHistoricalSpreadAnalysisService spreadAnalysis,
        AppSettings settings,
        ILogger<BacktestingEngine> logger)
    {
        _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
        _spreadAnalysis = spreadAnalysis ?? throw new ArgumentNullException(nameof(spreadAnalysis));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<BacktestResult> RunBacktestAsync(
        string asset, string fiat, BacktestOptions options,
        int lookbackHours = 720, CancellationToken ct = default)
    {
        options.Validate();
        var history = await LoadHistoryAsync(asset, fiat, lookbackHours, options.LookbackPeriod, ct);

        _logger.LogInformation(
            "Starting backtest for {Asset}/{Fiat} over {Bars} bars ({Hours}h window)",
            asset, fiat, history.Count, lookbackHours);

        var (trades, curve) = SimulateStrategy(history, options);
        return BuildResult(asset, fiat, history, trades, curve, options, monteCarloResult: null);
    }

    /// <inheritdoc />
    public async Task<BacktestResult> RunBacktestWithMonteCarloAsync(
        string asset, string fiat, BacktestOptions options,
        int lookbackHours = 720, CancellationToken ct = default)
    {
        options.Validate();
        var history = await LoadHistoryAsync(asset, fiat, lookbackHours, options.LookbackPeriod, ct);

        _logger.LogInformation(
            "Starting backtest+MC for {Asset}/{Fiat}: {Bars} bars, {Iterations} MC paths",
            asset, fiat, history.Count, options.MonteCarloIterations);

        var (trades, curve) = SimulateStrategy(history, options);
        var mc = RunMonteCarloSimulation(trades, options);
        return BuildResult(asset, fiat, history, trades, curve, options, mc);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TradeSignal>> GenerateSignalsAsync(
        string asset, string fiat, BacktestOptions options,
        int lookbackHours = 720, CancellationToken ct = default)
    {
        options.Validate();
        var history = await LoadHistoryAsync(asset, fiat, lookbackHours, options.LookbackPeriod, ct);
        return ExtractSignals(history, options);
    }

    // ── Private — data loading ───────────────────────────────────────────────

    private async Task<IReadOnlyList<PriceHistory>> LoadHistoryAsync(
        string asset, string fiat, int hours, int minBars, CancellationToken ct)
    {
        var raw = await _historyRepository.GetHistoryByAssetAndFiatAsync(asset, fiat, hours);
        var history = raw.OrderBy(h => h.RecordedAt).ToList();

        if (history.Count < minBars)
            throw new InvalidOperationException(
                $"Insufficient history for {asset}/{fiat}: need ≥{minBars} bars, got {history.Count}. " +
                $"Increase lookbackHours or reduce LookbackPeriod.");

        return history;
    }

    // ── Private — strategy simulation ───────────────────────────────────────

    private (IReadOnlyList<BacktestTrade> trades, IReadOnlyList<EquityCurvePoint> curve)
        SimulateStrategy(IReadOnlyList<PriceHistory> history, BacktestOptions options)
    {
        // (entryBarIndex, entryMid, positionValue, entryTransCost)
        var openPositions = new List<(int Bar, decimal Mid, decimal Value, decimal EntryCost)>(
            options.MaxConcurrentPositions);

        var trades = new List<BacktestTrade>(history.Count / options.LookbackPeriod + 1);
        var curve = new List<EquityCurvePoint>(history.Count + 1);

        decimal equity = options.InitialEquity;
        decimal peakEquity = equity;
        int tradeId = 0;

        curve.Add(new EquityCurvePoint(history[0].RecordedAt, equity, 0m));

        for (int i = options.LookbackPeriod; i < history.Count; i++)
        {
            var bar = history[i];
            decimal midPrice = bar.GetMidPrice();
            decimal spread = bar.CalculateSpread();

            // ── 1. Process exits on open positions ─────────────────────────
            for (int p = openPositions.Count - 1; p >= 0; p--)
            {
                var pos = openPositions[p];
                decimal priceChange = pos.Mid > 0
                    ? (midPrice - pos.Mid) / pos.Mid * 100m
                    : 0m;

                CloseReason? reason = priceChange <= -options.StopLossPercent ? CloseReason.StopLoss
                    : priceChange >= options.TakeProfitPercent ? CloseReason.TakeProfit
                    : spread < options.ExitSpreadThreshold ? CloseReason.SpreadExit
                    : null;

                if (reason is null)
                    continue;

                decimal exitCost = pos.Value * options.TransactionCostPercent / 100m;
                decimal grossPnl = pos.Value * priceChange / 100m;
                decimal netPnl = grossPnl - pos.EntryCost - exitCost;
                decimal returnPct = pos.Value > 0 ? Math.Round(netPnl / pos.Value * 100m, 6) : 0m;

                // Restore capital: pos.Value was already debited on entry
                equity += pos.Value + grossPnl - exitCost;

                trades.Add(new BacktestTrade(
                    ++tradeId,
                    bar.Asset, bar.Fiat,
                    history[pos.Bar].RecordedAt, bar.RecordedAt,
                    pos.Mid, midPrice,
                    pos.Value,
                    Math.Round(grossPnl, 8),
                    Math.Round(netPnl, 8),
                    returnPct,
                    reason.Value));

                openPositions.RemoveAt(p);
            }

            // ── 2. Evaluate entry ──────────────────────────────────────────
            if (openPositions.Count < options.MaxConcurrentPositions
                && spread > options.EntrySpreadThreshold)
            {
                // Rolling MA of spread over lookback window — direct loop avoids LINQ
                decimal maSum = 0m;
                for (int k = i - options.LookbackPeriod; k < i; k++)
                    maSum += history[k].CalculateSpread();
                decimal maSpread = maSum / options.LookbackPeriod;

                // Momentum filter: spread must be above its own rolling average
                if (spread > maSpread)
                {
                    decimal positionValue = equity * options.PositionSizeFraction;
                    if (positionValue > 0 && equity >= positionValue)
                    {
                        decimal entryCost = positionValue * options.TransactionCostPercent / 100m;
                        equity -= positionValue + entryCost;
                        openPositions.Add((i, midPrice, positionValue, entryCost));
                    }
                }
            }

            // ── 3. Record equity curve point ───────────────────────────────
            peakEquity = Math.Max(peakEquity, equity);
            decimal drawdown = peakEquity > 0 ? (peakEquity - equity) / peakEquity * 100m : 0m;
            curve.Add(new EquityCurvePoint(bar.RecordedAt, Math.Round(equity, 4), Math.Round(drawdown, 6)));
        }

        // ── 4. Force-close any remaining positions at end of data ──────────
        var lastBar = history[^1];
        decimal lastMid = lastBar.GetMidPrice();

        for (int p = openPositions.Count - 1; p >= 0; p--)
        {
            var pos = openPositions[p];
            decimal priceChange = pos.Mid > 0 ? (lastMid - pos.Mid) / pos.Mid * 100m : 0m;
            decimal exitCost = pos.Value * options.TransactionCostPercent / 100m;
            decimal grossPnl = pos.Value * priceChange / 100m;
            decimal netPnl = grossPnl - pos.EntryCost - exitCost;
            decimal returnPct = pos.Value > 0 ? Math.Round(netPnl / pos.Value * 100m, 6) : 0m;

            equity += pos.Value + grossPnl - exitCost;

            trades.Add(new BacktestTrade(
                ++tradeId,
                lastBar.Asset, lastBar.Fiat,
                history[pos.Bar].RecordedAt, lastBar.RecordedAt,
                pos.Mid, lastMid,
                pos.Value,
                Math.Round(grossPnl, 8),
                Math.Round(netPnl, 8),
                returnPct,
                CloseReason.EndOfData));
        }

        peakEquity = Math.Max(peakEquity, equity);
        decimal finalDrawdown = peakEquity > 0 ? (peakEquity - equity) / peakEquity * 100m : 0m;
        curve.Add(new EquityCurvePoint(lastBar.RecordedAt, Math.Round(equity, 4), Math.Round(finalDrawdown, 6)));

        _logger.LogInformation(
            "Strategy simulation complete — {Trades} trades, final equity {Equity:F2}",
            trades.Count, equity);

        return (trades, curve);
    }

    // ── Private — signal extraction ──────────────────────────────────────────

    private static IReadOnlyList<TradeSignal> ExtractSignals(
        IReadOnlyList<PriceHistory> history, BacktestOptions options)
    {
        var signals = new List<TradeSignal>();

        for (int i = options.LookbackPeriod; i < history.Count; i++)
        {
            var bar = history[i];
            decimal spread = bar.CalculateSpread();

            if (spread <= options.EntrySpreadThreshold)
                continue;

            decimal maSum = 0m;
            for (int k = i - options.LookbackPeriod; k < i; k++)
                maSum += history[k].CalculateSpread();
            decimal maSpread = maSum / options.LookbackPeriod;

            if (spread > maSpread)
            {
                signals.Add(new TradeSignal(
                    bar.RecordedAt, bar.Asset, bar.Fiat,
                    SignalDirection.Long,
                    bar.GetMidPrice(),
                    Math.Round(spread, 4),
                    $"Spread {spread:F2}% > entry threshold {options.EntrySpreadThreshold}% and MA {maSpread:F2}%"));
            }
        }

        return signals;
    }

    // ── Private — Monte Carlo simulation ─────────────────────────────────────

    private MonteCarloSimulationResult RunMonteCarloSimulation(
        IReadOnlyList<BacktestTrade> trades, BacktestOptions options)
    {
        if (trades.Count == 0)
            return EmptyMonteCarloResult(options);

        var empiricalReturns = trades.Select(t => t.ReturnPercent).ToArray();
        decimal empiricalStdDev = PriceCalculator.CalculateStandardDeviation(empiricalReturns);
        double noiseScale = (double)(empiricalStdDev * options.VolatilityScaleFactor);

        var rng = options.RandomSeed.HasValue
            ? new Random(options.RandomSeed.Value)
            : Random.Shared;

        int n = options.MonteCarloIterations;
        var paths = new List<MonteCarloPathSummary>(n);
        var finalEquities = new decimal[n];

        for (int iter = 0; iter < n; iter++)
        {
            decimal equity = options.InitialEquity;
            decimal peakEquity = equity;
            decimal maxDrawdown = 0m;
            decimal sumReturns = 0m;
            decimal sumSqReturns = 0m;

            for (int t = 0; t < empiricalReturns.Length; t++)
            {
                // Bootstrap: sample a historical trade return with replacement
                decimal sampledReturn = empiricalReturns[rng.Next(empiricalReturns.Length)];

                // Perturb with zero-mean Gaussian noise
                decimal noise = noiseScale > 0
                    ? (decimal)(SampleGaussian(rng) * noiseScale)
                    : 0m;

                decimal perturbedReturn = sampledReturn + noise;

                decimal tradeValue = equity * options.PositionSizeFraction;
                equity += tradeValue * perturbedReturn / 100m;

                sumReturns += perturbedReturn;
                sumSqReturns += perturbedReturn * perturbedReturn;

                peakEquity = Math.Max(peakEquity, equity);
                if (peakEquity > 0)
                {
                    decimal dd = (peakEquity - equity) / peakEquity * 100m;
                    maxDrawdown = Math.Max(maxDrawdown, dd);
                }
            }

            int count = empiricalReturns.Length;
            decimal totalReturn = options.InitialEquity > 0
                ? (equity - options.InitialEquity) / options.InitialEquity * 100m
                : 0m;

            decimal pathSharpe = CalculatePathSharpe(sumReturns, sumSqReturns, count);

            finalEquities[iter] = equity;
            paths.Add(new MonteCarloPathSummary(
                Math.Round(equity, 2),
                Math.Round(maxDrawdown, 4),
                Math.Round(totalReturn, 4),
                Math.Round(pathSharpe, 4)));
        }

        Array.Sort(finalEquities);

        decimal tailFrac = (1m - options.ConfidenceLevel) / 2m;
        int lowerIdx = (int)Math.Floor((double)tailFrac * n);
        int upperIdx = Math.Min((int)Math.Ceiling((double)(1m - tailFrac) * n) - 1, n - 1);
        int varIdx = Math.Max(0, (int)Math.Floor((double)(1m - options.ConfidenceLevel) * n) - 1);

        decimal varLoss = options.InitialEquity - finalEquities[varIdx];

        decimal cvarLoss = 0m;
        if (varIdx > 0)
        {
            decimal tailSum = 0m;
            for (int k = 0; k < varIdx; k++)
                tailSum += options.InitialEquity - finalEquities[k];
            cvarLoss = tailSum / varIdx;
        }

        int profitCount = 0;
        for (int k = 0; k < n; k++)
            if (finalEquities[k] > options.InitialEquity) profitCount++;

        var sortedDrawdowns = paths.Select(p => p.MaxDrawdownPercent).OrderBy(d => d).ToList();
        decimal medianDrawdown = CalculatePercentile(sortedDrawdowns, 50m);
        decimal medianEquity = finalEquities[n / 2];
        decimal meanEquity = 0m;
        for (int k = 0; k < n; k++) meanEquity += finalEquities[k];
        meanEquity /= n;

        _logger.LogInformation(
            "Monte Carlo complete — {Iterations} paths, median equity {Median:F2}, P(profit)={PProfit:P1}",
            n, medianEquity, (double)profitCount / n);

        return new MonteCarloSimulationResult
        {
            Iterations = n,
            MedianFinalEquity = Math.Round(medianEquity, 2),
            MeanFinalEquity = Math.Round(meanEquity, 2),
            LowerConfidenceBound = Math.Round(finalEquities[lowerIdx], 2),
            UpperConfidenceBound = Math.Round(finalEquities[upperIdx], 2),
            ValueAtRisk = Math.Round(Math.Max(0m, varLoss), 2),
            ConditionalValueAtRisk = Math.Round(Math.Max(0m, cvarLoss), 2),
            ProbabilityOfProfit = Math.Round((decimal)profitCount / n, 6),
            MedianMaxDrawdownPercent = Math.Round(medianDrawdown, 4),
            Paths = paths
        };
    }

    // ── Private — result assembly ─────────────────────────────────────────────

    private static BacktestResult BuildResult(
        string asset, string fiat,
        IReadOnlyList<PriceHistory> history,
        IReadOnlyList<BacktestTrade> trades,
        IReadOnlyList<EquityCurvePoint> curve,
        BacktestOptions options,
        MonteCarloSimulationResult? monteCarloResult)
    {
        decimal finalEquity = curve.Count > 0 ? curve[^1].Equity : options.InitialEquity;
        decimal totalReturn = options.InitialEquity > 0
            ? (finalEquity - options.InitialEquity) / options.InitialEquity * 100m
            : 0m;

        double days = (history[^1].RecordedAt - history[0].RecordedAt).TotalDays;
        decimal annualisedReturn = days > 0 && options.InitialEquity > 0
            ? ((decimal)Math.Pow((double)(finalEquity / options.InitialEquity), 365.0 / days) - 1m) * 100m
            : 0m;

        var winners = trades.Where(t => t.IsWinner).ToList();
        var losers = trades.Where(t => !t.IsWinner).ToList();

        decimal winRate = trades.Count > 0 ? (decimal)winners.Count / trades.Count : 0m;
        decimal avgWin = winners.Count > 0 ? winners.Average(t => t.NetPnL) : 0m;
        decimal avgLoss = losers.Count > 0 ? Math.Abs(losers.Average(t => t.NetPnL)) : 0m;
        decimal totalWins = winners.Sum(t => t.NetPnL);
        decimal totalLosses = Math.Abs(losers.Sum(t => t.NetPnL));
        decimal profitFactor = totalLosses > 0 ? totalWins / totalLosses
            : totalWins > 0 ? 999.9999m : 0m;

        var returns = trades.Select(t => t.ReturnPercent).ToList();
        decimal sharpe = CalculateSharpeRatio(returns);
        decimal sortino = CalculateSortinoRatio(returns);
        decimal maxDrawdown = curve.Count > 0 ? curve.Max(p => p.DrawdownPercent) : 0m;
        decimal calmar = maxDrawdown > 0 ? annualisedReturn / maxDrawdown : 0m;

        return new BacktestResult
        {
            Asset = asset,
            Fiat = fiat,
            PeriodStart = history[0].RecordedAt,
            PeriodEnd = history[^1].RecordedAt,
            InitialEquity = options.InitialEquity,
            FinalEquity = Math.Round(finalEquity, 2),
            TotalReturnPercent = Math.Round(totalReturn, 4),
            AnnualisedReturnPercent = Math.Round(annualisedReturn, 4),
            TotalTrades = trades.Count,
            WinRate = Math.Round(winRate, 6),
            AverageWin = Math.Round(avgWin, 4),
            AverageLoss = Math.Round(avgLoss, 4),
            ProfitFactor = Math.Round(Math.Min(profitFactor, 999.9999m), 4),
            SharpeRatio = Math.Round(sharpe, 4),
            SortinoRatio = Math.Round(sortino, 4),
            MaxDrawdownPercent = Math.Round(maxDrawdown, 4),
            CalmarRatio = Math.Round(calmar, 4),
            Trades = trades,
            EquityCurve = curve,
            MonteCarloResult = monteCarloResult,
            CalculatedAt = DateTime.UtcNow
        };
    }

    // ── Private — statistics helpers ─────────────────────────────────────────

    /// <summary>
    /// Trade-normalised Sharpe ratio: (mean_return / std_return) × √N.
    /// </summary>
    private static decimal CalculateSharpeRatio(IList<decimal> returns)
    {
        int n = returns.Count;
        if (n < 2) return 0m;

        decimal mean = 0m;
        for (int i = 0; i < n; i++) mean += returns[i];
        mean /= n;

        decimal variance = 0m;
        for (int i = 0; i < n; i++)
        {
            decimal d = returns[i] - mean;
            variance += d * d;
        }
        variance /= n;

        decimal stdDev = (decimal)Math.Sqrt((double)variance);
        return stdDev > 0 ? mean / stdDev * (decimal)Math.Sqrt(n) : 0m;
    }

    /// <summary>
    /// Sortino ratio: penalises only downside deviation, ignoring upside volatility.
    /// </summary>
    private static decimal CalculateSortinoRatio(IList<decimal> returns)
    {
        int n = returns.Count;
        if (n < 2) return 0m;

        decimal mean = 0m;
        for (int i = 0; i < n; i++) mean += returns[i];
        mean /= n;

        decimal downsideVariance = 0m;
        int downsideCount = 0;
        for (int i = 0; i < n; i++)
        {
            if (returns[i] < 0)
            {
                downsideVariance += returns[i] * returns[i];
                downsideCount++;
            }
        }

        if (downsideCount == 0)
            return mean > 0 ? 99.9999m : 0m;

        downsideVariance /= downsideCount;
        decimal downsideStdDev = (decimal)Math.Sqrt((double)downsideVariance);
        return downsideStdDev > 0 ? mean / downsideStdDev * (decimal)Math.Sqrt(n) : 0m;
    }

    /// <summary>
    /// Computes the Sharpe ratio for a single MC path from pre-aggregated sums,
    /// avoiding a second pass over the return array.
    /// </summary>
    private static decimal CalculatePathSharpe(decimal sumR, decimal sumSqR, int n)
    {
        if (n < 2) return 0m;
        decimal mean = sumR / n;
        decimal variance = sumSqR / n - mean * mean;
        if (variance <= 0) return 0m;
        decimal stdDev = (decimal)Math.Sqrt((double)variance);
        return stdDev > 0 ? mean / stdDev * (decimal)Math.Sqrt(n) : 0m;
    }

    /// <summary>
    /// Linear-interpolation percentile over a pre-sorted list.
    /// </summary>
    private static decimal CalculatePercentile(IList<decimal> sorted, decimal percentile)
    {
        if (sorted.Count == 0) return 0m;
        double idx = (double)(percentile / 100m) * (sorted.Count - 1);
        int lo = (int)Math.Floor(idx);
        int hi = Math.Min(lo + 1, sorted.Count - 1);
        decimal frac = (decimal)(idx - lo);
        return sorted[lo] + frac * (sorted[hi] - sorted[lo]);
    }

    /// <summary>
    /// Box-Muller transform: produces a standard-normal sample from two uniform draws.
    /// </summary>
    private static double SampleGaussian(Random rng)
    {
        double u1 = 1.0 - rng.NextDouble();
        double u2 = 1.0 - rng.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    }

    private static MonteCarloSimulationResult EmptyMonteCarloResult(BacktestOptions options) =>
        new()
        {
            Iterations = 0,
            MedianFinalEquity = options.InitialEquity,
            MeanFinalEquity = options.InitialEquity,
            LowerConfidenceBound = options.InitialEquity,
            UpperConfidenceBound = options.InitialEquity,
            ValueAtRisk = 0m,
            ConditionalValueAtRisk = 0m,
            ProbabilityOfProfit = 0m,
            MedianMaxDrawdownPercent = 0m,
            Paths = []
        };
}

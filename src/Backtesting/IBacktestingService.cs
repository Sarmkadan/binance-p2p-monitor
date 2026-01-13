#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Backtesting;

/// <summary>
/// Contract for running historical strategy simulations and Monte Carlo analyses
/// against recorded P2P price data.
/// </summary>
public interface IBacktestingService
{
    /// <summary>
    /// Replays the spread-momentum strategy over the most recent
    /// <paramref name="lookbackHours"/> hours of recorded price history for the
    /// given asset/fiat pair and returns a full set of performance metrics.
    /// </summary>
    /// <param name="asset">Crypto asset symbol (e.g. <c>BTC</c>).</param>
    /// <param name="fiat">Fiat denomination (e.g. <c>USDT</c>).</param>
    /// <param name="options">Strategy and simulation parameters.</param>
    /// <param name="lookbackHours">
    /// Number of hours of history to load. Defaults to 720 (30 days).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="BacktestResult"/> containing trade log, equity curve,
    /// and aggregate risk/return metrics.
    /// </returns>
    Task<BacktestResult> RunBacktestAsync(
        string asset,
        string fiat,
        BacktestOptions options,
        int lookbackHours = 720,
        CancellationToken ct = default);

    /// <summary>
    /// Runs the same strategy backtest as <see cref="RunBacktestAsync"/> and
    /// then executes a Monte Carlo simulation over the resulting trade-return
    /// distribution to produce probabilistic outcome projections.
    /// </summary>
    /// <param name="asset">Crypto asset symbol.</param>
    /// <param name="fiat">Fiat denomination.</param>
    /// <param name="options">
    /// Strategy parameters; the <c>MonteCarloIterations</c>,
    /// <c>ConfidenceLevel</c>, <c>RandomSeed</c>, and
    /// <c>VolatilityScaleFactor</c> fields govern the simulation.
    /// </param>
    /// <param name="lookbackHours">Hours of history to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="BacktestResult"/> whose <see cref="BacktestResult.MonteCarloResult"/>
    /// property is populated with VaR, CVaR, confidence intervals, and per-path summaries.
    /// </returns>
    Task<BacktestResult> RunBacktestWithMonteCarloAsync(
        string asset,
        string fiat,
        BacktestOptions options,
        int lookbackHours = 720,
        CancellationToken ct = default);

    /// <summary>
    /// Generates the sequence of trade signals that the strategy would have produced
    /// over the specified historical window without executing any simulated trades.
    /// Useful for signal-quality analysis and parameter tuning.
    /// </summary>
    /// <param name="asset">Crypto asset symbol.</param>
    /// <param name="fiat">Fiat denomination.</param>
    /// <param name="options">Strategy parameters that govern signal generation.</param>
    /// <param name="lookbackHours">Hours of history to scan.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Ordered sequence of <see cref="TradeSignal"/> records — both entries and implied exits.
    /// </returns>
    Task<IReadOnlyList<TradeSignal>> GenerateSignalsAsync(
        string asset,
        string fiat,
        BacktestOptions options,
        int lookbackHours = 720,
        CancellationToken ct = default);
}

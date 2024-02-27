#nullable enable

namespace BinanceP2pMonitor.Backtesting;

/// <summary>
/// Extension methods for <see cref="BacktestingEngine"/> that provide additional utility functionality
/// for backtesting workflows, result analysis, and signal processing.
/// </summary>
public static class BacktestingEngineExtensions
{
    /// <summary>
    /// Runs a backtest and immediately returns the equity curve as a sequence of points.
    /// Useful when you need the full equity progression rather than just the final result.
    /// </summary>
    /// <param name="engine">The backtesting engine instance.</param>
    /// <param name="asset">The asset symbol to test (e.g., "USDT").</param>
    /// <param name="fiat">The fiat currency (e.g., "UAH").</param>
    /// <param name="options">The backtest configuration options.</param>
    /// <param name="lookbackHours">Number of hours of historical data to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An enumerable sequence of equity curve points representing the backtest progression.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="options"/> is null.</exception>
    public static async Task<IReadOnlyList<EquityCurvePoint>> RunBacktestWithCurveAsync(
        this BacktestingEngine engine,
        string asset,
        string fiat,
        BacktestOptions options,
        int lookbackHours = 720,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(options);

        var result = await engine.RunBacktestAsync(asset, fiat, options, lookbackHours, ct).ConfigureAwait(false);
        return result.EquityCurve;
    }

    /// <summary>
    /// Runs a Monte Carlo simulation and returns the raw path summaries without aggregation.
    /// Useful for analyzing individual simulation paths or custom aggregation logic.
    /// </summary>
    /// <param name="engine">The backtesting engine instance.</param>
    /// <param name="asset">The asset symbol to test.</param>
    /// <param name="fiat">The fiat currency.</param>
    /// <param name="options">The backtest configuration options.</param>
    /// <param name="lookbackHours">Number of hours of historical data to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of Monte Carlo path summaries, one per simulation iteration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="options"/> is null.</exception>
    public static async Task<IReadOnlyList<MonteCarloPathSummary>> RunMonteCarloWithPathsAsync(
        this BacktestingEngine engine,
        string asset,
        string fiat,
        BacktestOptions options,
        int lookbackHours = 720,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(options);

        var result = await engine.RunBacktestWithMonteCarloAsync(asset, fiat, options, lookbackHours, ct).ConfigureAwait(false);
        return result.MonteCarloResult?.Paths ?? Array.Empty<MonteCarloPathSummary>();
    }

    /// <summary>
    /// Generates trade signals and immediately filters them by a minimum spread threshold.
    /// Useful for creating signal watchlists or filtering signals before execution.
    /// </summary>
    /// <param name="engine">The backtesting engine instance.</param>
    /// <param name="asset">The asset symbol to test.</param>
    /// <param name="fiat">The fiat currency.</param>
    /// <param name="options">The backtest configuration options.</param>
    /// <param name="minSpreadPercent">Minimum spread percentage required for a signal to be included.</param>
    /// <param name="lookbackHours">Number of hours of historical data to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Filtered list of trade signals that meet the minimum spread requirement.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minSpreadPercent"/> is negative.</exception>
    public static async Task<IReadOnlyList<TradeSignal>> GenerateSignalsWithMinSpreadAsync(
        this BacktestingEngine engine,
        string asset,
        string fiat,
        BacktestOptions options,
        decimal minSpreadPercent,
        int lookbackHours = 720,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(minSpreadPercent);

        var signals = await engine.GenerateSignalsAsync(asset, fiat, options, lookbackHours, ct).ConfigureAwait(false);
        return signals.Where(s => s.SpreadPercent >= minSpreadPercent).ToList();
    }

    /// <summary>
    /// Runs a quick backtest and returns only the key performance metrics without full result object.
    /// Useful for fast iteration during strategy research when you only need high-level metrics.
    /// </summary>
    /// <param name="engine">The backtesting engine instance.</param>
    /// <param name="asset">The asset symbol to test.</param>
    /// <param name="fiat">The fiat currency.</param>
    /// <param name="options">The backtest configuration options.</param>
    /// <param name="lookbackHours">Number of hours of historical data to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple containing the most important performance metrics: total return %, max drawdown %, and win rate.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="options"/> is null.</exception>
    public static async Task<(decimal TotalReturnPercent, decimal MaxDrawdownPercent, decimal WinRate)> GetQuickMetricsAsync(
        this BacktestingEngine engine,
        string asset,
        string fiat,
        BacktestOptions options,
        int lookbackHours = 720,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(options);

        var result = await engine.RunBacktestAsync(asset, fiat, options, lookbackHours, ct).ConfigureAwait(false);

        return (
            result.TotalReturnPercent,
            result.MaxDrawdownPercent,
            result.WinRate
        );
    }
}
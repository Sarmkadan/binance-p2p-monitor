#nullable enable
namespace BinanceP2pMonitor.Backtesting;

/// <summary>Directional intent of a generated trade signal.</summary>
public enum SignalDirection
{
    /// <summary>No actionable signal — market conditions do not meet strategy criteria.</summary>
    None,
    /// <summary>Buy signal: spread is wide and momentum confirms an opportunity.</summary>
    Long,
    /// <summary>Sell short signal: reserved for future strategy extensions.</summary>
    Short
}

/// <summary>Reason a simulated position was closed.</summary>
public enum CloseReason
{
    /// <summary>Spread compressed below the configured exit threshold.</summary>
    SpreadExit,
    /// <summary>Mid-price fell through the stop-loss level.</summary>
    StopLoss,
    /// <summary>Mid-price reached the take-profit level.</summary>
    TakeProfit,
    /// <summary>Position was open when the historical data window ended.</summary>
    EndOfData,
    /// <summary>Position exceeded the maximum configured holding period.</summary>
    MaxHoldingPeriod
}

/// <summary>
/// Represents a strategy signal generated at a specific bar during simulation.
/// </summary>
/// <param name="Timestamp">UTC time of the bar that generated the signal.</param>
/// <param name="Asset">Crypto asset symbol.</param>
/// <param name="Fiat">Fiat denomination.</param>
/// <param name="Direction">Long, Short, or None.</param>
/// <param name="MidPrice">Mid-price at signal time.</param>
/// <param name="SpreadPercent">Bid-ask spread percentage that triggered the signal.</param>
/// <param name="Reason">Human-readable description of why the signal fired.</param>
public sealed record TradeSignal(
    DateTime Timestamp,
    string Asset,
    string Fiat,
    SignalDirection Direction,
    decimal MidPrice,
    decimal SpreadPercent,
    string Reason);

/// <summary>
/// Represents a completed round-trip trade produced during strategy simulation.
/// </summary>
/// <param name="Id">Sequential identifier within the backtest run.</param>
/// <param name="Asset">Crypto asset traded.</param>
/// <param name="Fiat">Fiat denomination.</param>
/// <param name="EntryTime">UTC time the position was opened.</param>
/// <param name="ExitTime">UTC time the position was closed.</param>
/// <param name="EntryPrice">Mid-price at entry.</param>
/// <param name="ExitPrice">Mid-price at exit.</param>
/// <param name="PositionValue">Fiat value of the position at entry.</param>
/// <param name="GrossPnL">Profit or loss before transaction costs.</param>
/// <param name="NetPnL">Profit or loss after all round-trip transaction costs.</param>
/// <param name="ReturnPercent">Net return as a percentage of the invested position value.</param>
/// <param name="CloseReason">Condition that triggered the exit.</param>
public sealed record BacktestTrade(
    int Id,
    string Asset,
    string Fiat,
    DateTime EntryTime,
    DateTime ExitTime,
    decimal EntryPrice,
    decimal ExitPrice,
    decimal PositionValue,
    decimal GrossPnL,
    decimal NetPnL,
    decimal ReturnPercent,
    CloseReason CloseReason)
{
    /// <summary>True when the trade ended with a positive net PnL.</summary>
    public bool IsWinner => NetPnL > 0;

    /// <summary>Duration the position was held.</summary>
    public TimeSpan HoldingPeriod => ExitTime - EntryTime;
}

/// <summary>
/// A single point on the portfolio equity curve sampled after each trade event.
/// </summary>
/// <param name="Timestamp">UTC time of the equity observation.</param>
/// <param name="Equity">Portfolio equity in fiat at this point.</param>
/// <param name="DrawdownPercent">Peak-to-trough drawdown as a percentage at this point.</param>
public sealed record EquityCurvePoint(
    DateTime Timestamp,
    decimal Equity,
    decimal DrawdownPercent);

/// <summary>
/// Summary statistics for one path in the Monte Carlo simulation.
/// </summary>
/// <param name="FinalEquity">Portfolio equity at the end of the simulated path.</param>
/// <param name="MaxDrawdownPercent">Maximum peak-to-trough drawdown observed along the path.</param>
/// <param name="TotalReturnPercent">Cumulative return across all simulated trades on this path.</param>
/// <param name="SharpeRatio">Trade-normalised Sharpe ratio for this path.</param>
public sealed record MonteCarloPathSummary(
    decimal FinalEquity,
    decimal MaxDrawdownPercent,
    decimal TotalReturnPercent,
    decimal SharpeRatio);

/// <summary>
/// Aggregated statistics produced by the Monte Carlo simulation over many paths.
/// </summary>
public sealed record MonteCarloSimulationResult
{
    /// <summary>Total number of simulation paths generated.</summary>
    public required int Iterations { get; init; }

    /// <summary>Median final equity across all paths.</summary>
    public required decimal MedianFinalEquity { get; init; }

    /// <summary>Mean (expected) final equity across all paths.</summary>
    public required decimal MeanFinalEquity { get; init; }

    /// <summary>
    /// Final equity at the lower tail of the confidence interval
    /// (e.g. 2.5th percentile for a 95 % two-sided interval).
    /// </summary>
    public required decimal LowerConfidenceBound { get; init; }

    /// <summary>
    /// Final equity at the upper tail of the confidence interval
    /// (e.g. 97.5th percentile for a 95 % two-sided interval).
    /// </summary>
    public required decimal UpperConfidenceBound { get; init; }

    /// <summary>
    /// Value-at-Risk at the configured confidence level: the maximum expected
    /// loss such that worse outcomes occur only with probability (1 − CL).
    /// </summary>
    public required decimal ValueAtRisk { get; init; }

    /// <summary>
    /// Conditional Value-at-Risk (Expected Shortfall): the average loss
    /// across the tail of paths that breach the VaR threshold.
    /// </summary>
    public required decimal ConditionalValueAtRisk { get; init; }

    /// <summary>
    /// Empirical probability that the strategy finishes with equity above
    /// the starting value across all simulation paths.
    /// </summary>
    public required decimal ProbabilityOfProfit { get; init; }

    /// <summary>Median maximum drawdown percentage across all paths.</summary>
    public required decimal MedianMaxDrawdownPercent { get; init; }

    /// <summary>Per-path summary statistics ordered by final equity ascending.</summary>
    public required IReadOnlyList<MonteCarloPathSummary> Paths { get; init; }
}

/// <summary>
/// Complete result produced by a single backtest run over historical P2P price data.
/// </summary>
public sealed record BacktestResult
{
    /// <summary>Crypto asset that was simulated.</summary>
    public required string Asset { get; init; }

    /// <summary>Fiat currency denomination.</summary>
    public required string Fiat { get; init; }

    /// <summary>UTC start of the historical data window used.</summary>
    public required DateTime PeriodStart { get; init; }

    /// <summary>UTC end of the historical data window used.</summary>
    public required DateTime PeriodEnd { get; init; }

    /// <summary>Initial portfolio equity at the start of the simulation.</summary>
    public required decimal InitialEquity { get; init; }

    /// <summary>Portfolio equity at the end of the simulation.</summary>
    public required decimal FinalEquity { get; init; }

    /// <summary>Total return as a percentage of initial equity.</summary>
    public required decimal TotalReturnPercent { get; init; }

    /// <summary>
    /// Return annualised to a 365-day basis via geometric compounding,
    /// enabling cross-period strategy comparisons.
    /// </summary>
    public required decimal AnnualisedReturnPercent { get; init; }

    /// <summary>Total number of completed round-trip trades.</summary>
    public required int TotalTrades { get; init; }

    /// <summary>Fraction of trades that generated a positive net PnL.</summary>
    public required decimal WinRate { get; init; }

    /// <summary>Mean net PnL of winning trades in fiat units.</summary>
    public required decimal AverageWin { get; init; }

    /// <summary>Mean absolute net PnL of losing trades in fiat units.</summary>
    public required decimal AverageLoss { get; init; }

    /// <summary>
    /// Ratio of total gross profits to total gross losses.
    /// Values above 1 indicate the strategy extracts more than it gives back.
    /// </summary>
    public required decimal ProfitFactor { get; init; }

    /// <summary>
    /// Trade-normalised Sharpe ratio (mean return ÷ return standard deviation × √N).
    /// </summary>
    public required decimal SharpeRatio { get; init; }

    /// <summary>
    /// Sortino ratio: like Sharpe but penalises only downside deviation,
    /// making it more sensitive to harmful volatility.
    /// </summary>
    public required decimal SortinoRatio { get; init; }

    /// <summary>Maximum peak-to-trough equity drawdown as a percentage.</summary>
    public required decimal MaxDrawdownPercent { get; init; }

    /// <summary>
    /// Calmar ratio: annualised return divided by maximum drawdown.
    /// Higher values indicate better risk-adjusted performance.
    /// </summary>
    public required decimal CalmarRatio { get; init; }

    /// <summary>All completed trades in chronological order.</summary>
    public required IReadOnlyList<BacktestTrade> Trades { get; init; }

    /// <summary>Equity curve sampled at each trade event and at the initial bar.</summary>
    public required IReadOnlyList<EquityCurvePoint> EquityCurve { get; init; }

    /// <summary>
    /// Monte Carlo simulation output. <see langword="null"/> when
    /// <see cref="IBacktestingService.RunBacktestAsync"/> was used instead of
    /// <see cref="IBacktestingService.RunBacktestWithMonteCarloAsync"/>.
    /// </summary>
    public MonteCarloSimulationResult? MonteCarloResult { get; init; }

    /// <summary>UTC timestamp when this backtest result was computed.</summary>
    public required DateTime CalculatedAt { get; init; }

    /// <summary>Calendar days covered by the backtest window.</summary>
    public double PeriodDays => (PeriodEnd - PeriodStart).TotalDays;
}

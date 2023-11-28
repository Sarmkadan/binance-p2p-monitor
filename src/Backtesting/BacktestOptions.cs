// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Backtesting;

/// <summary>
/// Configuration options governing both the strategy backtest and the
/// Monte Carlo simulation that optionally follows it.
/// </summary>
public sealed class BacktestOptions
{
    /// <summary>
    /// Initial portfolio equity in fiat currency units (e.g. USD).
    /// </summary>
    public decimal InitialEquity { get; set; } = 10_000m;

    /// <summary>
    /// Fraction of current equity allocated to each new position (0 exclusive – 1 inclusive).
    /// A value of 0.10 allocates 10 % of available equity per trade.
    /// </summary>
    public decimal PositionSizeFraction { get; set; } = 0.10m;

    /// <summary>
    /// Minimum bid-ask spread percentage required to trigger a long entry.
    /// Higher values filter for only the most profitable spread regimes.
    /// </summary>
    public decimal EntrySpreadThreshold { get; set; } = 1.0m;

    /// <summary>
    /// Spread percentage below which an open position is closed via a normal exit.
    /// </summary>
    public decimal ExitSpreadThreshold { get; set; } = 0.3m;

    /// <summary>
    /// Adverse price movement percentage (relative to entry mid-price) that triggers
    /// a stop-loss closure to cap downside risk.
    /// </summary>
    public decimal StopLossPercent { get; set; } = 2.0m;

    /// <summary>
    /// Favourable price movement percentage (relative to entry mid-price) at which the
    /// position is closed to lock in gains.
    /// </summary>
    public decimal TakeProfitPercent { get; set; } = 4.0m;

    /// <summary>
    /// Round-trip transaction cost expressed as a percentage of position value.
    /// Applied symmetrically on entry and exit to model exchange fees and slippage.
    /// </summary>
    public decimal TransactionCostPercent { get; set; } = 0.2m;

    /// <summary>
    /// Maximum number of simultaneously open positions.
    /// Prevents over-concentration when multiple entry signals fire at once.
    /// </summary>
    public int MaxConcurrentPositions { get; set; } = 3;

    /// <summary>
    /// Number of historical bars used to compute the spread moving average that
    /// confirms momentum before an entry is taken.
    /// </summary>
    public int LookbackPeriod { get; set; } = 50;

    // ── Monte Carlo ──────────────────────────────────────────────────────────

    /// <summary>
    /// Number of Monte Carlo simulation paths to generate.
    /// More iterations yield tighter confidence intervals at the cost of CPU time.
    /// </summary>
    public int MonteCarloIterations { get; set; } = 1_000;

    /// <summary>
    /// Confidence level for VaR and interval calculations, expressed as a probability
    /// in the range (0, 1). A value of 0.95 gives 95 % confidence bounds.
    /// </summary>
    public decimal ConfidenceLevel { get; set; } = 0.95m;

    /// <summary>
    /// Optional fixed seed for the Monte Carlo random number generator.
    /// When set, simulation runs are fully reproducible across calls.
    /// </summary>
    public int? RandomSeed { get; set; }

    /// <summary>
    /// Multiplicative scaling factor applied to the empirical return standard deviation
    /// when injecting Gaussian perturbation into bootstrapped returns.
    /// Values above 1.0 stress-test the strategy under amplified volatility.
    /// </summary>
    public decimal VolatilityScaleFactor { get; set; } = 1.0m;

    /// <summary>
    /// Returns a new <see cref="BacktestOptions"/> instance populated with
    /// production-ready defaults suitable for most P2P spread strategies.
    /// </summary>
    public static BacktestOptions Default => new();

    /// <summary>
    /// Validates all option values and throws <see cref="ConfigurationException"/>
    /// if any constraint is violated.
    /// </summary>
    public void Validate()
    {
        var errors = new List<string>();

        if (InitialEquity <= 0)
            errors.Add("InitialEquity must be positive");
        if (PositionSizeFraction is <= 0 or > 1)
            errors.Add("PositionSizeFraction must be in (0, 1]");
        if (EntrySpreadThreshold < 0)
            errors.Add("EntrySpreadThreshold cannot be negative");
        if (ExitSpreadThreshold < 0)
            errors.Add("ExitSpreadThreshold cannot be negative");
        if (StopLossPercent < 0)
            errors.Add("StopLossPercent cannot be negative");
        if (TakeProfitPercent < 0)
            errors.Add("TakeProfitPercent cannot be negative");
        if (TransactionCostPercent < 0)
            errors.Add("TransactionCostPercent cannot be negative");
        if (MaxConcurrentPositions < 1)
            errors.Add("MaxConcurrentPositions must be at least 1");
        if (LookbackPeriod < 2)
            errors.Add("LookbackPeriod must be at least 2");
        if (MonteCarloIterations < 10)
            errors.Add("MonteCarloIterations must be at least 10");
        if (ConfidenceLevel is <= 0 or >= 1)
            errors.Add("ConfidenceLevel must be in (0, 1)");
        if (VolatilityScaleFactor <= 0)
            errors.Add("VolatilityScaleFactor must be positive");

        if (errors.Count > 0)
            throw new ConfigurationException(
                $"BacktestOptions validation failed: {string.Join("; ", errors)}");
    }
}

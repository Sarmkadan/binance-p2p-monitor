namespace BinanceP2pMonitor.Backtesting;

/// <summary>
/// Provides extension methods for <see cref="BacktestOptions"/>.
/// </summary>
public static class BacktestOptionsExtensions
{
    /// <summary>
    /// Calculates the maximum allowed position size based on the initial equity and position size fraction.
    /// </summary>
    /// <param name="options">The backtest options.</param>
    /// <returns>The maximum allowed position size.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static decimal CalculateMaxPositionSize(this BacktestOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.InitialEquity * options.PositionSizeFraction;
    }

    /// <summary>
    /// Determines if the stop loss should be triggered based on the current spread.
    /// </summary>
    /// <param name="options">The backtest options.</param>
    /// <param name="currentSpread">The current spread.</param>
    /// <returns><c>true</c> if the stop loss should be triggered; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static bool ShouldTriggerStopLoss(this BacktestOptions options, decimal currentSpread)
    {
        ArgumentNullException.ThrowIfNull(options);

        return currentSpread <= options.EntrySpreadThreshold - options.StopLossPercent;
    }

    /// <summary>
    /// Determines if the take profit should be triggered based on the current spread.
    /// </summary>
    /// <param name="options">The backtest options.</param>
    /// <param name="currentSpread">The current spread.</param>
    /// <returns><c>true</c> if the take profit should be triggered; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static bool ShouldTriggerTakeProfit(this BacktestOptions options, decimal currentSpread)
    {
        ArgumentNullException.ThrowIfNull(options);

        return currentSpread >= options.ExitSpreadThreshold + options.TakeProfitPercent;
    }

    /// <summary>
    /// Determines if either the stop loss or take profit should be triggered based on the current spread and thresholds.
    /// </summary>
    /// <param name="options">The backtest options.</param>
    /// <param name="currentSpread">The current spread.</param>
    /// <returns><c>true</c> if either the stop loss or take profit should be triggered; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static bool ShouldTriggerStopLossOrTakeProfit(this BacktestOptions options, decimal currentSpread)
    {
        return options.ShouldTriggerStopLoss(currentSpread) || options.ShouldTriggerTakeProfit(currentSpread);
    }

    /// <summary>
    /// Calculates the transaction cost based on the transaction cost percent and position size.
    /// </summary>
    /// <param name="options">The backtest options.</param>
    /// <param name="positionSize">The position size.</param>
    /// <returns>The transaction cost.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="positionSize"/> is negative.</exception>
    public static decimal CalculateTransactionCost(this BacktestOptions options, decimal positionSize)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(positionSize);

        return positionSize * options.TransactionCostPercent / 100;
    }
}

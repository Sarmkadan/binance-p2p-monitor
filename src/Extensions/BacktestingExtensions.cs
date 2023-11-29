#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Backtesting;
using Microsoft.Extensions.DependencyInjection;

namespace BinanceP2pMonitor.Extensions;

/// <summary>
/// Extension methods for registering the strategy backtesting and Monte Carlo
/// simulation infrastructure with the application's DI container.
/// </summary>
public static class BacktestingExtensions
{
    /// <summary>
    /// Registers <see cref="IBacktestingService"/> (implemented by <see cref="BacktestingEngine"/>)
    /// as a scoped service, along with the default <see cref="BacktestOptions"/> singleton.
    /// </summary>
    /// <remarks>
    /// Prerequisites — the following services must already be registered before calling this method:
    /// <list type="bullet">
    ///   <item><see cref="IHistoryRepository"/></item>
    ///   <item><see cref="IHistoricalSpreadAnalysisService"/></item>
    ///   <item><see cref="AppSettings"/></item>
    ///   <item><c>ILogger&lt;BacktestingEngine&gt;</c> (provided automatically by
    ///     <c>services.AddLogging()</c>)</item>
    /// </list>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    public static IServiceCollection AddBacktesting(this IServiceCollection services)
    {
        services.AddSingleton(BacktestOptions.Default);
        services.AddScoped<IBacktestingService, BacktestingEngine>();
        return services;
    }

    /// <summary>
    /// Registers the backtesting infrastructure with a custom
    /// <see cref="BacktestOptions"/> configuration action.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">
    /// Delegate that receives the default <see cref="BacktestOptions"/> instance
    /// and applies caller-specific overrides before it is validated and registered.
    /// </param>
    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
    /// <exception cref="ConfigurationException">
    /// Thrown during startup if the configured options fail validation.
    /// </exception>
    public static IServiceCollection AddBacktesting(
        this IServiceCollection services,
        Action<BacktestOptions> configure)
    {
        var options = BacktestOptions.Default;
        configure(options);
        options.Validate();

        services.AddSingleton(options);
        services.AddScoped<IBacktestingService, BacktestingEngine>();
        return services;
    }

    /// <summary>
    /// Formats a <see cref="BacktestResult"/> as a multi-line human-readable summary
    /// suitable for console output or structured logging.
    /// </summary>
    /// <param name="result">The backtest result to summarise.</param>
    /// <returns>A formatted string representation of the key performance metrics.</returns>
    public static string ToSummaryString(this BacktestResult result)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"┌─ Backtest: {result.Asset}/{result.Fiat} ──────────────────────────────────");
        sb.AppendLine($"│  Period        : {result.PeriodStart:yyyy-MM-dd} → {result.PeriodEnd:yyyy-MM-dd} ({result.PeriodDays:F0}d)");
        sb.AppendLine($"│  Initial EQ    : {result.InitialEquity:F2}");
        sb.AppendLine($"│  Final EQ      : {result.FinalEquity:F2}");
        sb.AppendLine($"│  Total Return  : {result.TotalReturnPercent:+0.00;-0.00}%");
        sb.AppendLine($"│  Ann. Return   : {result.AnnualisedReturnPercent:+0.00;-0.00}%");
        sb.AppendLine($"├─ Trades ──────────────────────────────────────────────────────────────");
        sb.AppendLine($"│  Count         : {result.TotalTrades}");
        sb.AppendLine($"│  Win Rate      : {result.WinRate:P1}");
        sb.AppendLine($"│  Avg Win       : {result.AverageWin:F4}");
        sb.AppendLine($"│  Avg Loss      : {result.AverageLoss:F4}");
        sb.AppendLine($"│  Profit Factor : {result.ProfitFactor:F4}");
        sb.AppendLine($"├─ Risk ─────────────────────────────────────────────────────────────────");
        sb.AppendLine($"│  Max Drawdown  : {result.MaxDrawdownPercent:F4}%");
        sb.AppendLine($"│  Sharpe Ratio  : {result.SharpeRatio:F4}");
        sb.AppendLine($"│  Sortino Ratio : {result.SortinoRatio:F4}");
        sb.AppendLine($"│  Calmar Ratio  : {result.CalmarRatio:F4}");

        if (result.MonteCarloResult is { } mc)
        {
            sb.AppendLine($"├─ Monte Carlo ({mc.Iterations:N0} paths) ─────────────────────────────────────");
            sb.AppendLine($"│  Median EQ     : {mc.MedianFinalEquity:F2}");
            sb.AppendLine($"│  Mean EQ       : {mc.MeanFinalEquity:F2}");
            sb.AppendLine($"│  CI [{result.InitialEquity:F0}]   : [{mc.LowerConfidenceBound:F2}, {mc.UpperConfidenceBound:F2}]");
            sb.AppendLine($"│  VaR           : {mc.ValueAtRisk:F2}");
            sb.AppendLine($"│  CVaR (ES)     : {mc.ConditionalValueAtRisk:F2}");
            sb.AppendLine($"│  P(profit)     : {mc.ProbabilityOfProfit:P1}");
            sb.AppendLine($"│  Median MDD    : {mc.MedianMaxDrawdownPercent:F4}%");
        }

        sb.Append("└──────────────────────────────────────────────────────────────────────");
        return sb.ToString();
    }
}

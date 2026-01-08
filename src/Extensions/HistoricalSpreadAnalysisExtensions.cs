// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BinanceP2pMonitor.Extensions;

/// <summary>
/// Extension methods for registering historical spread analysis with the DI container
/// </summary>
public static class HistoricalSpreadAnalysisExtensions
{
    /// <summary>
    /// Registers <see cref="IHistoricalSpreadAnalysisService"/> as a scoped service.
    /// Requires <see cref="IHistoryRepository"/>, <see cref="ISpreadAnalysisService"/>,
    /// <see cref="IEventBus"/>, and <see cref="AppSettings"/> to be registered beforehand.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The same <paramref name="services"/> instance for chaining</returns>
    public static IServiceCollection AddHistoricalSpreadAnalysis(this IServiceCollection services)
    {
        services.AddScoped<IHistoricalSpreadAnalysisService, HistoricalSpreadAnalysisService>();
        return services;
    }
}

#nullable enable
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Benchmarks;

/// <summary>
/// A benchmark class for testing string extension methods.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class StringExtensionsBenchmarks
{
    private const string PascalInput = "BinancePriceMonitoringService";
    private const string SnakeInput = "binance_price_monitoring_service";
    private const string LongText = "This is a longer string that may need to be truncated for display purposes in the console output";
    private const string DecimalString = "42345.6789";
    private const string IntString = "98765";
    private const string InvalidNumeric = "not-a-number";

    /// <summary>
    /// Tests the <see cref="StringExtensions.SplitCamelCase"/> method with a cached regex.
    /// </summary>
    /// <returns>The input string split into camel case.</returns>
    [Benchmark(Description = "SplitCamelCase (cached regex)")]
    public string SplitCamelCase()
        => PascalInput.SplitCamelCase();

    /// <summary>
    /// Tests the <see cref="StringExtensions.ToSnakeCase"/> method with a cached regex.
    /// </summary>
    /// <returns>The input string converted to snake case.</returns>
    [Benchmark(Description = "ToSnakeCase (cached regex)")]
    public string ToSnakeCase()
        => PascalInput.ToSnakeCase();

    /// <summary>
    /// Tests the <see cref="StringExtensions.ToPascalCase"/> method using a split and concat approach.
    /// </summary>
    /// <returns>The input string converted to pascal case.</returns>
    [Benchmark(Description = "ToPascalCase (split + concat)")]
    public string ToPascalCase()
        => SnakeInput.ToPascalCase();

    /// <summary>
    /// Tests the <see cref="StringExtensions.Truncate"/> method with a string that needs to be truncated.
    /// </summary>
    /// <returns>The input string truncated to 30 characters.</returns>
    [Benchmark(Description = "Truncate (triggers truncation)")]
    public string Truncate_Triggered()
        => LongText.Truncate(30);

    /// <summary>
    /// Tests the <see cref="StringExtensions.Truncate"/> method with a string that does not need to be truncated.
    /// </summary>
    /// <returns>The input string, unchanged.</returns>
    [Benchmark(Description = "Truncate (no truncation needed)")]
    public string Truncate_NoOp()
        => LongText.Truncate(200);

    /// <summary>
    /// Tests the <see cref="StringExtensions.ToDecimalOrNull"/> method with a valid decimal string.
    /// </summary>
    /// <returns>The decimal value of the input string, or null if invalid.</returns>
    [Benchmark(Description = "ToDecimalOrNull (valid, span overload)")]
    public decimal? ToDecimalOrNull_Valid()
        => DecimalString.ToDecimalOrNull();

    /// <summary>
    /// Tests the <see cref="StringExtensions.ToDecimalOrNull"/> method with an invalid decimal string.
    /// </summary>
    /// <returns>null, as the input string is not a valid decimal.</returns>
    [Benchmark(Description = "ToDecimalOrNull (invalid)")]
    public decimal? ToDecimalOrNull_Invalid()
        => InvalidNumeric.ToDecimalOrNull();

    /// <summary>
    /// Tests the <see cref="StringExtensions.ToIntOrNull"/> method with a valid integer string.
    /// </summary>
    /// <returns>The integer value of the input string, or null if invalid.</returns>
    [Benchmark(Description = "ToIntOrNull (valid, span overload)")]
    public int? ToIntOrNull_Valid()
        => IntString.ToIntOrNull();

    /// <summary>
    /// Tests the <see cref="StringExtensions.Mask"/> method with an API key style mask.
    /// </summary>
    /// <returns>The input string masked with 4 characters.</returns>
    [Benchmark(Description = "Mask (API key style)")]
    public string Mask()
        => "sk-live-abcdefghijklmnopqrstuvwxyz".Mask(4);
}

#nullable enable
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Benchmarks;

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

    [Benchmark(Description = "SplitCamelCase (cached regex)")]
    public string SplitCamelCase()
        => PascalInput.SplitCamelCase();

    [Benchmark(Description = "ToSnakeCase (cached regex)")]
    public string ToSnakeCase()
        => PascalInput.ToSnakeCase();

    [Benchmark(Description = "ToPascalCase (split + concat)")]
    public string ToPascalCase()
        => SnakeInput.ToPascalCase();

    [Benchmark(Description = "Truncate (triggers truncation)")]
    public string Truncate_Triggered()
        => LongText.Truncate(30);

    [Benchmark(Description = "Truncate (no truncation needed)")]
    public string Truncate_NoOp()
        => LongText.Truncate(200);

    [Benchmark(Description = "ToDecimalOrNull (valid, span overload)")]
    public decimal? ToDecimalOrNull_Valid()
        => DecimalString.ToDecimalOrNull();

    [Benchmark(Description = "ToDecimalOrNull (invalid)")]
    public decimal? ToDecimalOrNull_Invalid()
        => InvalidNumeric.ToDecimalOrNull();

    [Benchmark(Description = "ToIntOrNull (valid, span overload)")]
    public int? ToIntOrNull_Valid()
        => IntString.ToIntOrNull();

    [Benchmark(Description = "Mask (API key style)")]
    public string Mask()
        => "sk-live-abcdefghijklmnopqrstuvwxyz".Mask(4);
}

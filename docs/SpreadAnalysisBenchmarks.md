# SpreadAnalysisBenchmarks

The `SpreadAnalysisBenchmarks` class provides a set of micro-benchmark methods for analyzing price spreads in a Binance P2P monitoring context. It is designed to measure the performance of different spread calculation, statistical aggregation, and anomaly detection algorithms. The class is intended to be used with a benchmarking framework (e.g., BenchmarkDotNet) where the `Setup` method initializes internal data, and each analysis method is invoked repeatedly to capture timing metrics.

## API

### `public void Setup()`

Initializes the internal data structures required for the subsequent analysis methods. This method must be called before any of the analysis methods are invoked. It typically populates a collection of spread values (e.g., from a pre-loaded dataset or generated test data).  
**Parameters:** None.  
**Returns:** Nothing.  
**Throws:** May throw if external data sources are unavailable or if memory allocation fails.

### `public decimal AnalyzeSpread_Direct()`

Computes the spread directly from the internal data set up by `Setup`. The exact definition of "spread" depends on the context (e.g., difference between best bid and best ask).  
**Parameters:** None.  
**Returns:** A `decimal` representing the computed spread value.  
**Throws:** `InvalidOperationException` if `Setup` has not been called.

### `public (decimal mean, decimal stdDev) ComputeStatistics_Loop()`

Calculates the mean and standard deviation of the spread values using a loop-based approach.  
**Parameters:** None.  
**Returns:** A tuple `(decimal mean, decimal stdDev)` where `mean` is the arithmetic mean and `stdDev` is the population standard deviation of the spread data.  
**Throws:** `InvalidOperationException` if `Setup` has not been called; `DivideByZeroException` if the data set is empty.

### `public int FindAnomalies_ZScore()`

Identifies anomalies in the spread data using a z-score threshold (commonly 2 or 3). The method counts how many data points fall outside the threshold relative to the mean and standard deviation computed from the same data.  
**Parameters:** None.  
**Returns:** An `int` representing the number of anomalous spread values detected.  
**Throws:** `InvalidOperationException` if `Setup` has not been called; `InvalidOperationException` if the data set has fewer than two elements (cannot compute standard deviation).

### `public int FindAnomalies_ZScore_ArrayPool()`

Performs the same anomaly detection as `FindAnomalies_ZScore` but uses `System.Buffers.ArrayPool<T>` to reduce memory allocation overhead during the computation.  
**Parameters:** None.  
**Returns:** An `int` representing the number of anomalous spread values detected.  
**Throws:** Same as `FindAnomalies_ZScore`.

## Usage

### Example 1: Basic Benchmark with BenchmarkDotNet

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class SpreadBenchmark
{
    private SpreadAnalysisBenchmarks _benchmarks;

    [GlobalSetup]
    public void Setup()
    {
        _benchmarks = new SpreadAnalysisBenchmarks();
        _benchmarks.Setup();
    }

    [Benchmark]
    public decimal DirectSpread() => _benchmarks.AnalyzeSpread_Direct();

    [Benchmark]
    public (decimal mean, decimal stdDev) StatisticsLoop() => _benchmarks.ComputeStatistics_Loop();

    [Benchmark]
    public int AnomaliesZScore() => _benchmarks.FindAnomalies_ZScore();

    [Benchmark]
    public int AnomaliesZScorePooled() => _benchmarks.FindAnomalies_ZScore_ArrayPool();
}

public class Program
{
    public static void Main() => BenchmarkRunner.Run<SpreadBenchmark>();
}
```

### Example 2: Manual Invocation for Verification

```csharp
var analyzer = new SpreadAnalysisBenchmarks();
analyzer.Setup();

decimal spread = analyzer.AnalyzeSpread_Direct();
Console.WriteLine($"Direct spread: {spread}");

var stats = analyzer.ComputeStatistics_Loop();
Console.WriteLine($"Mean: {stats.mean}, StdDev: {stats.stdDev}");

int anomalies = analyzer.FindAnomalies_ZScore();
Console.WriteLine($"Anomalies (standard): {anomalies}");

int anomaliesPooled = analyzer.FindAnomalies_ZScore_ArrayPool();
Console.WriteLine($"Anomalies (pooled): {anomaliesPooled}");
```

## Notes

- **Edge Cases:**  
  - If `Setup` is not called before any analysis method, an `InvalidOperationException` is thrown.  
  - An empty data set (no spread values) will cause `ComputeStatistics_Loop` to throw a `DivideByZeroException`.  
  - A data set with fewer than two elements cannot produce a meaningful standard deviation; `FindAnomalies_ZScore` and `FindAnomalies_ZScore_ArrayPool` will throw an `InvalidOperationException` in this case.  
  - If all spread values are identical, the standard deviation is zero, and the z-score anomaly detection will treat every point as non-anomalous (since no point deviates from the mean). The methods will still return zero anomalies.

- **Thread Safety:**  
  Instances of `SpreadAnalysisBenchmarks` are **not thread-safe**. The `Setup` method modifies internal state, and concurrent calls to any analysis methods from multiple threads may produce undefined behavior or corrupt data. For benchmarking purposes, each thread should use its own instance, or synchronization should be applied externally. The `ArrayPool` variant (`FindAnomalies_ZScore_ArrayPool`) rents and returns buffers from a shared pool, which is thread-safe by design, but the instance’s own state is not.

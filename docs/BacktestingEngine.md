# BacktestingEngine
The `BacktestingEngine` class is designed to facilitate the backtesting of trading strategies, allowing users to evaluate the performance of their strategies using historical data. This class provides methods for running backtests, generating trade signals, and performing Monte Carlo simulations, enabling users to refine their strategies and make informed decisions.

## API
### Constructors
* `public BacktestingEngine`: Initializes a new instance of the `BacktestingEngine` class.

### Methods
* `public async Task<BacktestResult> RunBacktestAsync`: Runs a backtest using the provided configuration. Returns a `BacktestResult` object containing the results of the backtest. May throw exceptions if the configuration is invalid or if an error occurs during the backtest.
* `public async Task<BacktestResult> RunBacktestWithMonteCarloAsync`: Runs a backtest with a Monte Carlo simulation using the provided configuration. Returns a `BacktestResult` object containing the results of the backtest. May throw exceptions if the configuration is invalid or if an error occurs during the backtest.
* `public async Task<IReadOnlyList<TradeSignal>> GenerateSignalsAsync`: Generates a list of trade signals based on the provided configuration. Returns an `IReadOnlyList<TradeSignal>` containing the generated trade signals. May throw exceptions if the configuration is invalid or if an error occurs during signal generation.

## Usage
```csharp
// Example 1: Running a simple backtest
var engine = new BacktestingEngine();
var result = await engine.RunBacktestAsync();
Console.WriteLine($"Backtest result: {result}");

// Example 2: Running a backtest with Monte Carlo simulation
var engine = new BacktestingEngine();
var result = await engine.RunBacktestWithMonteCarloAsync();
Console.WriteLine($"Backtest result with Monte Carlo: {result}");
```

## Notes
The `BacktestingEngine` class is designed to be used in a single-threaded or multi-threaded environment, but it is not thread-safe by default. Users should ensure that the class is properly synchronized if used in a multi-threaded context. Additionally, the `RunBacktestAsync` and `RunBacktestWithMonteCarloAsync` methods may throw exceptions if the configuration is invalid or if an error occurs during the backtest. It is recommended to handle these exceptions properly to ensure robust error handling. The `GenerateSignalsAsync` method may also throw exceptions if the configuration is invalid or if an error occurs during signal generation. Users should be aware of these potential edge cases and handle them accordingly.

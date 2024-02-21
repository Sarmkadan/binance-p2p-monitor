# BacktestCommand

The `BacktestCommand` class provides functionality to execute and validate backtesting operations for simulated trading strategies against historical Binance P2P market data. It encapsulates the logic for argument validation, help text generation, and asynchronous execution of backtests, enabling reproducible testing of trading algorithms without live market exposure.

## API

### `public BacktestCommand`

The default constructor initializes a new instance of the `BacktestCommand` class with default configuration. No parameters are required, as all necessary state is either derived from validated arguments or defaults to safe values.

### `public string GetHelp()`

Returns a human-readable help string describing the command's purpose, expected arguments, and usage examples. The help text includes:
- A brief description of the backtest command.
- The required and optional arguments with their formats.
- Example invocations.
- Notes on error handling and expected outputs.

**Returns:** A non-null string containing formatted help text.

**Throws:** Never throws exceptions; always returns a valid string.

### `public List<string> ValidateArguments(string[] args)`

Validates the provided command-line arguments against the expected schema for backtesting. Ensures required parameters are present, optional parameters are correctly formatted, and conflicting options are not combined.

**Parameters:**
- `args`: An array of command-line argument strings. Must not be null.

**Returns:** A list of validation error messages. An empty list indicates successful validation.

**Throws:**
- `ArgumentNullException`: If `args` is null.

### `public async Task<int> ExecuteAsync()`

Asynchronously executes the backtest using previously validated arguments. Loads historical market data, applies the specified trading strategy, simulates order execution, and computes performance metrics. The operation is non-blocking and returns a task representing the asynchronous process.

**Returns:** A task that completes with an integer exit code:
- `0`: Success.
- Non-zero: Failure (specific code indicates type of failure).

**Throws:**
- `InvalidOperationException`: If called before `ValidateArguments` or if arguments are invalid.
- `OperationCanceledException`: If the operation is canceled via linked cancellation token.
- `IOException`: If market data files cannot be read.
- `JsonException`: If strategy configuration is malformed.

## Usage

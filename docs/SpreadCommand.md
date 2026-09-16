# SpreadCommand

Command to display current buy/sell spread for trading pairs.

## Overview

The `SpreadCommand` displays the current buy/sell spread for trading pairs. It supports filtering by asset, fiat currency, or a specific pair, and can render results in table, JSON, or markdown format. It also reports the risk level of each spread and how far the current spread deviates from its historical average.

## Usage

```bash
binance-p2p-monitor spread [options]
```

### Options

| Option | Description | Required |
|--------|-------------|----------|
| `--asset=ASSET` | Show spread for a specific asset only (e.g., BTC) | No |
| `--fiat=FIAT` | Show spread for a specific fiat currency only (e.g., USD) | No |
| `--format=FORMAT` | Output format: table, json, markdown (default: table) | No |
| `--pair=PAIR` | Show spread for a specific pair (format: ASSET/FIAT) | No |
| `-h, --help` | Show this help message | No |

### Examples

```bash
binance-p2p-monitor spread
binance-p2p-monitor spread --asset=BTC
binance-p2p-monitor spread --fiat=USD
binance-p2p-monitor spread --pair=BTC/USD
binance-p2p-monitor spread --format=json
```

## Implementation Details

### Constructor Dependencies

- `ISpreadAnalysisService _spreadAnalysisService` - Service for retrieving spread analysis data
- `ConsoleOutputWriter _output` - Handles console output formatting
- `IEnumerable<IOutputFormatter> _formatters` - Collection of output formatters
- `ILogger<SpreadCommand> _logger` - Logger for error tracking
- `AppSettings _appSettings` - Application configuration settings

### Methods

#### GetHelp()
Returns the help text showing usage instructions, options, and examples.

#### ValidateArguments(CommandContext context)
Validates command arguments:
- Checks that `--format` is one of: table, json, markdown (case-insensitive)
- Returns a list of validation error messages

#### ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
Main execution method:
1. Writes header "Buy/Sell Spread Analysis"
2. Reads `--asset`, `--fiat`, `--pair`, and `--format` options from context (format defaults to `table`)
3. Resolves the output formatter by matching `--format` against the registered formatters' `FormatType` (case-insensitive). Returns exit code 1 if no formatter matches
4. If `--pair` is provided:
   - Splits the value on `/` or `\` into exactly two parts (asset and fiat)
   - Fetches the single spread via `_spreadAnalysisService.GetSpreadAnalysisAsync(pairAsset, pairFiat)`
   - If no data is available, writes a warning and returns 0
   - If the pair is not in `ASSET/FIAT` form, writes an error and returns 1
5. Otherwise, fetches all spreads via `_spreadAnalysisService.GetAllSpreadsAsync()` and applies the optional `--asset` and `--fiat` filters (case-insensitive)
6. If no spreads remain, writes "No spread data available" and returns 0
7. Renders the collected spreads with the selected formatter
8. Writes a "Configuration" section showing `DefaultSpreadThreshold` and `SpreadAnalysisHistoryHours` from `AppSettings`
9. Returns 0 on success

#### CreateSpreadDisplay(Spread spread)
Creates an anonymous object containing:
- Asset and Fiat symbols, plus a combined `Pair` string (`ASSET/FIAT`)
- Current, average, minimum, and maximum spread percentages (formatted to 4 decimal places)
- Standard deviation (formatted to 4 decimal places)
- Sample count
- Risk level (from `Spread.GetRiskLevel()`)
- Variance from average (formatted with explicit sign, e.g. `+0.00;-0.00;0`)
- `IsHigh` / `IsLow` flags (rendered with emoji indicators) based on `Spread.IsHighSpread()` and `Spread.IsLowSpread()`
- Last updated timestamp rendered as a relative time string via `GetTimeAgoString()`

## Error Handling

- Returns exit code 1 if:
  - An unsupported `--format` is specified
  - The `--pair` value is not in `ASSET/FIAT` form
  - An exception occurs during execution (logged and displayed to the user)
- Returns exit code 0 (with a warning/info message) if:
  - No spread data is available for the requested pair
  - No spreads match the given filters

## Dependencies

This command depends on:
- `ISpreadAnalysisService` for retrieving spread analysis data
- Output formatters for different output formats (table, json, markdown)
- Console output writer for formatted console output
- Application settings for configuration display
- Logging service for error tracking

## Output Format

The command outputs a table (or JSON/markdown) with one row per spread, showing:
- Asset and fiat pair
- Current, average, minimum, and maximum spread percentages
- Standard deviation and sample count
- Risk level and variance from average
- Whether the spread is considered high or low
- Last updated time

A trailing "Configuration" section reports the configured default spread threshold and spread analysis history window.
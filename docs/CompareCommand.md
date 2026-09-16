# CompareCommand

Command to compare buy/sell prices for the same asset across two fiat currencies.

## Overview

The `CompareCommand` allows users to compare cryptocurrency prices between two different fiat currencies side by side, showing price differences, ratios, and analysis of the best buy/sell locations.

## Usage

```bash
binance-p2p-monitor compare [options]
```

### Options

| Option | Description | Required |
|--------|-------------|----------|
| `--asset=ASSET` | Asset to compare (e.g., BTC, ETH, USDT) | Yes |
| `--from=FIAT` | First fiat currency (required) | Yes |
| `--to=FIAT` | Second fiat currency (required) | Yes |
| `--format=FORMAT` | Output format: table, json, markdown (default: table) | No |
| `-h, --help` | Show this help message | No |

### Examples

```bash
binance-p2p-monitor compare --asset=BTC --from=USD --to=USDT
binance-p2p-monitor compare --asset=ETH --from=EUR --to=GBP
binance-p2p-monitor compare --asset=USDT --from=USD --to=CNY --format=json
```

## Implementation Details

### Constructor Dependencies

- `IPriceMonitoringService _priceService` - Service for retrieving price data
- `ConsoleOutputWriter _output` - Handles console output formatting
- `IEnumerable<IOutputFormatter> _formatters` - Collection of output formatters
- `ILogger<CompareCommand> _logger` - Logger for error tracking
- `AppSettings _appSettings` - Application configuration settings

### Methods

#### GetHelp()
Returns the help text showing usage instructions, options, and examples.

#### ValidateArguments(CommandContext context)
Validates command arguments:
- Checks that `--asset`, `--from`, and `--to` options are provided
- Validates that `--format` is one of: table, json, markdown
- Returns a list of validation error messages

#### ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
Main execution method:
1. Writes header "Price Comparison"
2. Retrieves asset, fromFiat, toFiat, and format from context
3. Finds appropriate formatter based on format option
4. Gets current prices for both currency pairs using `_priceService.GetCurrentPriceAsync`
5. Creates comparison display object with price data for both currencies
6. Outputs formatted comparison data
7. Shows price difference analysis including:
   - Price difference (absolute and percentage)
   - Buy and sell price ratios
   - Best buy and sell locations

#### CreateComparisonDisplay(string asset, Price priceFrom, Price priceTo)
Creates an anonymous object containing:
- Asset symbol
- From/To fiat currencies
- Buy/sell prices for both currency pairs (formatted to 8 decimal places)
- Spread percentages (formatted to 4 decimal places)
- Price change percentages
- Price differences (absolute and percentage)
- Last updated timestamp

#### CalculatePriceDifferencePercent(decimal price1, decimal price2)
Calculates percentage difference between two prices:
- Returns 0 if price2 is 0 (to avoid division by zero)
- Otherwise returns ((price1 - price2) / price2) * 100

## Error Handling

- Returns exit code 1 if:
  - Required arguments are missing
  - Unsupported format is specified
  - Price data cannot be retrieved for one or both currency pairs
  - An exception occurs during execution (logged and displayed to user)

## Dependencies

This command depends on:
- Price monitoring service for retrieving real-time price data
- Output formatters for different output formats (table, json, markdown)
- Console output writer for formatted console output
- Application settings for configuration
- Logging service for error tracking

## Output Format

The command outputs a comparison table showing:
- Asset being compared
- Buy/sell prices for both fiat currency pairs
- Spread percentages for both pairs
- Price change percentages
- Price differences (absolute and relative)
- Analysis section with price difference metrics and best location recommendations
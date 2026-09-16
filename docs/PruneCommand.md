# Prune Command

The `prune` command deletes historical price data older than a specified number of days from the database.

## Usage

```
binance-p2p-monitor prune [options]
```

## Options

| Option | Description |
|--------|-------------|
| `--days=DAYS` | Number of days to keep (records older than this will be deleted) |
| `-h, --help` | Show this help message |

## Examples

```bash
binance-p2p-monitor prune --days=30
binance-p2p-monitor prune --days=90
binance-p2p-monitor prune --help
```

## Behavior

1. Validates that the `--days` parameter is provided and is a positive integer
2. Displays the current total record count in the history table
3. Prompts for confirmation before proceeding with deletion
4. Deletes records older than the specified number of days
5. Reports the number of deleted records and remaining total count

## Implementation Details

The command relies on the `IDatabaseCleanupService` to:
- Get the total history count (`GetTotalHistoryCountAsync`)
- Delete old records (`DeleteOldRecordsAsync`)

The command requires explicit user confirmation ("yes") before performing any deletion operations to prevent accidental data loss.

## Error Handling

- Returns exit code 1 if validation fails or an exception occurs
- Returns exit code 0 on successful completion or when cancelled by user
- Logs any unexpected exceptions using the injected `ILogger<PruneCommand>`
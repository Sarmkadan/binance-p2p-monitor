# DatabaseCleanupWorker

Background worker responsible for cleaning up old records from the database to prevent unbounded growth.

## Overview

The `DatabaseCleanupWorker` is a long-running background service that periodically removes old historical records from the database based on configured retention policies. It runs as a .NET `BackgroundService` and executes cleanup operations every 6 hours.

## Key Features

- **Periodic Execution**: Runs cleanup every 6 hours with an initial 5-minute delay at startup
- **Configurable Retention**: Uses `HistoryRetentionDays` setting to determine what constitutes "old" records
- **Secondary Safeguard**: Enforces maximum record count via `MaxHistoryRecords` setting
- **Error Handling**: Gracefully handles exceptions with exponential backoff (10-minute delay on failure)
- **Cancellation Support**: Properly responds to cancellation tokens for graceful shutdown
- **Logging**: Comprehensive logging of startup, execution, completion, and error conditions

## Dependencies

- `IServiceProvider`: For service resolution (though not directly used in current implementation)
- `ILogger<DatabaseCleanupWorker>`: For structured logging
- `AppSettings`: Configuration containing retention policies
- `IDatabaseCleanupService`: Service that performs the actual database cleanup operations

## Configuration

The worker relies on two key settings from `AppSettings`:

- `HistoryRetentionDays`: Number of days of history to retain (records older than this are deleted)
- `MaxHistoryRecords`: Maximum allowed history records as a secondary safeguard

## Execution Flow

1. **Startup**: Waits 5 minutes before first execution to avoid interfering with application startup
2. **Loop**: While not cancelled:
   - Attempts to clean up old records
   - Waits 6 hours before next execution
   - On exception: waits 10 minutes before retrying
3. **Cleanup Operation**:
   - Logs start of cleanup with retention period
   - Deletes records older than `HistoryRetentionDays`
   - Gets remaining record count
   - Warns if count exceeds `MaxHistoryRecords`
   - Logs completion with deletion and remaining counts

## Error Handling

- `OperationCanceledException`: Treated as normal shutdown, logs info and exits loop
- General exceptions: Logged as error, followed by 10-minute delay before retry
- All errors maintain the worker's availability for future cleanup cycles

## Notes

- The worker does not directly access the database; all operations are delegated to `IDatabaseCleanupService`
- Initial delay helps prevent resource contention during application startup
- Secondary safeguard (`MaxHistoryRecords`) provides additional protection against unexpected data growth
- Worker is designed to run indefinitely until application shutdown
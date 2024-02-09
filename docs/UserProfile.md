# UserProfile

Represents a user account in the Binance P2P monitoring system, tracking user preferences, activity, and notification settings for price alert management.

## API

### Properties

- **`Id`** (int)
  Unique identifier for the user profile. Immutable after creation.

- **`TelegramChatId`** (long)
  Telegram chat identifier associated with the user. Used for sending notifications and alerts.

- **`TelegramUsername`** (string)
  The user's Telegram username. May be null or empty if not set.

- **`Email`** (string)
  The user's email address. Used for account-related communications.

- **`FirstName`** (string)
  The user's first name.

- **`LastName`** (string)
  The user's last name.

- **`IsActive`** (bool)
  Indicates whether the user account is active and eligible to receive alerts and notifications.

- **`ReceiveNotifications`** (bool)
  Flag determining if the user receives general notifications (e.g., alerts, updates).

- **`ReceiveDailyReport`** (bool)
  Flag determining if the user receives a daily summary report.

- **`DailyReportHourUtc`** (int)
  Hour (0–23) in UTC when the daily report should be sent, if enabled.

- **`CreatedAt`** (DateTime)
  Timestamp when the user profile was created. Immutable after creation.

- **`UpdatedAt`** (DateTime)
  Timestamp of the last profile update. Automatically updated on modification.

- **`LastActivityAt`** (long?)
  Unix timestamp (milliseconds) of the user's last interaction with the system. Null if never active.

- **`Preferences`** (string?)
  Serialized JSON string containing user-specific preferences. May be null.

- **`Alerts`** (ICollection<PriceAlert>)
  Collection of price alerts associated with the user. Managed via entity framework or similar ORM.

### Methods

- **`GetFullName()`** → string
  Returns the concatenated full name of the user (e.g., "John Doe"). Combines `FirstName` and `LastName`. Returns an empty string if both are null or empty.

- **`UpdateActivity()`** → void
  Updates the `UpdatedAt` and `LastActivityAt` timestamps to the current UTC time. No parameters. No return value. No exceptions thrown.

- **`IsRecentlyActive()`** → bool
  Determines if the user has been active within the last 30 minutes. Uses `LastActivityAt` for comparison. Returns `false` if `LastActivityAt` is null.

- **`GetActiveAlertCount()`** → int
  Returns the number of alerts in the `Alerts` collection that are currently active (i.e., not expired or disabled). Does not modify state.

- **`IsValid()`** → bool
  Validates the user profile integrity. Returns `true` if:
  - `Id` > 0
  - `TelegramChatId` > 0
  - `Email` is a valid email format (non-empty and contains '@')
  - `CreatedAt` ≤ `UpdatedAt`
  Otherwise returns `false`. No exceptions thrown.

## Usage

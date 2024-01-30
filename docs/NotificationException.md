# NotificationException

Exception thrown when a notification-related operation fails, typically during the sending or processing of alerts via Telegram or webhook endpoints.

## API

### `NotificationException()`
Constructs a new `NotificationException` with a default message indicating a notification failure.

### `NotificationException(string message)`
Constructs a new `NotificationException` with a custom message describing the failure.

**Parameters**
- `message` (string): The error message associated with the exception.

### `TelegramNotificationException()`
Constructs a new `TelegramNotificationException` with a default message indicating a failure in sending a Telegram notification.

### `TelegramNotificationException(string message)`
Constructs a new `TelegramNotificationException` with a custom message describing the Telegram-specific failure.

**Parameters**
- `message` (string): The error message associated with the exception.

### `WebhookNotificationException()`
Constructs a new `WebhookNotificationException` with a default message indicating a failure in sending a webhook notification.

### `WebhookNotificationException(string message)`
Constructs a new `WebhookNotificationException` with a custom message describing the webhook-specific failure.

**Parameters**
- `message` (string): The error message associated with the exception.

## Usage

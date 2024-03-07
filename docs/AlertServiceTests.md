# AlertServiceTests
The `AlertServiceTests` class is designed to test the functionality of the `AlertService` class, which is responsible for managing alerts in the binance-p2p-monitor project. This class contains a series of test methods that verify the correct behavior of the `AlertService` under various scenarios, including creating, updating, and deleting alerts, as well as retrieving alerts for a specific user.

## API
The `AlertServiceTests` class contains the following public members:
* `AlertServiceTests`: The constructor for the `AlertServiceTests` class.
* `CreateAlertAsync_ShouldReturnAlertId_WhenAlertIsValidAndMaxAlertsNotReached`: Tests that creating a valid alert returns the alert ID when the maximum number of alerts has not been reached.
* `CreateAlertAsync_ShouldThrowInvalidAlertException_WhenAlertIsInvalid`: Tests that creating an invalid alert throws an `InvalidAlertException`.
* `CreateAlertAsync_ShouldThrowInvalidAlertException_WhenMaxAlertsReached`: Tests that creating an alert when the maximum number of alerts has been reached throws an `InvalidAlertException`.
* `UpdateAlertAsync_ShouldReturnTrue_WhenAlertIsValidAndExists`: Tests that updating a valid alert returns `true` when the alert exists.
* `UpdateAlertAsync_ShouldReturnFalse_WhenAlertDoesNotExist`: Tests that updating an alert that does not exist returns `false`.
* `DeleteAlertAsync_ShouldReturnTrue_WhenAlertExists`: Tests that deleting an alert returns `true` when the alert exists.
* `GetUserAlertsAsync_ShouldReturnAlerts_WhenUserHasAlerts`: Tests that retrieving alerts for a user returns the user's alerts when the user has alerts.

## Usage
Here are two examples of using the `AlertServiceTests` class:
```csharp
// Example 1: Creating a valid alert
var alertService = new AlertService();
var test = new AlertServiceTests();
await test.CreateAlertAsync_ShouldReturnAlertId_WhenAlertIsValidAndMaxAlertsNotReached();

// Example 2: Updating an existing alert
var existingAlertId = await alertService.CreateAlertAsync(new Alert { /* valid alert properties */ });
await test.UpdateAlertAsync_ShouldReturnTrue_WhenAlertIsValidAndExists();
```

## Notes
The `AlertServiceTests` class is designed to be used in a testing environment, and its methods should not be called in a production environment. The class is not thread-safe, and its methods should not be called concurrently. Additionally, the class assumes that the `AlertService` class is properly configured and initialized before use. Edge cases, such as creating an alert with invalid properties or updating a non-existent alert, are handled by the `InvalidAlertException` and other exception types.

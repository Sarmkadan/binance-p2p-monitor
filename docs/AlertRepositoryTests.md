# AlertRepositoryTests

Unit test class for `AlertRepository`, verifying CRUD operations and user-specific alert retrieval against an in-memory or test database context.

## API

### `AlertRepositoryTests()`
Constructor that initializes a fresh test context and repository instance for each test run.

### `Dispose()`
Disposes the test context and cleans up resources after each test completes.

### `AddAsync_ShouldAddAlertAndReturnId()`
Verifies that `AlertRepository.AddAsync` persists a new alert and returns a valid non-zero identifier.

- **Parameters**: None
- **Return value**: `Task` completing when the operation finishes
- **Throws**: Propagates any exceptions from the underlying data store (e.g., constraint violations)

### `GetByIdAsync_ShouldReturnAlert_WhenAlertExists()`
Ensures `AlertRepository.GetByIdAsync` returns the correct alert entity when the identifier is valid.

- **Parameters**: None
- **Return value**: `Task` asserting the alert is non-null and matches expected values
- **Throws**: Propagates exceptions if the identifier is malformed or the lookup fails

### `GetByIdAsync_ShouldReturnNull_WhenAlertDoesNotExist()`
Confirms `AlertRepository.GetByIdAsync` returns `null` when the requested alert does not exist.

- **Parameters**: None
- **Return value**: `Task` asserting the result is `null`
- **Throws**: None

### `UpdateAsync_ShouldUpdateAlertAndReturnTrue()`
Validates that `AlertRepository.UpdateAsync` modifies the alert in the store and returns `true` on success.

- **Parameters**: None
- **Return value**: `Task` asserting the update succeeded and the entity reflects changes
- **Throws**: Propagates exceptions if the update violates constraints or the identifier is invalid

### `UpdateAsync_ShouldReturnFalse_WhenAlertDoesNotExist()`
Checks that `AlertRepository.UpdateAsync` returns `false` when attempting to update a non-existent alert.

- **Parameters**: None
- **Return value**: `Task` asserting the result is `false`
- **Throws**: None

### `DeleteAsync_ShouldDeleteAlertAndReturnTrue()`
Ensures `AlertRepository.DeleteAsync` removes the alert and returns `true` when the alert existed.

- **Parameters**: None
- **Return value**: `Task` asserting the alert is no longer retrievable and the result is `true`
- **Throws**: Propagates exceptions if the identifier is malformed or the delete operation fails

### `DeleteAsync_ShouldReturnFalse_WhenAlertDoesNotExist()`
Confirms `AlertRepository.DeleteAsync` returns `false` when attempting to delete a non-existent alert.

- **Parameters**: None
- **Return value**: `Task` asserting the result is `false`
- **Throws**: None

### `GetUserAlertsAsync_ShouldReturnAlertsForUser()`
Verifies that `AlertRepository.GetUserAlertsAsync` returns only alerts belonging to the specified user.

- **Parameters**: None
- **Return value**: `Task` asserting the returned collection contains only alerts for the user and matches expected count
- **Throws**: Propagates exceptions if the user identifier is invalid or the query fails

## Usage

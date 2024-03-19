# PriceRepositoryTestsExtensions

Extension class providing test utilities for `PriceRepository` operations. It exposes helper methods to validate CRUD behavior and query scenarios against an in-memory or test database context, ensuring consistent test setup and assertions.

## API

### `GetByIdAsync_ShouldReturnValidPrice`

Validates that `PriceRepository.GetByIdAsync` returns a non-null `Price` when the entity exists.

- **Parameters**: None.
- **Return value**: `Task<Price>` – the retrieved price entity.
- **Throws**: Propagates any exception thrown by `PriceRepository.GetByIdAsync`.

### `AddAsync_ShouldReturnValidIdAndPersist`

Validates that `PriceRepository.AddAsync` returns a positive identifier and persists the entity.

- **Parameters**: None.
- **Return value**: `Task<int>` – the generated identifier of the added price.
- **Throws**: Propagates any exception thrown by `PriceRepository.AddAsync`.

### `GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist`

Validates that `PriceRepository.GetByIdAsync` returns `null` when the entity does not exist.

- **Parameters**: None.
- **Return value**: `Task<Price?>` – `null` if no price is found.
- **Throws**: Propagates any exception thrown by `PriceRepository.GetByIdAsync`.

### `GetLatestByAssetAndFiatAsync_ShouldReturnNull_WhenNoPriceExists`

Validates that `PriceRepository.GetLatestByAssetAndFiatAsync` returns `null` when no matching price exists.

- **Parameters**: None.
- **Return value**: `Task<Price?>` – `null` if no price matches the criteria.
- **Throws**: Propagates any exception thrown by `PriceRepository.GetLatestByAssetAndFiatAsync`.

### `UpdateAsync_ShouldReturnFalse_WhenPriceDoesNotExist`

Validates that `PriceRepository.UpdateAsync` returns `false` when attempting to update a non-existent price.

- **Parameters**: None.
- **Return value**: `Task<bool>` – `false` if the price was not found or updated.
- **Throws**: Propagates any exception thrown by `PriceRepository.UpdateAsync`.

### `DeleteAsync_ShouldReturnFalse_WhenPriceDoesNotExist`

Validates that `PriceRepository.DeleteAsync` returns `false` when attempting to delete a non-existent price.

- **Parameters**: None.
- **Return value**: `Task<bool>` – `false` if the price was not found or deleted.
- **Throws**: Propagates any exception thrown by `PriceRepository.DeleteAsync`.

### `GetAveragePriceAsync_ShouldReturnNull_WhenNoPricesInTimeRange`

Validates that `PriceRepository.GetAveragePriceAsync` returns `null` when no prices fall within the specified time range.

- **Parameters**: None.
- **Return value**: `Task<decimal?>` – `null` if no prices exist in the range.
- **Throws**: Propagates any exception thrown by `PriceRepository.GetAveragePriceAsync`.

### `GetPriceRepository`

Provides a configured `PriceRepository` instance for testing.

- **Parameters**: None.
- **Return value**: `PriceRepository` – a new or reused repository instance.
- **Throws**: May throw if database initialization fails.

### `GetDatabaseContext`

Provides a `DatabaseContext` instance for test isolation.

- **Parameters**: None.
- **Return value**: `DatabaseContext` – an in-memory or test database context.
- **Throws**: May throw if context creation fails.

## Usage

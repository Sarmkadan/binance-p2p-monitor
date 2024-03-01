# PriceRepository

Central repository for storing, retrieving, and managing `Price` records, primarily used to track asset-to-fiat price data from Binance P2P markets. Supports CRUD operations and querying by asset, fiat, time ranges, and change detection.

## API

### `public async Task<Price?> GetByIdAsync(Guid id)`
Retrieves a single `Price` record by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the price record.
- **Return value**
  - A `Price` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

---

### `public async Task<Price?> GetLatestByAssetAndFiatAsync(string asset, string fiat)`
Fetches the most recent `Price` for a given asset-fiat pair based on insertion order or timestamp.

- **Parameters**
  - `asset`: The cryptocurrency symbol (e.g., `"USDT"`).
  - `fiat`: The fiat currency code (e.g., `"EUR"`).
- **Return value**
  - The latest `Price` matching the pair; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentException` if `asset` or `fiat` is `null` or whitespace.

---

### `public async Task<IEnumerable<Price>> GetAllActiveAsync()`
Returns all active (non-deleted) `Price` records currently stored.

- **Return value**
  - An enumerable of `Price` instances, possibly empty.
- **Exceptions**
  - None.

---

### `public async Task<IEnumerable<Price>> GetByAssetAsync(string asset)`
Retrieves all active `Price` records for a specific asset.

- **Parameters**
  - `asset`: The cryptocurrency symbol.
- **Return value**
  - An enumerable of `Price` instances; empty if none match.
- **Exceptions**
  - Throws `ArgumentException` if `asset` is `null` or whitespace.

---
### `public async Task<IEnumerable<Price>> GetByFiatAsync(string fiat)`
Retrieves all active `Price` records for a specific fiat currency.

- **Parameters**
  - `fiat`: The fiat currency code.
- **Return value**
  - An enumerable of `Price` instances; empty if none match.
- **Exceptions**
  - Throws `ArgumentException` if `fiat` is `null` or whitespace.

---
### `public async Task<int> AddAsync(Price price)`
Adds a new `Price` record to the repository.

- **Parameters**
  - `price`: The `Price` instance to add.
- **Return value**
  - The number of affected rows (typically `1` on success).
- **Exceptions**
  - Throws `ArgumentNullException` if `price` is `null`.
  - Throws `InvalidOperationException` if the `price` violates uniqueness constraints (e.g., duplicate asset-fiat pair at the same timestamp).

---
### `public async Task<bool> UpdateAsync(Price price)`
Updates an existing `Price` record.

- **Parameters**
  - `price`: The updated `Price` instance.
- **Return value**
  - `true` if the record was found and updated; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `price` is `null`.

---
### `public async Task<bool> DeleteAsync(Guid id)`
Soft-deletes a `Price` record by marking it inactive.

- **Parameters**
  - `id`: The unique identifier of the record to delete.
- **Return value**
  - `true` if the record existed and was marked inactive; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentException` if `id` is an empty GUID.

---
### `public async Task<IEnumerable<Price>> GetPricesChangedSinceAsync(DateTimeOffset since)`
Returns all active `Price` records modified after the specified timestamp.

- **Parameters**
  - `since`: The cutoff timestamp (exclusive).
- **Return value**
  - An enumerable of `Price` instances; empty if none match.
- **Exceptions**
  - None.

---
### `public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat)`
Computes the average price across all active records for a given asset-fiat pair.

- **Parameters**
  - `asset`: The cryptocurrency symbol.
  - `fiat`: The fiat currency code.
- **Return value**
  - The average price as a `decimal`, or `null` if no matching records exist.
- **Exceptions**
  - Throws `ArgumentException` if `asset` or `fiat` is `null` or whitespace.

## Usage

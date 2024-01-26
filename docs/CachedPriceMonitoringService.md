# CachedPriceMonitoringService

A service that caches and monitors cryptocurrency prices from Binance P2P, providing efficient access to current prices, historical data, and spread analysis while minimizing API calls through in-memory caching.

## API

### `CachedPriceMonitoringService`
Initializes a new instance of the price monitoring service with the specified configuration.

### `public async Task<Price?> GetCurrentPriceAsync()`
Retrieves the most recently cached price for the monitored asset. Returns `null` if no price data is available.

- **Returns**: `Task<Price?>` – A task that resolves to the latest cached `Price` or `null` if unavailable.
- **Exceptions**: Throws `InvalidOperationException` if the service has not been started via `StartMonitoringAsync`.

### `public async Task<IEnumerable<Price>> GetAllCurrentPricesAsync()`
Returns all currently cached prices for the monitored asset(s). The collection may be empty if no prices are cached.

- **Returns**: `Task<IEnumerable<Price>>` – A task that resolves to an enumerable of cached `Price` objects.
- **Exceptions**: Throws `InvalidOperationException` if the service has not been started via `StartMonitoringAsync`.

### `public async Task<bool> UpdatePriceAsync()`
Forces an immediate refresh of price data from the Binance P2P API and updates the internal cache. Returns `true` if the update was successful and the cache was modified; otherwise `false`.

- **Returns**: `Task<bool>` – A task indicating whether the cache was updated with new data.
- **Exceptions**: Throws `InvalidOperationException` if the service has not been started via `StartMonitoringAsync`.

### `public async Task<decimal?> GetAveragePriceAsync()`
Computes the average price from all currently cached prices. Returns `null` if no prices are cached.

- **Returns**: `Task<decimal?>` – A task that resolves to the average price or `null` if no data is available.
- **Exceptions**: Throws `InvalidOperationException` if the service has not been started via `StartMonitoringAsync`.

### `public async Task<IEnumerable<Price>> GetPricesWithSignificantChangeAsync()`
Returns a collection of cached prices that exhibit a significant deviation from the average price, based on a configurable threshold. The threshold is applied as a percentage of the average price.

- **Returns**: `Task<IEnumerable<Price>>` – A task that resolves to an enumerable of `Price` objects with significant changes.
- **Exceptions**: Throws `InvalidOperationException` if the service has not been started via `StartMonitoringAsync`.

### `public async Task<Spread?> GetSpreadAnalysisAsync()`
Analyzes the spread between the highest bid and lowest ask prices from the current cache. Returns `null` if insufficient price data is available to compute a spread.

- **Returns**: `Task<Spread?>` – A task that resolves to a `Spread` object containing bid-ask spread metrics, or `null` if data is unavailable.
- **Exceptions**: Throws `InvalidOperationException` if the service has not been started via `StartMonitoringAsync`.

### `public async Task StartMonitoringAsync()`
Begins periodic price monitoring by initiating background tasks to fetch and cache price data at regular intervals. Must be called before any other public method to avoid exceptions.

- **Returns**: `Task` – A task that completes when monitoring has started.
- **Exceptions**: None.

### `public async Task StopMonitoringAsync()`
Stops all background monitoring tasks and clears the internal price cache. Subsequent calls to data retrieval methods will throw `InvalidOperationException` until monitoring is restarted.

- **Returns**: `Task` – A task that completes when monitoring has stopped.
- **Exceptions**: None.

## Usage

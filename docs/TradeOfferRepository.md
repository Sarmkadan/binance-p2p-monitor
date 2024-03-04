# TradeOfferRepository

Central repository component for persisting, retrieving, and managing `TradeOffer` entities related to Binance P2P trade advertisements. Provides asynchronous CRUD operations and specialized queries to support monitoring and analysis of active trade offers across assets and fiat pairs.

## API

### `TradeOfferRepository`

Initializes a new instance of the repository with required data access dependencies.

### `public async Task<TradeOffer?> GetByIdAsync(long id)`

Retrieves a single trade offer by its internal database identifier.

- **Parameters**
  - `id` – The internal numeric identifier of the trade offer.
- **Returns**
  - A `TradeOffer` instance if found; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `id` is less than or equal to zero.
  - Throws `InvalidOperationException` if the underlying data store fails during retrieval.

### `public async Task<TradeOffer?> GetByBinanceIdAsync(string binanceId)`

Fetches a trade offer by its unique Binance P2P advertisement ID.

- **Parameters**
  - `binanceId` – The Binance-assigned advertisement identifier (e.g., "123456789").
- **Returns**
  - A `TradeOffer` instance if found; otherwise `null`.
- **Exceptions**
  - Throws `ArgumentException` if `binanceId` is `null`, empty, or whitespace.
  - Throws `InvalidOperationException` if the query fails or the store is unavailable.

### `public async Task<IEnumerable<TradeOffer>> GetAllActiveAsync()`

Returns all currently active trade offers from the repository.

- **Returns**
  - An `IEnumerable<TradeOffer>` containing all active offers. The collection may be empty.
- **Exceptions**
  - Throws `InvalidOperationException` if the underlying store is unreachable or returns invalid data.

### `public async Task<IEnumerable<TradeOffer>> GetByAssetAndFiatAsync(string asset, string fiat)`

Filters trade offers by specified asset symbol and fiat currency.

- **Parameters**
  - `asset` – The crypto asset symbol (e.g., "USDT").
  - `fiat` – The fiat currency code (e.g., "USD").
- **Returns**
  - An `IEnumerable<TradeOffer>` of matching offers. May be empty.
- **Exceptions**
  - Throws `ArgumentException` if either `asset` or `fiat` is `null`, empty, or whitespace.
  - Throws `InvalidOperationException` on data access failure.

### `public async Task<IEnumerable<TradeOffer>> GetByTradeTypeAsync(TradeType tradeType)`

Returns all trade offers matching the specified trade direction (buy or sell).

- **Parameters**
  - `tradeType` – The `TradeType` enum value (`Buy` or `Sell`).
- **Returns**
  - An `IEnumerable<TradeOffer>` of matching offers. May be empty.
- **Exceptions**
  - Throws `InvalidOperationException` if the data store is unavailable.

### `public async Task<IEnumerable<TradeOffer>> GetBestOffersAsync(string asset, string fiat, TradeType tradeType, int limit)`

Retrieves the top-rated or most favorable trade offers based on internal ranking logic.

- **Parameters**
  - `asset` – The crypto asset symbol.
  - `fiat` – The fiat currency code.
  - `tradeType` – The trade direction (`Buy` or `Sell`).
  - `limit` – Maximum number of offers to return.
- **Returns**
  - An `IEnumerable<TradeOffer>` limited to `limit` entries, ordered by relevance.
- **Exceptions**
  - Throws `ArgumentException` if any string parameter is `null`, empty, or whitespace.
  - Throws `ArgumentOutOfRangeException` if `limit` is less than 1.
  - Throws `InvalidOperationException` if ranking or retrieval fails.

### `public async Task<int> AddAsync(TradeOffer offer)`

Persists a new trade offer to the repository.

- **Parameters**
  - `offer` – The `TradeOffer` instance to add.
- **Returns**
  - The internal numeric identifier assigned to the newly created offer.
- **Exceptions**
  - Throws `ArgumentNullException` if `offer` is `null`.
  - Throws `InvalidOperationException` if the offer conflicts with existing data or cannot be saved.

### `public async Task<bool> UpdateAsync(TradeOffer offer)`

Updates an existing trade offer in the repository.

- **Parameters**
  - `offer` – The modified `TradeOffer` instance with a valid internal identifier.
- **Returns**
  - `true` if the update was successful; otherwise `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `offer` is `null`.
  - Throws `InvalidOperationException` if the store is unavailable or the offer does not exist.

### `public async Task<bool> DeleteAsync(long id)`

Removes a trade offer by its internal identifier.

- **Parameters**
  - `id` – The internal numeric identifier of the offer to delete.
- **Returns**
  - `true` if the offer existed and was deleted; otherwise `false`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `id` is less than or equal to zero.
  - Throws `InvalidOperationException` if the deletion fails due to store issues.

### `public async Task<long> GetTotalOffersCountAsync()`

Returns the total number of trade offers stored in the repository.

- **Returns**
  - The total count of offers, including inactive ones.
- **Exceptions**
  - Throws `InvalidOperationException` if the count cannot be retrieved.

### `public async Task<decimal> GetAveragePriceAsync(string asset, string fiat, TradeType tradeType)`

Computes the average price of trade offers matching the specified asset, fiat, and trade type.

- **Parameters**
  - `asset` – The crypto asset symbol.
  - `fiat` – The fiat currency code.
  - `tradeType` – The trade direction (`Buy` or `Sell`).
- **Returns**
  - The arithmetic mean of prices as a `decimal`. Returns `0m` if no matching offers exist.
- **Exceptions**
  - Throws `ArgumentException` if any string parameter is `null`, empty, or whitespace.
  - Throws `InvalidOperationException` if the calculation or data access fails.

## Usage

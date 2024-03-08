# TradeOfferRepositoryTests

Test class for the `TradeOfferRepository` component in the **binance-p2p-monitor** project. It contains a suite of asynchronous unit tests that verify the repository’s CRUD operations and query methods against an in‑memory or mocked data store.

## API

### TradeOfferRepositoryTests()
Initializes a new instance of the test class. The constructor sets up any required test fixtures (e.g., in‑memory database context, mock services) before each test runs.

### void Dispose()
Releases resources allocated by the test class. Called automatically by the test framework after each test method completes. Implementations should dispose of database connections, mock objects, or any IDisposable dependencies.

### Task AddAsync_ShouldAddTradeOfferAndReturnId()
Verifies that calling `TradeOfferRepository.AddAsync` persists a new trade offer and returns its generated identifier.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May propagate exceptions from the repository (e.g., `DbUpdateException`) if the insert operation fails.

### Task GetByIdAsync_ShouldReturnTradeOffer_WhenOfferExists()
Confirms that `TradeOfferRepository.GetByIdAsync` returns the correct trade offer when an offer with the supplied identifier exists.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May throw if the repository encounters an error while querying (e.g., invalid identifier format).

### Task GetByIdAsync_ShouldReturnNull_WhenOfferDoesNotExist()
Ensures that `TradeOfferRepository.GetByIdAsync` returns `null` when no trade offer matches the given identifier.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May throw on unexpected data‑access failures.

### Task GetByBinanceIdAsync_ShouldReturnTradeOffer_WhenOfferExists()
Validates that `TradeOfferRepository.GetByBinanceIdAsync` retrieves the trade offer associated with a specific Binance offer ID when such an offer is present.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May throw exceptions originating from the underlying data store.

### Task GetAllActiveAsync_ShouldReturnAllActiveOffers()
Checks that `TradeOfferRepository.GetAllActiveAsync` returns all trade offers whose `IsActive` flag is set to `true`.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May throw if the query cannot be executed.

### Task UpdateAsync_ShouldUpdateTradeOfferAndReturnTrue()
Asserts that `TradeOfferRepository.UpdateAsync` modifies an existing trade offer and returns `true` to indicate success.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May throw if the update violates constraints or if the offer does not exist.

### Task DeleteAsync_ShouldDeleteTradeOfferAndReturnTrue()
Confirms that `TradeOfferRepository.DeleteAsync` removes a trade offer from the store and returns `true` upon successful deletion.

- **Parameters:** None.
- **Return Value:** A completed `Task`.
- **Throws:** May throw if the deletion fails (e.g., foreign‑key violation).

## Usage

```csharp
using System.Threading.Tasks;
using Xunit;
using BinanceP2pMonitor.Tests.Repositories;

public class TradeOfferRepositoryTests : IAsyncLifetime
{
    private TradeOfferRepository _repo;

    public Task InitializeAsync()
    {
        // Arrange: create an in‑memory repository for each test
        _repo = new TradeOfferRepository(/* test‑specific options */);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // Act: clean up resources after each test
        _repo.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task AddAsync_ShouldAddTradeOfferAndReturnId()
    {
        // Act
        var id = await _repo.AddAsync(new TradeOffer { /* properties */ });

        // Assert
        Assert.NotEqual(0, id);
        var offer = await _repo.GetByIdAsync(id);
        Assert.NotNull(offer);
    }
}
```

A simpler usage example that directly invokes a test method without the test framework harness:

```csharp
using System.Threading.Tasks;
using BinanceP2pMonitor.Tests.Repositories;

var tests = new TradeOfferRepositoryTests();
await tests.AddAsync_ShouldAddTradeOfferAndReturnId(); // verifies insert behavior
tests.Dispose(); // releases any held resources
```

## Notes

- The test class assumes that the repository under test is configured with an isolated data store (e.g., EF Core in‑memory database) so that tests do not affect persistent data.
- All test methods are asynchronous; callers must `await` them to observe any exceptions that may be thrown.
- If the repository implementation changes to throw domain‑specific exceptions (e.g., `OfferNotFoundException`), the corresponding tests will need to be updated to assert those exception types.
- The class is **not thread‑safe**; each test should instantiate its own `TradeOfferRepositoryTests` instance or rely on the test framework’s lifecycle (`IAsyncLifetime`) to avoid shared state between concurrent test executions.

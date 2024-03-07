# PriceRepositoryTests

`PriceRepositoryTests` is a test fixture that validates the behavior of `PriceRepository` within the `binance-p2p-monitor` project. It implements `IDisposable` to clean up any resources (e.g., database connections, mocked dependencies) after all tests have run. Each test method is asynchronous and uses standard assertion patterns to verify repository operations such as adding, retrieving, updating, deleting, and aggregating price records.

## API

### `public PriceRepositoryTests()`
Initializes a new instance of the test class. Typically sets up the test environment (e.g., in-memory database, mock services) required by the repository.

### `public void Dispose()`
Releases all resources used by the test fixture. Called once after all tests in the class have completed. Does not throw under normal conditions.

### `public async Task AddAsync_ShouldAddPriceAndReturnId()`
Tests that `PriceRepository.AddAsync` correctly inserts a new price record and returns its generated identifier.  
- **Parameters:** None.  
- **Returns:** `Task` – the test completes when the assertion passes.  
- **Throws:** `Xunit.Sdk.XunitException` if the returned ID is not a valid positive integer or if the record was not actually persisted.

### `public async Task GetByIdAsync_ShouldReturnPrice_WhenPriceExists()`
Tests that `PriceRepository.GetByIdAsync` returns the expected `Price` object when a record with the given ID exists.  
- **Parameters:** None.  
- **Returns:** `Task`.  
- **Throws:** `Xunit.Sdk.XunitException` if the returned object is null or its properties do not match the seeded data.

### `public async Task GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist()`
Tests that `PriceRepository.GetByIdAsync` returns `null` when queried with an ID that does not correspond to any stored record.  
- **Parameters:** None.  
- **Returns:** `Task`.  
- **Throws:** `Xunit.Sdk.XunitException` if the result is not null.

### `public async Task GetLatestByAssetAndFiatAsync_ShouldReturnLatestPrice()`
Tests that `PriceRepository.GetLatestByAssetAndFiatAsync` returns the most recent price record for a given asset and fiat currency pair.  
- **Parameters:** None.  
- **Returns:** `Task`.  
- **Throws:** `Xunit.Sdk.XunitException` if the returned record is not the one with the highest timestamp among the test data.

### `public async Task UpdateAsync_ShouldUpdatePriceAndReturnTrue()`
Tests that `PriceRepository.UpdateAsync` successfully modifies an existing price record and returns `true`.  
- **Parameters:** None.  
- **Returns:** `Task`.  
- **Throws:** `Xunit.Sdk.XunitException` if the method returns `false` or the record was not updated in the data store.

### `public async Task DeleteAsync_ShouldDeletePriceAndReturnTrue()`
Tests that `PriceRepository.DeleteAsync` removes an existing price record and returns `true`.  
- **Parameters:** None.  
- **Returns:** `Task`.  
- **Throws:** `Xunit.Sdk.XunitException` if the method returns `false` or the record still exists after deletion.

### `public async Task GetAveragePriceAsync_ShouldReturnAveragePrice()`
Tests that `PriceRepository.GetAveragePriceAsync` computes the correct average price over a specified time window for a given asset and fiat pair.  
- **Parameters:** None.  
- **Returns:** `Task`.  
- **Throws:** `Xunit.Sdk.XunitException` if the computed average does not match the expected value derived from the test data.

## Usage

The following examples demonstrate how to instantiate and run the tests using xUnit. In a typical CI/CD pipeline, these tests are executed automatically.

### Example 1: Running all tests in the fixture

```csharp
using Xunit;

public class TestRunner
{
    [Fact]
    public async Task RunPriceRepositoryTests()
    {
        // Arrange
        var fixture = new PriceRepositoryTests();

        // Act & Assert – each test is run individually
        await fixture.AddAsync_ShouldAddPriceAndReturnId();
        await fixture.GetByIdAsync_ShouldReturnPrice_WhenPriceExists();
        await fixture.GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist();
        await fixture.GetLatestByAssetAndFiatAsync_ShouldReturnLatestPrice();
        await fixture.UpdateAsync_ShouldUpdatePriceAndReturnTrue();
        await fixture.DeleteAsync_ShouldDeletePriceAndReturnTrue();
        await fixture.GetAveragePriceAsync_ShouldReturnAveragePrice();

        // Cleanup
        fixture.Dispose();
    }
}
```

### Example 2: Using a test collection to share context

```csharp
using Xunit;

[Collection("PriceRepository Tests")]
public class PriceRepositoryTestCollection
{
    private readonly PriceRepositoryTests _fixture;

    public PriceRepositoryTestCollection()
    {
        _fixture = new PriceRepositoryTests();
    }

    [Fact]
    public async Task AddAndRetrievePrice()
    {
        var id = await _fixture.AddAsync_ShouldAddPriceAndReturnId();
        // Additional assertions can be added here if the test method returns data
    }

    [Fact]
    public async Task UpdateAndDeletePrice()
    {
        await _fixture.UpdateAsync_ShouldUpdatePriceAndReturnTrue();
        await _fixture.DeleteAsync_ShouldDeletePriceAndReturnTrue();
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}
```

## Notes

- **Edge Cases:**  
  - `GetByIdAsync_ShouldReturnNull_WhenPriceDoesNotExist` verifies correct handling of non‑existent IDs, including zero and negative values if the repository uses integer keys.  
  - `GetAveragePriceAsync_ShouldReturnAveragePrice` should be tested with an empty data set to ensure the repository returns `0` or `null` (depending on design) rather than throwing an exception.  
  - `DeleteAsync_ShouldDeletePriceAndReturnTrue` should also confirm that deleting a non‑existent record returns `false` (or throws, per repository contract), though this is not covered by the listed tests.

- **Thread Safety:**  
  - The test fixture is not thread‑safe by design. Each test method assumes exclusive access to the underlying data store. Running tests in parallel on the same fixture instance may cause interleaved state and false failures.  
  - If the repository itself is intended for concurrent use, additional integration tests with parallel calls should be written separately. The current test methods do not exercise concurrent scenarios.  
  - The `Dispose` method should not be called while a test is still executing; it is intended to be invoked after all tests in the fixture have completed.

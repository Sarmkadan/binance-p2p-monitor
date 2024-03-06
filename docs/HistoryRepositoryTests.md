# HistoryRepositoryTests

`HistoryRepositoryTests` is the test suite for the `HistoryRepository` class in the `binance-p2p-monitor` project. It verifies the correct behavior of persistence operations against historical P2P trade data, including insertion, retrieval by identifier, filtered queries by asset/fiat pair within a time window, deletion of stale records, aggregate counts, and price extremes. The class implements `IDisposable` to clean up shared test fixtures such as an in‑memory or temporary database context.

## API

### `public HistoryRepositoryTests`
The parameterless constructor. Initializes the test context, typically by setting up a lightweight database instance (e.g., an in‑memory provider) and instantiating the `HistoryRepository` under test. No arguments are required; any configuration is self‑contained within the test class.

### `public void Dispose`
Releases all resources acquired during the test run. This includes disposing of the database context and any other `IDisposable` dependencies. Called by the test framework after each test execution when the class implements `IDisposable`.

### `public async Task AddAsync_ShouldAddHistoryAndReturnId`
**Purpose:** Validates that `AddAsync` persists a history record and returns a non‑default identifier.  
**Parameters:** None (self‑contained test).  
**Return value:** A completed `Task` (the assertion either passes or throws).  
**Throws:** Exception from the underlying assertion framework if the returned ID is zero or the record cannot be retrieved afterwards.

### `public async Task GetByIdAsync_ShouldReturnHistory_WhenHistoryExists`
**Purpose:** Confirms that `GetByIdAsync` returns the correct `History` entity when a record with the given ID exists in the store.  
**Parameters:** None.  
**Return value:** A completed `Task`.  
**Throws:** Assertion failure if the returned entity is `null` or its properties do not match the seeded data.

### `public async Task GetByIdAsync_ShouldReturnNull_WhenHistoryDoesNotExist`
**Purpose:** Ensures `GetByIdAsync` returns `null` for an ID that has no corresponding record.  
**Parameters:** None.  
**Return value:** A completed `Task`.  
**Throws:** Assertion failure if the result is not `null`.

### `public async Task GetHistoryByAssetAndFiatAsync_ShouldReturnHistoryForAssetAndFiatWithinHours`
**Purpose:** Verifies that `GetHistoryByAssetAndFiatAsync` returns only records matching the specified asset and fiat currency, and whose timestamps fall within the given number of hours from the current time.  
**Parameters:** None.  
**Return value:** A completed `Task`.  
**Throws:** Assertion failure if records outside the time window or with mismatched asset/fiat are included, or if expected records are missing.

### `public async Task DeleteOldRecordsAsync_ShouldDeleteRecordsOlderThanDays`
**Purpose:** Tests that `DeleteOldRecordsAsync` removes records whose age exceeds the specified number of days, while leaving newer records intact.  
**Parameters:** None.  
**Return value:** A completed `Task`.  
**Throws:** Assertion failure if old records remain or recent records are incorrectly deleted.

### `public async Task GetTotalHistoryCountAsync_ShouldReturnCorrectCount`
**Purpose:** Checks that `GetTotalHistoryCountAsync` returns the exact number of history records currently stored.  
**Parameters:** None.  
**Return value:** A completed `Task`.  
**Throws:** Assertion failure if the count does not match the expected value after seeding.

### `public async Task GetHighestPriceAsync_ShouldReturnHighestPrice_WithinHours`
**Purpose:** Ensures `GetHighestPriceAsync` returns the maximum price among records for a given asset/fiat pair within a specified time window.  
**Parameters:** None.  
**Return value:** A completed `Task`.  
**Throws:** Assertion failure if the returned price is not the true maximum, or if records outside the window influence the result.

## Usage

### Example 1: Running the full test suite with xUnit
```csharp
using Xunit;

public class HistoryRepositoryTestRunner
{
    [Fact]
    public async Task ExecuteAllHistoryRepositoryTests()
    {
        var tests = new HistoryRepositoryTests();
        try
        {
            await tests.AddAsync_ShouldAddHistoryAndReturnId();
            await tests.GetByIdAsync_ShouldReturnHistory_WhenHistoryExists();
            await tests.GetByIdAsync_ShouldReturnNull_WhenHistoryDoesNotExist();
            await tests.GetHistoryByAssetAndFiatAsync_ShouldReturnHistoryForAssetAndFiatWithinHours();
            await tests.DeleteOldRecordsAsync_ShouldDeleteRecordsOlderThanDays();
            await tests.GetTotalHistoryCountAsync_ShouldReturnCorrectCount();
            await tests.GetHighestPriceAsync_ShouldReturnHighestPrice_WithinHours();
        }
        finally
        {
            tests.Dispose();
        }
    }
}
```

### Example 2: Selective integration into a CI pipeline
```csharp
using System;
using System.Threading.Tasks;

public static class CiVerification
{
    public static async Task VerifyHistoryRepository()
    {
        using var tests = new HistoryRepositoryTests();

        // Core CRUD and query operations
        await tests.AddAsync_ShouldAddHistoryAndReturnId();
        await tests.GetByIdAsync_ShouldReturnHistory_WhenHistoryExists();
        await tests.GetByIdAsync_ShouldReturnNull_WhenHistoryDoesNotExist();

        // Business‑critical filtered queries
        await tests.GetHistoryByAssetAndFiatAsync_ShouldReturnHistoryForAssetAndFiatWithinHours();
        await tests.GetHighestPriceAsync_ShouldReturnHighestPrice_WithinHours();

        // Maintenance operations
        await tests.DeleteOldRecordsAsync_ShouldDeleteRecordsOlderThanDays();
        await tests.GetTotalHistoryCountAsync_ShouldReturnCorrectCount();

        Console.WriteLine("All HistoryRepository tests passed.");
    }
}
```

## Notes

- **Test isolation:** Each test method is designed to run independently. The constructor and `Dispose` method ensure a fresh database state for every test, preventing cross‑test contamination.
- **Time‑sensitive queries:** Tests involving time windows (`GetHistoryByAssetAndFiatAsync_ShouldReturnHistoryForAssetAndFiatWithinHours`, `GetHighestPriceAsync_ShouldReturnHighestPrice_WithinHours`, `DeleteOldRecordsAsync_ShouldDeleteRecordsOlderThanDays`) rely on seeded timestamps relative to the current moment. On extremely slow test runners or systems with clock skew, borderline records could fall just outside the window and cause spurious failures.
- **Thread safety:** The test class itself is not thread‑safe by design; test methods are expected to be invoked sequentially by the test framework. Concurrent execution of multiple tests against the same instance is not supported and would lead to shared‑state corruption.
- **Disposal:** Failure to call `Dispose` may leave database connections or file handles open, especially when using a real database provider for integration testing. The `using` statement or explicit `try/finally` block is strongly recommended.
- **Edge cases covered:** The suite explicitly tests the “not found” path (`GetByIdAsync_ShouldReturnNull_WhenHistoryDoesNotExist`), deletion boundaries (`DeleteOldRecordsAsync_ShouldDeleteRecordsOlderThanDays`), and aggregate correctness on empty or sparsely populated tables (`GetTotalHistoryCountAsync_ShouldReturnCorrectCount`).

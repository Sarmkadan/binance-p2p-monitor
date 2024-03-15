# PriceHistoryServiceTestsExtensions

This static class provides factory methods for creating `PriceHistoryService` instances and sample `PriceHistory` sequences used in unit tests. The helpers simplify test setup by encapsulating common configurations such as default services, services pre‑populated with history, services that simulate cleanup outcomes, and services that throw exceptions.

## API

### CreateDefaultService
- **Purpose:** Returns a `PriceHistoryService` configured with default dependencies and no pre‑loaded history.
- **Parameters:** None.
- **Return Value:** A new `PriceHistoryService` ready for use.
- **Throws:** None.

### CreateServiceWithHistory
- **Purpose:** Returns a `PriceHistoryService` initialized with the supplied history sequence.
- **Parameters:** 
  - `history` (`IEnumerable<PriceHistory>`): The price history to inject into the service.
- **Return Value:** A `PriceHistoryService` that will expose the given history.
- **Throws:** 
  - `ArgumentNullException` if `history` is `null`.

### CreateServiceWithHistoryCount
- **Purpose:** Returns a `PriceHistoryService` populated with a generated history containing the specified number of entries.
- **Parameters:** 
  - `count` (`int`): Number of history entries to generate.
- **Return Value:** A `PriceHistoryService` containing `count` synthetic `PriceHistory` items.
- **Throws:** 
  - `ArgumentOutOfRangeException` if `count` is negative.

### CreateServiceWithCleanupResult
- **Purpose:** Returns a `PriceHistoryService` whose cleanup operation yields the supplied boolean result.
- **Parameters:** 
  - `cleanupResult` (`bool`): The value to return when the service’s cleanup method is invoked.
- **Return Value:** A `PriceHistoryService` configured to return `cleanupResult` from its cleanup logic.
- **Throws:** None.

### CreateAscendingPriceHistory
- **Purpose:** Generates an `IEnumerable<PriceHistory>` where prices increase monotonically.
- **Parameters:** 
  - `count` (`int`): Number of history items to produce.
- **Return Value:** An sequence of `PriceHistory` objects with strictly ascending prices.
- **Throws:** 
  - `ArgumentOutOfRangeException` if `count` is negative.

### CreateDescendingPriceHistory
- **Purpose:** Generates an `IEnumerable<PriceHistory>` where prices decrease monotonically.
- **Parameters:** 
  - `count` (`int`): Number of history items to produce.
- **Return Value:** An sequence of `PriceHistory` objects with strictly descending prices.
- **Throws:** 
  - `ArgumentOutOfRangeException` if `count` is negative.

### CreateServiceWithException
- **Purpose:** Returns a `PriceHistoryService` that throws the supplied exception when a specified operation is invoked (e.g., fetching history or performing cleanup).
- **Parameters:** 
  - `ex` (`Exception`): The exception to be thrown by the service.
- **Return Value:** A `PriceHistoryService` configured to propagate `ex`.
- **Throws:** 
  - `ArgumentNullException` if `ex` is `null`.

## Usage

```csharp
// Example 1: Testing a method that depends on price history.
var history = PriceHistoryServiceTestsExtensions.CreateAscendingPriceHistory(5);
var service = PriceHistoryServiceTestsExtensions.CreateServiceWithHistory(history);

// Act
var result = service.GetLatestPrice();

// Assert
Assert.AreEqual(history.Last().Price, result);
```

```csharp
// Example 2: Verifying cleanup behavior with a custom outcome.
var service = PriceHistoryServiceTestsExtensions.CreateServiceWithCleanupResult(true);

// Act
bool cleanupSucceeded = service.PerformCleanup();

// Assert
Assert.IsTrue(cleanupSucceeded);
```

```csharp
// Example 3: Simulating a failure scenario.
var ex = new InvalidOperationException("Simulated failure");
var service = PriceHistoryServiceTestsExtensions.CreateServiceWithException(ex);

// Act & Assert
Assert.Throws<InvalidOperationException>(() => service.FetchHistory());
```

## Notes

- All factory methods are static and do not rely on mutable shared state, making them safe to call concurrently from multiple threads.
- The returned `PriceHistoryService` instances are not guaranteed to be thread‑safe; callers must synchronize access if the same instance is used across threads.
- Negative `count` arguments are considered invalid and will result in an `ArgumentOutOfRangeException`.
- Passing `null` for any reference‑type parameter (`history` or `ex`) triggers an `ArgumentNullException`.
- The generated history sequences (`CreateAscendingPriceHistory` and `CreateDescendingPriceHistory`) produce deterministic values based solely on the supplied `count`, which aids in reproducible unit tests.

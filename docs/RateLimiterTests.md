# RateLimiterTests
The `RateLimiterTests` class is a test suite designed to verify the correctness and functionality of a rate limiter implementation. It provides a comprehensive set of test cases to ensure that the rate limiter behaves as expected under various scenarios, including allowing requests up to the maximum allowed, refilling tokens after a time window, handling multiple keys independently, and more.

## API
The `RateLimiterTests` class contains the following public members:
* `IsAllowed_ShouldAllowRequestsUpToMaxRequests`: Verifies that the rate limiter allows requests up to the maximum allowed.
* `IsAllowed_ShouldRefillTokensAfterTimeWindow`: Tests that the rate limiter refills tokens after a time window.
* `IsAllowed_ShouldHandleMultipleKeysIndependently`: Ensures that the rate limiter handles multiple keys independently.
* `IsAllowed_ShouldBeThreadSafe`: Asynchronously tests that the rate limiter is thread-safe.
* `GetRemainingTokens_ShouldReturnCorrectCount`: Verifies that the `GetRemainingTokens` method returns the correct count of remaining tokens.
* `GetRemainingTokens_ShouldReturnMaxRequestsForNonExistentKey`: Tests that the `GetRemainingTokens` method returns the maximum allowed requests for a non-existent key.
* `Reset_ShouldRestoreTokensForGivenKey`: Ensures that the `Reset` method restores tokens for a given key.
* `Reset_ShouldNotAffectOtherKeys`: Verifies that the `Reset` method does not affect other keys.
* `Reset_ShouldDoNothingForNonExistentKey`: Tests that the `Reset` method does nothing for a non-existent key.
* `ClearAll_ShouldClearAllBuckets`: Ensures that the `ClearAll` method clears all buckets.
* `GetTimeUntilNextToken_ShouldReturnZero_WhenTokensAvailable`: Verifies that the `GetTimeUntilNextToken` method returns zero when tokens are available.
* `GetTimeUntilNextToken_ShouldReturnPositiveTime_WhenNoTokensAvailable`: Tests that the `GetTimeUntilNextToken` method returns a positive time when no tokens are available.
* `GetTimeUntilNextToken_ShouldReturnNull_ForNonExistentKey`: Ensures that the `GetTimeUntilNextToken` method returns null for a non-existent key.

## Usage
Here are two examples of using the `RateLimiterTests` class:
```csharp
// Example 1: Testing the rate limiter with a single key
var rateLimiter = new RateLimiter();
var test = new RateLimiterTests();
test.IsAllowed_ShouldAllowRequestsUpToMaxRequests();
test.GetRemainingTokens_ShouldReturnCorrectCount();

// Example 2: Testing the rate limiter with multiple keys
var rateLimiter = new RateLimiter();
var test = new RateLimiterTests();
test.IsAllowed_ShouldHandleMultipleKeysIndependently();
test.Reset_ShouldRestoreTokensForGivenKey();
```

## Notes
The `RateLimiterTests` class is designed to be thread-safe, as demonstrated by the `IsAllowed_ShouldBeThreadSafe` test. However, it is essential to note that the rate limiter implementation itself must also be thread-safe to ensure correct behavior in multi-threaded environments. Additionally, the `GetTimeUntilNextToken` method may return null for non-existent keys, and the `Reset` method may not affect other keys. These edge cases should be considered when using the rate limiter in production environments.

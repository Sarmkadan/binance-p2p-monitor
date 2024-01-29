# RateLimiter
The `RateLimiter` type is designed to control the rate at which certain actions can be performed, typically to prevent abuse or overload of a system. It uses a token bucket algorithm to manage the rate limit, where tokens are added to the bucket at a specified rate, and actions can only be performed if there are sufficient tokens available.

## API
* `public RateLimiter`: The constructor for the `RateLimiter` class, used to create a new instance.
* `public bool IsAllowed`: Checks if an action is allowed based on the current state of the rate limiter. Returns `true` if the action is allowed, `false` otherwise.
* `public int GetRemainingTokens`: Returns the number of remaining tokens in the bucket.
* `public void Reset`: Resets the rate limiter to its initial state.
* `public void ClearAll`: Clears all tokens from the bucket.
* `public TimeSpan? GetTimeUntilNextToken`: Returns the time until the next token will be added to the bucket, or `null` if no tokens are available.
* `public TokenBucket TokenBucket`: Exposes the underlying token bucket used by the rate limiter.
* `public bool TryConsumeToken`: Attempts to consume a token from the bucket. Returns `true` if the token was consumed successfully, `false` otherwise.
* `public int GetRemainingTokens`: Returns the number of remaining tokens in the bucket.
* `public TimeSpan? GetTimeUntilNextToken`: Returns the time until the next token will be added to the bucket, or `null` if no tokens are available.

## Usage
```csharp
// Example 1: Simple rate limiting
var rateLimiter = new RateLimiter();
if (rateLimiter.TryConsumeToken())
{
    // Perform action
    Console.WriteLine("Action performed");
}
else
{
    // Rate limit exceeded
    Console.WriteLine("Rate limit exceeded");
}

// Example 2: Using the rate limiter to manage a queue of actions
var rateLimiter = new RateLimiter();
var actions = new Queue<Action>();
while (actions.Count > 0)
{
    if (rateLimiter.TryConsumeToken())
    {
        var action = actions.Dequeue();
        action();
    }
    else
    {
        // Rate limit exceeded, wait until next token is available
        var timeUntilNextToken = rateLimiter.GetTimeUntilNextToken;
        if (timeUntilNextToken.HasValue)
        {
            Thread.Sleep(timeUntilNextToken.Value);
        }
    }
}
```

## Notes
The `RateLimiter` class is designed to be thread-safe, allowing it to be used in concurrent environments. However, it is still possible for multiple threads to attempt to consume tokens simultaneously, which may lead to unexpected behavior. In such cases, it is recommended to use a lock or other synchronization mechanism to ensure that only one thread can access the rate limiter at a time. Additionally, the `GetTimeUntilNextToken` method may return `null` if no tokens are available, indicating that the rate limiter has been exhausted. In such cases, it is recommended to wait until the next token is added to the bucket before attempting to perform an action.

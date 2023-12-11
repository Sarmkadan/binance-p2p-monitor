#nullable enable

using BinanceP2pMonitor.Infrastructure;
using FluentAssertions;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Tests;

public class RateLimiterTests
{
    [Fact]
    public void IsAllowed_ShouldAllowRequestsUpToMaxRequests()
    {
        // Arrange
        var maxRequests = 3;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "testKey";

        // Act & Assert
        for (int i = 0; i < maxRequests; i++)
        {
            limiter.IsAllowed(key).Should().BeTrue($"Request {i + 1} should be allowed");
        }
        limiter.IsAllowed(key).Should().BeFalse("No more requests should be allowed");
    }

    [Fact]
    public void IsAllowed_ShouldRefillTokensAfterTimeWindow()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMilliseconds(100); // Short time window for testing
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "testKey";

        // Act & Assert
        limiter.IsAllowed(key).Should().BeTrue("First request should be allowed");
        limiter.IsAllowed(key).Should().BeFalse("Second request should be denied immediately");

        Thread.Sleep(timeWindow); // Wait for the time window to pass

        limiter.IsAllowed(key).Should().BeTrue("Request should be allowed after refill");
    }

    [Fact]
    public void IsAllowed_ShouldHandleMultipleKeysIndependently()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key1 = "key1";
        var key2 = "key2";

        // Act
        limiter.IsAllowed(key1).Should().BeTrue();
        limiter.IsAllowed(key1).Should().BeFalse(); // Key1 exhausted

        limiter.IsAllowed(key2).Should().BeTrue(); // Key2 still works
        limiter.IsAllowed(key2).Should().BeFalse(); // Key2 exhausted

        // Assert
        limiter.IsAllowed(key1).Should().BeFalse();
        limiter.IsAllowed(key2).Should().BeFalse();
    }
    
    [Fact]
    public async Task IsAllowed_ShouldBeThreadSafe()
    {
        // Arrange
        var maxRequests = 10;
        var timeWindow = TimeSpan.FromSeconds(10);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "sharedKey";
        int allowedCount = 0;
        int numTasks = 100;

        // Act
        var tasks = new Task[numTasks];
        for (int i = 0; i < numTasks; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                if (limiter.IsAllowed(key))
                {
                    Interlocked.Increment(ref allowedCount);
                }
            });
        }
        await Task.WhenAll(tasks);

        // Assert
        allowedCount.Should().Be(maxRequests);
    }

    [Fact]
    public void GetRemainingTokens_ShouldReturnCorrectCount()
    {
        // Arrange
        var maxRequests = 5;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "testKey";

        // Act & Assert
        limiter.GetRemainingTokens(key).Should().Be(maxRequests, "Initially all tokens should be available");

        limiter.IsAllowed(key); // Consume 1
        limiter.GetRemainingTokens(key).Should().Be(maxRequests - 1, "Should show 1 less token");

        limiter.IsAllowed(key); // Consume 2
        limiter.IsAllowed(key); // Consume 3
        limiter.GetRemainingTokens(key).Should().Be(maxRequests - 3, "Should show 3 less tokens");
    }

    [Fact]
    public void GetRemainingTokens_ShouldReturnMaxRequestsForNonExistentKey()
    {
        // Arrange
        var maxRequests = 5;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var nonExistentKey = "nonExistent";

        // Act
        var remainingTokens = limiter.GetRemainingTokens(nonExistentKey);

        // Assert
        remainingTokens.Should().Be(maxRequests);
    }

    [Fact]
    public void Reset_ShouldRestoreTokensForGivenKey()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "testKey";

        limiter.IsAllowed(key).Should().BeTrue();
        limiter.IsAllowed(key).Should().BeFalse(); // Exhaust tokens

        // Act
        limiter.Reset(key);

        // Assert
        limiter.IsAllowed(key).Should().BeTrue("Tokens should be restored after reset");
        limiter.GetRemainingTokens(key).Should().Be(maxRequests);
    }

    [Fact]
    public void Reset_ShouldNotAffectOtherKeys()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key1 = "key1";
        var key2 = "key2";

        limiter.IsAllowed(key1).Should().BeTrue();
        limiter.IsAllowed(key1).Should().BeFalse(); // Exhaust key1
        limiter.IsAllowed(key2).Should().BeTrue();
        limiter.IsAllowed(key2).Should().BeFalse(); // Exhaust key2

        // Act
        limiter.Reset(key1);

        // Assert
        limiter.IsAllowed(key1).Should().BeTrue(); // Key1 restored
        limiter.IsAllowed(key2).Should().BeFalse("Key2 should remain exhausted"); // Key2 not affected
    }

    [Fact]
    public void Reset_ShouldDoNothingForNonExistentKey()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var nonExistentKey = "nonExistent";

        // Act
        Action act = () => limiter.Reset(nonExistentKey);

        // Assert
        act.Should().NotThrow(); // Should not throw an error
    }

    [Fact]
    public void ClearAll_ShouldClearAllBuckets()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key1 = "key1";
        var key2 = "key2";

        limiter.IsAllowed(key1).Should().BeTrue();
        limiter.IsAllowed(key1).Should().BeFalse(); // Exhaust key1
        limiter.IsAllowed(key2).Should().BeTrue();
        limiter.IsAllowed(key2).Should().BeFalse(); // Exhaust key2

        // Act
        limiter.ClearAll();

        // Assert
        limiter.IsAllowed(key1).Should().BeTrue("Key1 should be restored after ClearAll");
        limiter.IsAllowed(key2).Should().BeTrue("Key2 should be restored after ClearAll");
    }

    [Fact]
    public void GetTimeUntilNextToken_ShouldReturnZero_WhenTokensAvailable()
    {
        // Arrange
        var maxRequests = 2;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "testKey";

        // Act
        var time = limiter.GetTimeUntilNextToken(key);

        // Assert
        time.Should().NotBeNull();
        time.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetTimeUntilNextToken_ShouldReturnPositiveTime_WhenNoTokensAvailable()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMilliseconds(500);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var key = "testKey";

        limiter.IsAllowed(key); // Consume the only token

        // Act
        var time = limiter.GetTimeUntilNextToken(key);

        // Assert
        time.Should().NotBeNull();
        time.Should().BeGreaterThan(TimeSpan.FromMilliseconds(400)); // Should be close to timeWindow
        time.Should().BeLessThanOrEqualTo(timeWindow);
    }

    [Fact]
    public void GetTimeUntilNextToken_ShouldReturnNull_ForNonExistentKey()
    {
        // Arrange
        var maxRequests = 1;
        var timeWindow = TimeSpan.FromMinutes(1);
        var limiter = new RateLimiter(maxRequests, timeWindow);
        var nonExistentKey = "nonExistent";

        // Act
        var time = limiter.GetTimeUntilNextToken(nonExistentKey);

        // Assert
        time.Should().BeNull();
    }
}

#nullable enable

using BinanceP2pMonitor.Infrastructure;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Tests rate-limit accounting across keys, time windows, and concurrent callers.
/// </summary>
public class RateLimiterConcurrencyTests
{
    /// <summary>
    /// Verifies that a single key cannot exceed the configured request limit within a window.
    /// </summary>
    [Fact]
    public void IsAllowed_ShouldEnforceMaxRequestsWithinWindowForSingleKey()
    {
        // Arrange
        const int maxRequests = 3;
        var limiter = new RateLimiter(maxRequests, TimeSpan.FromMinutes(1));
        const string key = "singleKey";

        // Act
        var allowedResults = new bool[maxRequests + 1];
        for (int i = 0; i < allowedResults.Length; i++)
        {
            allowedResults[i] = limiter.IsAllowed(key);
        }

        // Assert
        allowedResults.Should().Equal(true, true, true, false);
    }

    /// <summary>
    /// Verifies that consuming one key's bucket does not consume another key's bucket.
    /// </summary>
    [Fact]
    public void IsAllowed_ShouldMaintainIndependentBucketsForDifferentKeys()
    {
        // Arrange
        var limiter = new RateLimiter(1, TimeSpan.FromMinutes(1));
        const string firstKey = "firstKey";
        const string secondKey = "secondKey";

        // Act & Assert
        limiter.IsAllowed(firstKey).Should().BeTrue();
        limiter.IsAllowed(firstKey).Should().BeFalse();
        limiter.IsAllowed(secondKey).Should().BeTrue();
        limiter.IsAllowed(secondKey).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that remaining-token reporting reflects consumed requests.
    /// </summary>
    [Fact]
    public void GetRemainingTokens_ShouldDecreaseAfterConsumption()
    {
        // Arrange
        const int maxRequests = 4;
        var limiter = new RateLimiter(maxRequests, TimeSpan.FromMinutes(1));
        const string key = "tokenKey";

        // Act
        limiter.IsAllowed(key).Should().BeTrue();
        limiter.IsAllowed(key).Should().BeTrue();

        // Assert
        limiter.GetRemainingTokens(key).Should().Be(maxRequests - 2);
    }

    /// <summary>
    /// Verifies that concurrent callers cannot collectively exceed the configured limit.
    /// </summary>
    [Fact]
    public void IsAllowed_ShouldNotExceedMaxRequestsWhenCalledConcurrently()
    {
        // Arrange
        const int maxRequests = 10;
        const int attempts = 100;
        var limiter = new RateLimiter(maxRequests, TimeSpan.FromMinutes(1));
        const string key = "concurrentKey";
        var allowedCount = 0;

        // Act
        Parallel.For(0, attempts, _ =>
        {
            if (limiter.IsAllowed(key))
            {
                Interlocked.Increment(ref allowedCount);
            }
        });

        // Assert
        allowedCount.Should().Be(maxRequests);
        limiter.GetRemainingTokens(key).Should().Be(0);
    }

    /// <summary>
    /// Verifies that an exhausted bucket refills after its time window elapses.
    /// </summary>
    [Fact]
    public void IsAllowed_ShouldRefillTokensAfterWindowElapses()
    {
        // Arrange
        var timeWindow = TimeSpan.FromMilliseconds(100);
        var limiter = new RateLimiter(1, timeWindow);
        const string key = "refillKey";

        // Act & Assert
        limiter.IsAllowed(key).Should().BeTrue();
        limiter.IsAllowed(key).Should().BeFalse();

        Thread.Sleep(timeWindow + TimeSpan.FromMilliseconds(50));

        limiter.IsAllowed(key).Should().BeTrue();
    }
}

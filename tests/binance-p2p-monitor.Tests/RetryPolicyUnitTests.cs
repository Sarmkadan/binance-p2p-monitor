#nullable enable
using BinanceP2pMonitor.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for the <see cref="RetryPolicy"/> class.
/// </summary>
public class RetryPolicyUnitTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    /// <summary>
    /// Creates a new instance of <see cref="RetryPolicy"/> with mocked dependencies.
    /// </summary>
    /// <returns>A new <see cref="RetryPolicy"/> instance.</returns>
    private RetryPolicy CreatePolicy(
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        double backoffMultiplier = 2.0,
        TimeSpan? maxDelay = null)
    {
        return new RetryPolicy(
            maxRetries,
            initialDelay,
            backoffMultiplier,
            maxDelay,
            _loggerMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy"/> constructor initializes with default values when no parameters are provided.
    /// </summary>
    [Fact]
    public void Constructor_DefaultParameters_InitializesCorrectly()
    {
        // Act
        var policy = new RetryPolicy();

        // Assert - using reflection to verify private fields
        var maxRetriesField = policy.GetType().GetField("_maxRetries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var initialDelayField = policy.GetType().GetField("_initialDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var maxDelayField = policy.GetType().GetField("_maxDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var backoffMultiplierField = policy.GetType().GetField("_backoffMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        maxRetriesField.Should().NotBeNull();
        initialDelayField.Should().NotBeNull();
        maxDelayField.Should().NotBeNull();
        backoffMultiplierField.Should().NotBeNull();

        maxRetriesField?.GetValue(policy).Should().Be(3);
        initialDelayField?.GetValue(policy).Should().Be(TimeSpan.FromSeconds(1));
        maxDelayField?.GetValue(policy).Should().Be(TimeSpan.FromSeconds(30));
        backoffMultiplierField?.GetValue(policy).Should().Be(2.0);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy"/> constructor initializes with custom values when parameters are provided.
    /// </summary>
    [Fact]
    public void Constructor_CustomParameters_InitializesCorrectly()
    {
        // Arrange
        var initialDelay = TimeSpan.FromSeconds(2);
        var maxDelay = TimeSpan.FromSeconds(60);

        // Act
        var policy = new RetryPolicy(
            maxRetries: 5,
            initialDelay: initialDelay,
            backoffMultiplier: 3.0,
            maxDelay: maxDelay);

        // Assert
        var maxRetriesField = policy.GetType().GetField("_maxRetries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var initialDelayField = policy.GetType().GetField("_initialDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var maxDelayField = policy.GetType().GetField("_maxDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var backoffMultiplierField = policy.GetType().GetField("_backoffMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        maxRetriesField?.GetValue(policy).Should().Be(5);
        initialDelayField?.GetValue(policy).Should().Be(initialDelay);
        maxDelayField?.GetValue(policy).Should().Be(maxDelay);
        backoffMultiplierField?.GetValue(policy).Should().Be(3.0);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> succeeds on first attempt without retry.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ReturnsResultWithoutRetry()
    {
        // Arrange
        var policy = CreatePolicy();
        var expectedResult = "Success";
        var attemptCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            await Task.CompletedTask;
            return expectedResult;
        }).ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedResult);
        attemptCount.Should().Be(1);
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> retries on failure and succeeds on retry.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithTransientFailure_RetriesAndSucceeds()
    {
        // Arrange
        var policy = CreatePolicy(maxRetries: 3);
        var expectedResult = "Success";
        var attemptCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new TimeoutException("Simulated timeout");
            }
            await Task.CompletedTask;
            return expectedResult;
        }).ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedResult);
        attemptCount.Should().Be(3);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempt 1/3 failed")),
                It.IsAny<TimeoutException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Attempt 2/3 failed")),
                It.IsAny<TimeoutException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> throws after max retries are exhausted.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithPersistentFailure_ThrowsAfterMaxRetries()
    {
        // Arrange
        var policy = CreatePolicy(maxRetries: 2);
        var attemptCount = 0;

        // Act
        Func<Task> act = async () => await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            throw new HttpRequestException("Simulated HTTP error");
        }).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        attemptCount.Should().Be(2);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Operation failed after")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> respects custom shouldRetry predicate.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithCustomShouldRetryPredicate_RespectsPredicate()
    {
        // Arrange
        var policy = CreatePolicy(maxRetries: 5);
        var attemptCount = 0;

        // Custom predicate that only retries TimeoutException
        Func<Exception, bool> shouldRetry = ex => ex is TimeoutException;

        // Act
        Func<Task> act = async () => await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            throw new InvalidOperationException("Non-retryable error");
        }, shouldRetry).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        attemptCount.Should().Be(1); // Should not retry because predicate returns false
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> respects custom shouldRetry predicate that allows retry.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithCustomShouldRetryPredicateAllowsRetry_RetriesSuccessfully()
    {
        // Arrange
        var policy = CreatePolicy(maxRetries: 3);
        var attemptCount = 0;

        // Custom predicate that allows retry for specific exception
        Func<Exception, bool> shouldRetry = ex => ex is TimeoutException;

        // Act
        var result = await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new TimeoutException("Simulated timeout");
            }
            await Task.CompletedTask;
            return "Success";
        }, shouldRetry).ConfigureAwait(false);

        // Assert
        result.Should().Be("Success");
        attemptCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> respects cancellation token.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var policy = CreatePolicy();
        var cts = new CancellationTokenSource();
        var attemptCount = 0;

        // Act
        Func<Task> act = async () => await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            cts.Cancel();
            await Task.Delay(1000, ct); // This will throw OperationCanceledException
            return "Should not reach here";
        }, ct: cts.Token).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        attemptCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync"/> succeeds on first attempt without retry.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Void_WithSuccessfulOperation_ExecutesWithoutRetry()
    {
        // Arrange
        var policy = CreatePolicy();
        var attemptCount = 0;

        // Act
        await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            await Task.CompletedTask;
        }).ConfigureAwait(false);

        // Assert
        attemptCount.Should().Be(1);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync"/> retries on failure and succeeds on retry.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Void_WithTransientFailure_RetriesAndSucceeds()
    {
        // Arrange
        var policy = CreatePolicy(maxRetries: 3);
        var attemptCount = 0;

        // Act
        await policy.ExecuteAsync(async ct =>
        {
            attemptCount++;
            if (attemptCount < 3)
            {
                throw new IOException("Simulated IO error");
            }
            await Task.CompletedTask;
        }).ConfigureAwait(false);

        // Assert
        attemptCount.Should().Be(3);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.IsTransientError"/> returns true for TimeoutException.
    /// </summary>
    [Fact]
    public void IsTransientError_TimeoutException_ReturnsTrue()
    {
        // Act
        var result = RetryPolicy.IsTransientError(new TimeoutException("Timeout"));

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.IsTransientError"/> returns true for HttpRequestException.
    /// </summary>
    [Fact]
    public void IsTransientError_HttpRequestException_ReturnsTrue()
    {
        // Act
        var result = RetryPolicy.IsTransientError(new HttpRequestException("HTTP error"));

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.IsTransientError"/> returns true for IOException.
    /// </summary>
    [Fact]
    public void IsTransientError_IOException_ReturnsTrue()
    {
        // Act
        var result = RetryPolicy.IsTransientError(new IOException("IO error"));

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.IsTransientError"/> returns false for OperationCanceledException.
    /// </summary>
    [Fact]
    public void IsTransientError_OperationCanceledException_ReturnsFalse()
    {
        // Act
        var result = RetryPolicy.IsTransientError(new OperationCanceledException("Cancelled"));

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.IsTransientError"/> returns false for generic Exception.
    /// </summary>
    [Fact]
    public void IsTransientError_GenericException_ReturnsFalse()
    {
        // Act
        var result = RetryPolicy.IsTransientError(new Exception("Generic error"));

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> handles exponential backoff correctly.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithMultipleRetries_AppliesExponentialBackoff()
    {
        // Arrange
        var policy = CreatePolicy(
            maxRetries: 4,
            initialDelay: TimeSpan.FromMilliseconds(100),
            backoffMultiplier: 2.0);
        var attemptTimes = new List<DateTime>();
        var startTime = DateTime.UtcNow;

        // Act
        try
        {
            await policy.ExecuteAsync(async ct =>
            {
                attemptTimes.Add(DateTime.UtcNow);
                throw new TimeoutException("Simulated timeout");
            }).ConfigureAwait(false);
        }
        catch { /* Expected to fail */ }

        // Assert - verify delays between attempts
        attemptTimes.Should().HaveCount(4);
        var delays = new List<TimeSpan>();
        for (int i = 1; i < attemptTimes.Count; i++)
        {
            delays.Add(attemptTimes[i] - attemptTimes[i - 1]);
        }

        // First delay should be initial delay (100ms)
        delays[0].TotalMilliseconds.Should().BeApproximately(100, 50);
        // Second delay should be 2x initial (200ms)
        delays[1].TotalMilliseconds.Should().BeApproximately(200, 50);
        // Third delay should be 4x initial (400ms)
        delays[2].TotalMilliseconds.Should().BeApproximately(400, 50);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> respects max delay cap.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithMaxDelay_RespectsMaxDelayCap()
    {
        // Arrange
        var maxDelay = TimeSpan.FromMilliseconds(200);
        var policy = CreatePolicy(
            maxRetries: 5,
            initialDelay: TimeSpan.FromMilliseconds(100),
            backoffMultiplier: 3.0,
            maxDelay: maxDelay);
        var attemptTimes = new List<DateTime>();

        // Act
        try
        {
            await policy.ExecuteAsync(async ct =>
            {
                attemptTimes.Add(DateTime.UtcNow);
                throw new TimeoutException("Simulated timeout");
            }).ConfigureAwait(false);
        }
        catch { /* Expected to fail */ }

        // Assert - verify that delays don't exceed max delay
        attemptTimes.Should().HaveCount(5);
        var delays = new List<TimeSpan>();
        for (int i = 1; i < attemptTimes.Count; i++)
        {
            delays.Add(attemptTimes[i] - attemptTimes[i - 1]);
        }

        // All delays should be capped at maxDelay (200ms)
        foreach (var delay in delays)
        {
            delay.Should().BeLessOrEqualTo(maxDelay.Add(TimeSpan.FromMilliseconds(10)));
        }
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> works with value types.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValueType_ReturnsCorrectValue()
    {
        // Arrange
        var policy = CreatePolicy();
        var expectedValue = 42;

        // Act
        var result = await policy.ExecuteAsync(async ct =>
        {
            await Task.CompletedTask;
            return expectedValue;
        }).ConfigureAwait(false);

        // Assert
        result.Should().Be(expectedValue);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> works with reference types.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithReferenceType_ReturnsCorrectObject()
    {
        // Arrange
        var policy = CreatePolicy();
        var expectedObject = new { Name = "Test", Value = 123 };

        // Act
        var result = await policy.ExecuteAsync(async ct =>
        {
            await Task.CompletedTask;
            return expectedObject;
        }).ConfigureAwait(false);

        // Assert
        result.Should().BeSameAs(expectedObject);
    }

    /// <summary>
    /// Verifies that <see cref="RetryPolicy.ExecuteAsync{T}"/> works with null return value.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNullReturn_ReturnsNull()
    {
        // Arrange
        var policy = CreatePolicy();

        // Act
        var result = await policy.ExecuteAsync(async ct =>
        {
            await Task.CompletedTask;
            return (string?)null;
        }).ConfigureAwait(false);

        // Assert
        result.Should().BeNull();
    }
}

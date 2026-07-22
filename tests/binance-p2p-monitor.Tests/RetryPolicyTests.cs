using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BinanceP2pMonitor.Infrastructure;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BinanceP2pMonitor.Tests
{
    public class RetryPolicyTests
    {
        [Fact]
        public async Task ExecuteAsyncT_ReturnsResult_WhenOperationSucceedsFirstTry()
        {
            // Arrange
            var policy = new RetryPolicy(maxRetries: 3);
            var expected = 42;

            // Act
            var result = await policy.ExecuteAsync<int>((ct) => Task.FromResult(expected));

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExecuteAsyncT_RetriesOnTransientError_AndEventuallySucceeds()
        {
            // Arrange
            var policy = new RetryPolicy(maxRetries: 5, initialDelay: TimeSpan.Zero);
            int callCount = 0;
            const int failTimes = 2;
            var expected = "success";

            Task<string> Operation(CancellationToken ct)
            {
                callCount++;
                if (callCount <= failTimes)
                {
                    throw new TimeoutException(); // transient
                }

                return Task.FromResult(expected);
            }

            // Act
            var result = await policy.ExecuteAsync<string>(Operation, RetryPolicy.IsTransientError);

            // Assert
            Assert.Equal(expected, result);
            Assert.Equal(failTimes + 1, callCount);
        }

        [Fact]
        public async Task ExecuteAsyncT_StopsRetrying_WhenNonTransientErrorOccurs()
        {
            // Arrange
            var policy = new RetryPolicy(maxRetries: 5, initialDelay: TimeSpan.Zero);
            int callCount = 0;

            Task<int> Operation(CancellationToken ct)
            {
                callCount++;
                throw new InvalidOperationException("non‑transient");
            }

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await policy.ExecuteAsync<int>(Operation, RetryPolicy.IsTransientError));

            Assert.Equal("non‑transient", ex.Message);
            Assert.Equal(1, callCount); // should not retry
        }

        [Fact]
        public async Task ExecuteAsyncT_ThrowsAfterMaxRetries_OnTransientError()
        {
            // Arrange
            var policy = new RetryPolicy(maxRetries: 3, initialDelay: TimeSpan.Zero);
            int callCount = 0;

            Task<int> Operation(CancellationToken ct)
            {
                callCount++;
                throw new IOException(); // transient
            }

            // Act & Assert
            var ex = await Assert.ThrowsAsync<IOException>(async () =>
                await policy.ExecuteAsync<int>(Operation, RetryPolicy.IsTransientError));

            Assert.Equal(3, callCount); // attempted maxRetries times
        }

        [Fact]
        public async Task ExecuteAsync_NoReturnValue_CompletesSuccessfully()
        {
            // Arrange
            var policy = new RetryPolicy();
            bool called = false;

            // Act
            await policy.ExecuteAsync(
                async ct =>
                {
                    await Task.Delay(1, ct);
                    called = true;
                });

            // Assert
            Assert.True(called);
        }

        [Theory]
        [InlineData(typeof(TimeoutException), true)]
        [InlineData(typeof(HttpRequestException), true)]
        [InlineData(typeof(IOException), true)]
        [InlineData(typeof(OperationCanceledException), false)]
        [InlineData(typeof(ArgumentException), false)]
        public void IsTransientError_ReturnsExpectedResult(Type exceptionType, bool expected)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            // Act
            var result = RetryPolicy.IsTransientError(exception);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void NullLogger_DoesNotThrow_OnLogCalls()
        {
            // Arrange
            var policy = new RetryPolicy(); // uses NullLogger internally
            var logger = typeof(RetryPolicy)
                .GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(policy) as ILogger;

            // Act & Assert – calling the logger methods should be no‑ops and not throw
            logger!.LogInformation("test");
            logger.LogWarning("test");
            logger.LogError(new Exception("error"), "test");
            logger.BeginScope("scope");
            logger.IsEnabled(LogLevel.Information);
        }
    }
}

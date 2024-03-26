using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Infrastructure
{
    /// <summary>
    /// Extension methods for retry policy functionality
    /// </summary>
    public static class RetryPolicyExtensions
    {
        /// <summary>
        /// Executes the specified async operation with retry logic using the default retry policy
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="policy">The retry policy instance.</param>
        /// <param name="action">The async operation to execute.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="action"/> is null.</exception>
        public static Task<T> ExecuteWithRetryAsync<T>(this RetryPolicy policy, Func<Task<T>> action)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(action);

            return policy.ExecuteAsync(async ct => await action().ConfigureAwait(false));
        }

        /// <summary>
        /// Executes the specified async operation with retry logic using the default retry policy
        /// </summary>
        /// <param name="policy">The retry policy instance.</param>
        /// <param name="action">The async operation to execute.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="action"/> is null.</exception>
        public static Task ExecuteWithRetryAsync(this RetryPolicy policy, Func<Task> action)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(action);

            return policy.ExecuteAsync(async ct =>
            {
                await action().ConfigureAwait(false);
                return true;
            });
        }

        /// <summary>
        /// Executes the specified async operation with retry logic using the default retry policy
        /// </summary>
        /// <typeparam name="T">The return type of the operation.</typeparam>
        /// <param name="policy">The retry policy instance.</param>
        /// <param name="action">The async operation to execute.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="action"/> is null.</exception>
        public static Task<T> ExecuteWithRetryAsync<T>(this RetryPolicy policy, Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(action);

            return policy.ExecuteAsync(action, ct: cancellationToken);
        }

        /// <summary>
        /// Executes the specified async operation with retry logic using the default retry policy
        /// </summary>
        /// <param name="policy">The retry policy instance.</param>
        /// <param name="action">The async operation to execute.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> or <paramref name="action"/> is null.</exception>
        public static Task ExecuteWithRetryAsync(this RetryPolicy policy, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(action);

            return policy.ExecuteAsync(async ct =>
            {
                await action(ct).ConfigureAwait(false);
                return true;
            }, ct: cancellationToken);
        }

        /// <summary>
        /// Determines whether the specified exception is considered retryable based on transient error detection.
        /// </summary>
        /// <param name="policy">The retry policy instance.</param>
        /// <param name="ex">The exception to check.</param>
        /// <returns>True if the exception is transient and should be retried; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        public static bool IsRetryableException(this RetryPolicy policy, Exception ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return RetryPolicy.IsTransientError(ex);
        }
    }
}
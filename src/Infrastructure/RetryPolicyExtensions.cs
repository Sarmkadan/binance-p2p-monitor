using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Infrastructure
{
    public static class RetryPolicyExtensions
    {
        public static async Task<T> ExecuteWithRetryAsync<T>(this RetryPolicy policy, Func<Task<T>> action)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            int attempt = 0;
            while (attempt < 5) // default max attempts
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (RetryPolicy.IsTransientError(ex))
                {
                    attempt++;
                    await Task.Delay(100); // default delay
                }
            }

            throw new Exception("Maximum attempts exceeded");
        }

        public static async Task ExecuteWithRetryAsync(this RetryPolicy policy, Func<Task> action)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            int attempt = 0;
            while (attempt < 5) // default max attempts
            {
                try
                {
                    await action();
                    break;
                }
                catch (Exception ex) when (RetryPolicy.IsTransientError(ex))
                {
                    attempt++;
                    await Task.Delay(100); // default delay
                }
            }
        }

        public static bool IsRetryableException(this RetryPolicy policy, Exception ex)
        {
            return RetryPolicy.IsTransientError(ex);
        }
    }
}

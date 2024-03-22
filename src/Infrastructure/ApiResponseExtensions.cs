using System;
using System.Collections.Generic;

namespace BinanceP2pMonitor.Infrastructure
{
    /// <summary>
    /// Extension methods for <see cref="ApiResponse"/> and <see cref="ApiResponse{T}"/>.
    /// </summary>
    public static class ApiResponseExtensions
    {
        /// <summary>
        /// Determines whether the response indicates success.
        /// </summary>
        /// <param name="response">The API response to check.</param>
        /// <returns><see langword="true"/> if the response was successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static bool IsSuccessful(this ApiResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
            return response.Success;
        }

        /// <summary>
        /// Determines whether the generic response indicates success.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="response">The API response to check.</param>
        /// <returns><see langword="true"/> if the response was successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static bool IsSuccessful<T>(this ApiResponse<T> response)
        {
            ArgumentNullException.ThrowIfNull(response);
            return response.Success;
        }

        /// <summary>
        /// Adds an error message to the response and returns the same instance for chaining.
        /// </summary>
        /// <param name="response">The API response to add the error to.</param>
        /// <param name="error">The error message to add.</param>
        /// <returns>The same <paramref name="response"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="error"/> is <see langword="null"/>.</exception>
        public static ApiResponse AddError(this ApiResponse response, string error)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(error);
            response.Errors.Add(error);
            return response;
        }

        /// <summary>
        /// Adds an error message to the generic response and returns the same instance for chaining.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="response">The API response to add the error to.</param>
        /// <param name="error">The error message to add.</param>
        /// <returns>The same <paramref name="response"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> or <paramref name="error"/> is <see langword="null"/>.</exception>
        public static ApiResponse<T> AddError<T>(this ApiResponse<T> response, string error)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(error);
            response.Errors.Add(error);
            return response;
        }

        /// <summary>
        /// Sets the <see cref="ApiResponse{T}.Data"/> property and returns the same instance for chaining.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="response">The API response to update.</param>
        /// <param name="data">The data to set.</param>
        /// <returns>The same <paramref name="response"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static ApiResponse<T> WithData<T>(this ApiResponse<T> response, T data)
        {
            ArgumentNullException.ThrowIfNull(response);
            response.Data = data;
            return response;
        }

        /// <summary>
        /// Returns a concise, human‑readable summary of the response.
        /// </summary>
        /// <param name="response">The API response to summarize.</param>
        /// <returns>A formatted string containing key response information.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static string Summary(this ApiResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);
            return $"[{response.Timestamp:u}] RequestId={response.RequestId} Success={response.Success} " +
                   $"Message={response.Message ?? "none"} Errors={response.Errors.Count}";
        }

        /// <summary>
        /// Returns a concise, human‑readable summary of the generic response, including the data type.
        /// </summary>
        /// <typeparam name="T">The type of data in the response.</typeparam>
        /// <param name="response">The API response to summarize.</param>
        /// <returns>A formatted string containing key response information and data type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/>.</exception>
        public static string Summary<T>(this ApiResponse<T> response)
        {
            ArgumentNullException.ThrowIfNull(response);
            var dataInfo = response.Data is null ? "null" : $"type={response.Data.GetType().Name}";
            return $"[{response.Timestamp:u}] RequestId={response.RequestId} Success={response.Success} " +
                   $"Message={response.Message ?? "none"} Errors={response.Errors.Count} Data={dataInfo}";
        }
    }
}

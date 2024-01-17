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
        public static bool IsSuccessful(this ApiResponse response) => response.Success;

        /// <summary>
        /// Determines whether the generic response indicates success.
        /// </summary>
        public static bool IsSuccessful<T>(this ApiResponse<T> response) => response.Success;

        /// <summary>
        /// Adds an error message to the response and returns the same instance for chaining.
        /// </summary>
        public static ApiResponse AddError(this ApiResponse response, string error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            response.Errors.Add(error);
            return response;
        }

        /// <summary>
        /// Adds an error message to the generic response and returns the same instance for chaining.
        /// </summary>
        public static ApiResponse<T> AddError<T>(this ApiResponse<T> response, string error)
        {
            if (error == null) throw new ArgumentNullException(nameof(error));
            response.Errors.Add(error);
            return response;
        }

        /// <summary>
        /// Sets the <see cref="ApiResponse{T}.Data"/> property and returns the same instance for chaining.
        /// </summary>
        public static ApiResponse<T> WithData<T>(this ApiResponse<T> response, T data)
        {
            response.Data = data;
            return response;
        }

        /// <summary>
        /// Returns a concise, human‑readable summary of the response.
        /// </summary>
        public static string Summary(this ApiResponse response)
        {
            return $"[{response.Timestamp:u}] RequestId={response.RequestId} Success={response.Success} " +
                   $"Message={response.Message ?? "none"} Errors={response.Errors.Count}";
        }

        /// <summary>
        /// Returns a concise, human‑readable summary of the generic response, including the data type.
        /// </summary>
        public static string Summary<T>(this ApiResponse<T> response)
        {
            var dataInfo = response.Data is null ? "null" : $"type={response.Data.GetType().Name}";
            return $"[{response.Timestamp:u}] RequestId={response.RequestId} Success={response.Success} " +
                   $"Message={response.Message ?? "none"} Errors={response.Errors.Count} Data={dataInfo}";
        }
    }
}

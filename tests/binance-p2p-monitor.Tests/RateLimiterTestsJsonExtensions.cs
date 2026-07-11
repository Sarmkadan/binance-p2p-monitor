using System.Text.Json;

namespace BinanceP2pMonitor.Tests
{
    /// <summary>
    /// JSON serialization helpers for <see cref="RateLimiterTests"/> test data.
    /// </summary>
    public static class RateLimiterTestsJsonExtensions
    {
        // Cached serializer options with camelCase naming.
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Preserve default behavior for other settings.
        };

        /// <summary>
        /// Serializes the <paramref name="value"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="RateLimiterTests"/> instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output will be formatted with indentation.</param>
        /// <returns>A JSON representation of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
        public static string ToJson(this RateLimiterTests value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (indented)
            {
                // Create a copy of the cached options with indentation enabled.
                var indentedOptions = new JsonSerializerOptions(_options)
                {
                    WriteIndented = true
                };
                return JsonSerializer.Serialize(value, indentedOptions);
            }

            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="RateLimiterTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>The deserialized <see cref="RateLimiterTests"/> object, or <c>null</c> if the JSON is <c>null</c>, empty, or whitespace.</returns>
        /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
        public static RateLimiterTests? FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<RateLimiterTests>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="RateLimiterTests"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the deserialized object if the operation succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <c>null</c>.</exception>
        public static bool TryFromJson(string json, out RateLimiterTests? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<RateLimiterTests>(json, _options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}

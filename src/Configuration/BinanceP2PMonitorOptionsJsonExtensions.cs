using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace BinanceP2pMonitor.Configuration
{
    /// <summary>
    /// Provides System.Text.Json serialization/deserialization extensions for <see cref="BinanceP2PMonitorOptions"/>.
    /// </summary>
    public static class BinanceP2PMonitorOptionsJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the <see cref="BinanceP2PMonitorOptions"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The options instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the options.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this BinanceP2PMonitorOptions value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="BinanceP2PMonitorOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized options instance, or null if the JSON is invalid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
        /// <exception cref="JsonException">Thrown when the JSON string is invalid or cannot be deserialized.</exception>
        public static BinanceP2PMonitorOptions? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);

            return JsonSerializer.Deserialize<BinanceP2PMonitorOptions>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="BinanceP2PMonitorOptions"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">Receives the deserialized options instance if successful, otherwise null.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static bool TryFromJson(string json, out BinanceP2PMonitorOptions? value)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentException.ThrowIfNullOrEmpty(json);

            try
            {
                value = JsonSerializer.Deserialize<BinanceP2PMonitorOptions>(json, _jsonSerializerOptions);
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
using System.Text.Json;

namespace BinanceP2pMonitor.Models
{
    /// <summary>
    /// JSON serialization helpers for <see cref="UserProfile"/>.
    /// </summary>
    public static class UserProfileJsonExtensions
    {
        // Cached options with camelCase naming policy.
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Do not write indented by default; indentation is handled per-call.
        };

        /// <summary>
        /// Serializes the <paramref name="value"/> to a JSON string.
        /// </summary>
        /// <param name="value">The <see cref="UserProfile"/> instance to serialize.</param>
        /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
        /// <returns>A JSON representation of the <paramref name="value"/>.</returns>
        public static string ToJson(this UserProfile value, bool indented = false)
        {
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
        /// Deserializes a JSON string into a <see cref="UserProfile"/> instance.
        /// </summary>
        /// <param name="json">The JSON string representing a <see cref="UserProfile"/>.</param>
        /// <returns>The deserialized <see cref="UserProfile"/>, or <c>null</c> if the JSON is empty.</returns>
        public static UserProfile? FromJson(string json)
        {
            return JsonSerializer.Deserialize<UserProfile>(json, _options);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into a <see cref="UserProfile"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">When this method returns, contains the deserialized <see cref="UserProfile"/> if the operation succeeded; otherwise, <c>null</c>.</param>
        /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
        public static bool TryFromJson(string json, out UserProfile? value)
        {
            try
            {
                value = FromJson(json);
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

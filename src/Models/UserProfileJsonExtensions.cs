using System.Text.Json;

namespace BinanceP2pMonitor.Models
{
	/// <summary>
	/// JSON serialization helpers for <see cref="UserProfile"/>.
	/// </summary>
	public static class UserProfileJsonExtensions
	{
		// Cached options with camelCase naming policy.
		private static readonly JsonSerializerOptions _options = new()
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
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
		public static string ToJson(this UserProfile value, bool indented = false)
		{
			ArgumentNullException.ThrowIfNull(value);

			return indented
				? JsonSerializer.Serialize(value, new JsonSerializerOptions(_options) { WriteIndented = true })
				: JsonSerializer.Serialize(value, _options);
		}

		/// <summary>
		/// Deserializes a JSON string into a <see cref="UserProfile"/> instance.
		/// </summary>
		/// <param name="json">The JSON string representing a <see cref="UserProfile"/>. Must not be <c>null</c>.</param>
		/// <returns>The deserialized <see cref="UserProfile"/>, or <c>null</c> if the JSON is empty or invalid.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
		/// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to <see cref="UserProfile"/>.</exception>
		public static UserProfile? FromJson(string json)
		{
			ArgumentNullException.ThrowIfNull(json);
			return JsonSerializer.Deserialize<UserProfile>(json, _options);
		}

		/// <summary>
		/// Attempts to deserialize a JSON string into a <see cref="UserProfile"/> instance.
		/// </summary>
		/// <param name="json">The JSON string to deserialize. Must not be <c>null</c>.</param>
		/// <param name="value">When this method returns, contains the deserialized <see cref="UserProfile"/> if the operation succeeded; otherwise, <c>null</c>.</param>
		/// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
		public static bool TryFromJson(string json, out UserProfile? value)
		{
			ArgumentNullException.ThrowIfNull(json);

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
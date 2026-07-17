using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		WriteIndented = false,
		ReferenceHandler = ReferenceHandler.IgnoreCycles
	};

	/// <summary>
	/// Serializes a <see cref="ValidationException"/> to a JSON string.
	/// </summary>
	/// <param name="value">The exception to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON representation of the exception containing the error messages.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
	public static string ToJson(this ValidationException value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = new JsonSerializerOptions(indented ? _jsonOptions : _jsonOptions)
		{
			WriteIndented = indented
		};

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a <see cref="ValidationException"/> from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized exception, or <see langword="null"/> if the JSON is <see langword="null"/> or empty.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	/// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized.</exception>
	public static ValidationException? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		return string.IsNullOrEmpty(json)
			? null
			: JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a <see cref="ValidationException"/> from a JSON string.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized exception if successful.</param>
	/// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
	public static bool TryFromJson(string json, out ValidationException? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrEmpty(json))
		{
			return true;
		}

		try
		{
			value = JsonSerializer.Deserialize<ValidationException>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
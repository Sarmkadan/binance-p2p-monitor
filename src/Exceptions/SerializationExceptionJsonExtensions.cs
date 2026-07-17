#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="SerializationException"/>
/// </summary>
public static class SerializationExceptionJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
	};

	/// <summary>
	/// Converts a <see cref="SerializationException"/> to its JSON representation
	/// </summary>
	/// <param name="value">The exception to serialize</param>
	/// <param name="indented">Whether to format the JSON with indentation</param>
	/// <returns>A JSON string representation of the exception</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
	public static string ToJson(this SerializationException value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a <see cref="SerializationException"/> from JSON
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <returns>The deserialized exception if successful; otherwise, null</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty</exception>
	public static SerializationException? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			return JsonSerializer.Deserialize<SerializationException>(json, _jsonSerializerOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a <see cref="SerializationException"/> from JSON
	/// </summary>
	/// <param name="json">The JSON string to deserialize</param>
	/// <param name="value">Receives the deserialized exception if successful</param>
	/// <returns>True if deserialization succeeded; otherwise, false</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty</exception>
	public static bool TryFromJson(string json, out SerializationException? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<SerializationException>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
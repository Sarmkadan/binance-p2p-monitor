#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Provides System.Text.Json serialization/deserialization extensions for strings
/// with support for <see cref="StringExtensions"/> extension methods.
/// </summary>
public static class StringExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a string to JSON.
    /// </summary>
    /// <param name="value">The string to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this string value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A string deserialized from JSON, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static string? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<string>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized string if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out string? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<string>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinanceP2pMonitor.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="Spread"/> type.
/// </summary>
public static class SpreadJsonExtensions
{
    // Cached options with camelCase naming policy and optimized settings for JSON serialization.
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Serializes the <paramref name="value"/> to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="Spread"/> instance to serialize.</param>
    /// <param name="indented">If <c>true</c>, the output JSON will be formatted with indentation.</param>
    /// <returns>A JSON representation of the <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this Spread value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(
            value,
            indented
                ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
                : _jsonOptions
        );
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="Spread"/> instance.
    /// </summary>
    /// <param name="json">The JSON string representing a <see cref="Spread"/>. Must not be <c>null</c> or empty.</param>
    /// <returns>The deserialized <see cref="Spread"/>, or <c>null</c> if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to <see cref="Spread"/>.</exception>
    public static Spread? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<Spread>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="Spread"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <c>null</c>.</param>
    /// <param name="value">When this method returns, contains the deserialized <see cref="Spread"/> if the operation succeeded; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
    public static bool TryFromJson(string json, out Spread? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

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
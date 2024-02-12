using System.Text.Json;
using System.Text.Json.Serialization;

namespace BinanceP2PMonitor.Repositories;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="PriceRepository"/>.
/// </summary>
public static class PriceRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="PriceRepository"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The repository instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this PriceRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PriceRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="PriceRepository"/> instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static PriceRepository? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<PriceRepository>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PriceRepository"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out PriceRepository? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<PriceRepository>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
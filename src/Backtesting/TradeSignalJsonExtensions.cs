using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace BinanceP2pMonitor.Backtesting;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="TradeSignal"/>.
/// </summary>
/// <remarks>
/// Uses System.Text.Json with camelCase naming policy for consistent serialization format.
/// All methods are thread-safe and reuse a cached <see cref="JsonSerializerOptions"/> instance.
/// </remarks>
public static class TradeSignalJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    /// <summary>
    /// Serializes the <see cref="TradeSignal"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The trade signal to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the trade signal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this TradeSignal value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="TradeSignal"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized <see cref="TradeSignal"/> instance, or <see langword="null"/> if the JSON is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed or cannot be deserialized.</exception>
    public static TradeSignal? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<TradeSignal>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="TradeSignal"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful; otherwise, <see langword="null"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out TradeSignal? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        value = null;

        try
        {
            value = JsonSerializer.Deserialize<TradeSignal>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

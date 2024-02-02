#nullable enable

using System.Text.Json;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Infrastructure;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for CachedPriceMonitoringService
/// </summary>
public static class CachedPriceMonitoringServiceJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the CachedPriceMonitoringService instance to a JSON string
    /// </summary>
    /// <param name="value">The cached price monitoring service instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the service instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this CachedPriceMonitoringService value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? JsonSerializerOptionsIncremental : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to a CachedPriceMonitoringService instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A CachedPriceMonitoringService instance if successful; otherwise, null</returns>
    public static CachedPriceMonitoringService? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<CachedPriceMonitoringService>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CachedPriceMonitoringService instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">The deserialized CachedPriceMonitoringService instance, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    public static bool TryFromJson(string json, out CachedPriceMonitoringService? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<CachedPriceMonitoringService>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Gets JSON serialization options with indentation enabled
    /// </summary>
    private static JsonSerializerOptions JsonSerializerOptionsIncremental => new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

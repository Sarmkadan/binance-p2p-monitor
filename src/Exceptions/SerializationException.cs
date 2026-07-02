#nullable enable

using System;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Thrown when serialization or deserialization fails
/// </summary>
public class SerializationException : BinanceP2pException
{
    public string? DataType { get; set; }

    public SerializationException(string message, string? dataType = null, string? errorCode = "SERIALIZATION_ERROR")
        : base(message, errorCode)
    {
        DataType = dataType;
    }

    public SerializationException(string message, Exception innerException, string? dataType = null, string? errorCode = "SERIALIZATION_ERROR")
        : base(message, innerException, errorCode)
    {
        DataType = dataType;
    }

    public override string ToString()
    {
        var baseStr = base.ToString();
        var dataTypeInfo = DataType is not null ? $"\nDataType: {DataType}" : string.Empty;
        return $"{baseStr}{dataTypeInfo}";
    }
}

/// <summary>
/// Thrown when JSON serialization/deserialization fails
/// </summary>
public class JsonSerializationException : SerializationException
{
    public string? JsonContent { get; set; }

    public JsonSerializationException(string message, string? jsonContent = null, string? errorCode = "JSON_ERROR")
        : base(message, errorCode)
    {
        JsonContent = jsonContent;
        DataType = "JSON";
    }

    public JsonSerializationException(string message, Exception innerException, string? jsonContent = null, string? errorCode = "JSON_ERROR")
        : base(message, innerException, errorCode)
    {
        JsonContent = jsonContent;
        DataType = "JSON";
    }

    public override string ToString()
    {
        var baseStr = base.ToString();
        var jsonInfo = JsonContent is not null ? $"\nJsonPreview: {JsonContent[..Math.Min(200, JsonContent.Length)]}{(JsonContent.Length > 200 ? "..." : "")}" : string.Empty;
        return $"{baseStr}{jsonInfo}";
    }
}

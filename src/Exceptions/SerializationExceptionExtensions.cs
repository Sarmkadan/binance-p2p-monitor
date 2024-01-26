using BinanceP2pMonitor.Exceptions;

namespace BinanceP2pMonitor.Exceptions;

public static class SerializationExceptionExtensions
{
    public static bool IsJsonSerializationException(this SerializationException exception)
    {
        return exception is JsonSerializationException;
    }

    public static string GetDetailedMessage(this SerializationException exception)
    {
        return $"Serialization error: {exception.Message}, DataType: {exception.DataType}, JsonContent: {(exception as JsonSerializationException)?.JsonContent}";
    }

    public static bool HasJsonContent(this SerializationException exception)
    {
        return exception is JsonSerializationException jsonException && jsonException.JsonContent != null;
    }
}

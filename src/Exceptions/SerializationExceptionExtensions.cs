using BinanceP2pMonitor.Exceptions;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="SerializationException"/> and its derived types
/// </summary>
public static class SerializationExceptionExtensions
{
	/// <summary>
	/// Determines whether the exception is a <see cref="JsonSerializationException"/>
	/// </summary>
	/// <param name="exception">The exception to check</param>
	/// <returns>True if the exception is a JsonSerializationException; otherwise, false</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
	public static bool IsJsonSerializationException(this SerializationException exception)
	{
		ArgumentNullException.ThrowIfNull(exception);
		return exception is JsonSerializationException;
	}

	/// <summary>
	/// Gets a detailed error message including the exception message, data type, and JSON content (if available)
	/// </summary>
	/// <param name="exception">The serialization exception</param>
	/// <returns>A formatted error message with detailed information</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
	public static string GetDetailedMessage(this SerializationException exception)
	{
		ArgumentNullException.ThrowIfNull(exception);

		return exception switch
		{
			JsonSerializationException jsonEx => $"Serialization error: {jsonEx.Message}, DataType: {jsonEx.DataType}, JsonContent: {jsonEx.JsonContent}",
			_ => $"Serialization error: {exception.Message}, DataType: {exception.DataType}"
		};
	}

	/// <summary>
	/// Determines whether the exception contains JSON content
	/// </summary>
	/// <param name="exception">The serialization exception to check</param>
	/// <returns>True if the exception is a JsonSerializationException with non-null JsonContent; otherwise, false</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null</exception>
	public static bool HasJsonContent(this SerializationException exception)
	{
		ArgumentNullException.ThrowIfNull(exception);
		return exception is JsonSerializationException jsonException && jsonException.JsonContent is not null;
	}
}

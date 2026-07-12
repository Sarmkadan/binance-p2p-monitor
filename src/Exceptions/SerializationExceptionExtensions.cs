using BinanceP2pMonitor.Exceptions;

namespace BinanceP2pMonitor.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="SerializationException"/> and its derived types
/// to extract detailed information about serialization errors.
/// </summary>
public static class SerializationExceptionExtensions
{
	/// <summary>
	/// Determines whether the specified exception is of type <see cref="JsonSerializationException"/>.
	/// </summary>
	/// <param name="exception">The <see cref="SerializationException"/> to check.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="exception"/> is a <see cref="JsonSerializationException"/>; 
	/// otherwise, <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="exception"/> is <c>null</c>.
	/// </exception>
	public static bool IsJsonSerializationException(this SerializationException exception)
	{
		ArgumentNullException.ThrowIfNull(exception);
		return exception is JsonSerializationException;
	}

	/// <summary>
	/// Gets a detailed error message that includes the exception message, data type, and JSON content
	/// (if available) for the specified <see cref="SerializationException"/>.
	/// </summary>
	/// <param name="exception">The <see cref="SerializationException"/> to process.</param>
	/// <returns>
	/// A formatted string containing detailed error information. For <see cref="JsonSerializationException"/>,
	/// includes <see cref="JsonSerializationException.DataType"/> and <see cref="JsonSerializationException.JsonContent"/>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="exception"/> is <c>null</c>.
	/// </exception>
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
	/// Determines whether the specified <see cref="SerializationException"/> contains non-null JSON content.
	/// </summary>
	/// <param name="exception">The <see cref="SerializationException"/> to check.</param>
	/// <returns>
	/// <c>true</c> if <paramref name="exception"/> is a <see cref="JsonSerializationException"/> 
	/// and <see cref="JsonSerializationException.JsonContent"/> is not <c>null</c>; otherwise, <c>false</c>.
	/// </returns>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="exception"/> is <c>null</c>.
	/// </exception>
	public static bool HasJsonContent(this SerializationException exception)
	{
		ArgumentNullException.ThrowIfNull(exception);
		return exception is JsonSerializationException jsonException && jsonException.JsonContent is not null;
	}
}

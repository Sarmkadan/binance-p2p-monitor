using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BinanceP2pMonitor.Integration
{
	/// <summary>
	/// Provides extension methods for <see cref="HttpClientFactory"/> to simplify HTTP requests.
	/// </summary>
	public static class HttpClientFactoryExtensions
	{
		/// <summary>
		/// Creates an <see cref="HttpClient"/> for the specified <paramref name="baseUrl"/> and
		/// performs a GET request for the given <paramref name="relativePath"/>, returning the raw response string.
		/// </summary>
		/// <param name="factory">The <see cref="HttpClientFactory"/> instance.</param>
		/// <param name="baseUrl">The base URL of the API (e.g., <c>https://api.example.com</c>).</param>
		/// <param name="relativePath">The relative path to request (e.g., <c>/v1/status</c>).</param>
		/// <returns>The response body as a string.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="factory"/>, <paramref name="baseUrl"/>, or <paramref name="relativePath"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="baseUrl"/> or <paramref name="relativePath"/> is <see cref="string.IsNullOrEmpty"/>.
		/// </exception>
		/// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
		/// <exception cref="UriFormatException">
		/// Thrown when <paramref name="baseUrl"/> is not a valid absolute URI.
		/// </exception>
		public static async Task<string> GetStringAsync(this HttpClientFactory factory, string baseUrl, string relativePath)
		{
			ArgumentNullException.ThrowIfNull(factory);
			ArgumentException.ThrowIfNullOrEmpty(baseUrl);
			ArgumentException.ThrowIfNullOrEmpty(relativePath);

			using var client = factory.CreateApiClient(baseUrl);
			return await client.GetStringAsync(relativePath).ConfigureAwait(false);
		}

		/// <summary>
		/// Sends a GET request to <paramref name="relativePath"/> on the API identified by <paramref name="baseUrl"/>
		/// and deserializes the JSON response to an instance of <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type to deserialize the JSON payload into.</typeparam>
		/// <param name="factory">The <see cref="HttpClientFactory"/> instance.</param>
		/// <param name="baseUrl">The base URL of the API.</param>
		/// <param name="relativePath">The relative path to request.</param>
		/// <returns>An instance of <typeparamref name="T"/> if deserialization succeeds; otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="factory"/>, <paramref name="baseUrl"/>, or <paramref name="relativePath"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="baseUrl"/> or <paramref name="relativePath"/> is <see cref="string.IsNullOrEmpty"/>.
		/// </exception>
		/// <exception cref="HttpRequestException">Thrown when the HTTP request fails.</exception>
		/// <exception cref="JsonException">
		/// Thrown when JSON deserialization fails.
		/// </exception>
		public static async Task<T?> GetJsonAsync<T>(this HttpClientFactory factory, string baseUrl, string relativePath)
		{
			ArgumentNullException.ThrowIfNull(factory);
			ArgumentException.ThrowIfNullOrEmpty(baseUrl);
			ArgumentException.ThrowIfNullOrEmpty(relativePath);

			var json = await factory.GetStringAsync(baseUrl, relativePath).ConfigureAwait(false);
			return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}

		/// <summary>
		/// Sends a POST request with a JSON payload to <paramref name="relativePath"/> on the API identified by <paramref name="baseUrl"/>
		/// and deserializes the JSON response to an instance of <typeparamref name="TResponse"/>.
		/// </summary>
		/// <typeparam name="TResponse">The type to deserialize the JSON response into.</typeparam>
		/// <param name="factory">The <see cref="HttpClientFactory"/> instance.</param>
		/// <param name="baseUrl">The base URL of the API.</param>
		/// <param name="relativePath">The relative path to post to.</param>
		/// <param name="payload">The object that will be serialized to JSON and sent as the request body.</param>
		/// <returns>An instance of <typeparamref name="TResponse"/> if deserialization succeeds; otherwise <c>null</c>.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when <paramref name="factory"/>, <paramref name="baseUrl"/>, <paramref name="relativePath"/>, or <paramref name="payload"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when <paramref name="baseUrl"/> or <paramref name="relativePath"/> is <see cref="string.IsNullOrEmpty"/>.
		/// </exception>
		/// <exception cref="HttpRequestException">
		/// Thrown when the HTTP request fails or the response status code indicates an error.
		/// </exception>
		/// <exception cref="JsonException">
		/// Thrown when JSON serialization of the payload or deserialization of the response fails.
		/// </exception>
		public static async Task<TResponse?> PostJsonAsync<TResponse>(this HttpClientFactory factory, string baseUrl, string relativePath, object payload)
		{
			ArgumentNullException.ThrowIfNull(factory);
			ArgumentException.ThrowIfNullOrEmpty(baseUrl);
			ArgumentException.ThrowIfNullOrEmpty(relativePath);
			ArgumentNullException.ThrowIfNull(payload);

			using var client = factory.CreateApiClient(baseUrl);
			var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
			using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
			using var response = await client.PostAsync(relativePath, content).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();

			var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonSerializer.Deserialize<TResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}
	}
}

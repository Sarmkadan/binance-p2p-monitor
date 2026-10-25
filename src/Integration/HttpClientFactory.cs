#nullable enable
namespace BinanceP2pMonitor.Integration;

/// <summary>
/// Factory for creating configured HTTP clients with standard headers and policies
/// </summary>
public class HttpClientFactory
{
    private readonly ILogger<HttpClientFactory> _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactory"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client used for all requests created by this factory</param>
    /// <param name="logger">The logger used to record request activity and failures</param>
    /// <exception cref="ArgumentNullException"><paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
    public HttpClientFactory(HttpClient httpClient, ILogger<HttpClientFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Creates a configured HTTP client for API calls
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="baseUrl"/> is <see langword="null"/>.</exception>
    public HttpClient CreateApiClient(string baseUrl)
    {
        ArgumentNullException.ThrowIfNull(baseUrl);

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BinanceP2pMonitor/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        _logger.LogDebug("Created API client for {BaseUrl}", baseUrl);

        return _httpClient;
    }

    /// <summary>
    /// Sends a GET request and returns deserialized response
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="url"/> is <see langword="null"/>.</exception>
    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            _logger.LogDebug("GET request to {Url}", url);
            var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<T>(content);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request failed for {Url}", url);
            throw new ApiException($"GET request failed for {url}", ex);
        }
    }

    /// <summary>
    /// Sends a POST request with JSON body
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="url"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
    public async Task<T?> PostAsync<T>(string url, object data, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(data);

        try
        {
            _logger.LogDebug("POST request to {Url}", url);
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<T>(responseContent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request failed for {Url}", url);
            throw new ApiException($"POST request failed for {url}", ex);
        }
    }

    /// <summary>
    /// Gets raw response as string
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="url"/> is <see langword="null"/>.</exception>
    public async Task<string> GetStringAsync(string url, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(url);

        try
        {
            return await _httpClient.GetStringAsync(url, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET string request failed for {Url}", url);
            throw new ApiException($"GET string request failed for {url}", ex);
        }
    }
}

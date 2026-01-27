#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Integration;

/// <summary>
/// Factory for creating configured HTTP clients with standard headers and policies
/// </summary>
public class HttpClientFactory
{
    private readonly ILogger<HttpClientFactory> _logger;
    private readonly HttpClient _httpClient;

    public HttpClientFactory(HttpClient httpClient, ILogger<HttpClientFactory> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Creates a configured HTTP client for API calls
    /// </summary>
    public HttpClient CreateApiClient(string baseUrl)
    {
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
    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("GET request to {Url}", url);
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<T>(content);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request failed for {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Sends a POST request with JSON body
    /// </summary>
    public async Task<T?> PostAsync<T>(string url, object data, CancellationToken ct = default)
    {
        try
        {
            _logger.LogDebug("POST request to {Url}", url);
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, ct);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<T>(responseContent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request failed for {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Gets raw response as string
    /// </summary>
    public async Task<string> GetStringAsync(string url, CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetStringAsync(url, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET string request failed for {Url}", url);
            throw;
        }
    }
}

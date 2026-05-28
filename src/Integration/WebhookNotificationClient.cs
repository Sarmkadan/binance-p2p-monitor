#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http;
using System.Text;
using System.Text.Json;
using BinanceP2pMonitor.Configuration;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Integration;

public interface IWebhookNotificationClient
{
    Task<bool> SendAlertAsync(WebhookPayload payload, CancellationToken ct = default);
    Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default);
}

public class WebhookNotificationClient : IWebhookNotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _appSettings;
    private readonly ILogger<WebhookNotificationClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public WebhookNotificationClient(
        IHttpClientFactory httpClientFactory,
        AppSettings appSettings,
        ILogger<WebhookNotificationClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient(nameof(WebhookNotificationClient));
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Posts a generic alert payload to the configured webhook URL.
    /// Returns true on HTTP 2xx response, false otherwise.
    /// </summary>
    public async Task<bool> SendAlertAsync(WebhookPayload payload, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_appSettings.WebhookUrl))
        {
            _logger.LogDebug("Webhook URL is not configured; skipping webhook delivery");
            return false;
        }

        try
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug("Sending webhook alert to {Url}", _appSettings.WebhookUrl);
            var response = await _httpClient.PostAsync(_appSettings.WebhookUrl, content, ct).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook delivered successfully (HTTP {StatusCode})", (int)response.StatusCode);
                return true;
            }

            _logger.LogWarning("Webhook endpoint returned non-success status {StatusCode}", (int)response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver webhook to {Url}", _appSettings.WebhookUrl);
            return false;
        }
    }

    /// <summary>
    /// Convenience overload for price-alert events
    /// </summary>
    public Task<bool> SendPriceAlertAsync(
        string asset,
        string fiat,
        decimal buyPrice,
        decimal sellPrice,
        string alertReason,
        CancellationToken ct = default)
    {
        var payload = new WebhookPayload
        {
            Event = "price_alert",
            Asset = asset,
            Fiat = fiat,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            AlertReason = alertReason,
            Timestamp = DateTimeOffset.UtcNow
        };

        return SendAlertAsync(payload, ct);
    }
}

/// <summary>
/// JSON payload POSTed to the webhook endpoint on each alert
/// </summary>
public sealed class WebhookPayload
{
    public string Event { get; set; } = "alert";
    public string Asset { get; set; } = string.Empty;
    public string Fiat { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string AlertReason { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public string? CustomData { get; set; }
}

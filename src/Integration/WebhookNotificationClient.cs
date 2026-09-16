#nullable enable
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BinanceP2pMonitor.Configuration;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Integration;

/// <summary>
/// Defines operations for sending notifications to a webhook endpoint.
/// </summary>
public interface IWebhookNotificationClient
{
    /// <summary>
    /// Sends an alert payload to the configured webhook endpoint.
    /// </summary>
    /// <param name="payload">The alert payload to send.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><see langword="true"/> if the alert was delivered successfully; otherwise, <see langword="false"/>.</returns>
    Task<bool> SendAlertAsync(WebhookPayload payload, CancellationToken ct = default);

    /// <summary>
    /// Sends a price alert to the configured webhook endpoint.
    /// </summary>
    /// <param name="asset">The asset being monitored.</param>
    /// <param name="fiat">The fiat currency used to price the asset.</param>
    /// <param name="buyPrice">The current buy price.</param>
    /// <param name="sellPrice">The current sell price.</param>
    /// <param name="alertReason">The reason the alert was triggered.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><see langword="true"/> if the alert was delivered successfully; otherwise, <see langword="false"/>.</returns>
    Task<bool> SendPriceAlertAsync(string asset, string fiat, decimal buyPrice, decimal sellPrice, string alertReason, CancellationToken ct = default);
}

/// <summary>
/// Sends alert notifications to a configured webhook endpoint.
/// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookNotificationClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the HTTP client.</param>
    /// <param name="appSettings">The application settings containing the webhook configuration.</param>
    /// <param name="logger">The logger used to record webhook delivery activity.</param>
    public WebhookNotificationClient(
        IHttpClientFactory httpClientFactory,
        AppSettings appSettings,
        ILogger<WebhookNotificationClient> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);

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
        ArgumentNullException.ThrowIfNull(payload);

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
        ArgumentNullException.ThrowIfNull(asset);
        ArgumentNullException.ThrowIfNull(fiat);
        ArgumentNullException.ThrowIfNull(alertReason);

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
    /// <summary>
    /// Gets or sets the name of the webhook event.
    /// </summary>
    public string Event { get; set; } = "alert";

    /// <summary>
    /// Gets or sets the asset associated with the alert.
    /// </summary>
    public string Asset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fiat currency used to price the asset.
    /// </summary>
    public string Fiat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the buy price associated with the alert.
    /// </summary>
    public decimal BuyPrice { get; set; }

    /// <summary>
    /// Gets or sets the sell price associated with the alert.
    /// </summary>
    public decimal SellPrice { get; set; }

    /// <summary>
    /// Gets or sets the reason the alert was triggered.
    /// </summary>
    public string AlertReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time at which the alert was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets optional custom data associated with the alert.
    /// </summary>
    public string? CustomData { get; set; }
}

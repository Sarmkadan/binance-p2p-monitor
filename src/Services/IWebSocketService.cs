#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Services;

/// <summary>
/// Service interface for WebSocket real-time price feeds
/// </summary>
public interface IWebSocketService
{
    Task ConnectAsync();
    Task DisconnectAsync();
    bool IsConnected { get; }
    event EventHandler<PriceUpdateEventArgs>? OnPriceUpdate;
    Task SubscribeToPairAsync(string asset, string fiat);
    Task UnsubscribeFromPairAsync(string asset, string fiat);
}

/// <summary>
/// Event arguments for price updates
/// </summary>
public class PriceUpdateEventArgs : EventArgs
{
    public string Asset { get; set; } = string.Empty;
    public string Fiat { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public DateTime UpdateTime { get; set; }
}

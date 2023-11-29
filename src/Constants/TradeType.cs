#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Constants;

/// <summary>
/// Enumeration for trade types in P2P trading
/// </summary>
public enum TradeType
{
    Unknown = 0,
    Buy = 1,
    Sell = 2,
    BuyAndSell = 3
}

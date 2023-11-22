// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Constants;

/// <summary>
/// Enumeration for types of price alerts
/// </summary>
public enum AlertType
{
    Unknown = 0,
    PriceChange = 1,
    SpreadAlert = 2,
    VolumeAlert = 3,
    OfferAlert = 4,
    HighSpreadAlert = 5,
    LowSpreadAlert = 6
}

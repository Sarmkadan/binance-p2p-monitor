// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Constants;

/// <summary>
/// Enumeration for alert condition operators
/// </summary>
public enum AlertCondition
{
    Unknown = 0,
    GreaterThan = 1,
    LessThan = 2,
    Equals = 3,
    GreaterThanOrEqual = 4,
    LessThanOrEqual = 5
}

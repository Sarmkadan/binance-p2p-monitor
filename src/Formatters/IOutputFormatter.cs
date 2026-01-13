#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Interface for formatting output in different formats
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Supported format name (e.g., "json", "csv", "table")
    /// </summary>
    string FormatType { get; }

    /// <summary>
    /// Formats a single object
    /// </summary>
    string Format(object? data);

    /// <summary>
    /// Formats a collection of objects
    /// </summary>
    string Format(IEnumerable<object> data);

    /// <summary>
    /// Formats a collection of objects with custom headers
    /// </summary>
    string Format(IEnumerable<object> data, IEnumerable<string> headers);
}

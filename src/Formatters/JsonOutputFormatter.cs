// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Formats output as JSON
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    public string FormatType => "json";

    public string Format(object? data)
    {
        if (data is null)
            return "null";

        try
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { error = ex.Message });
        }
    }

    public string Format(IEnumerable<object> data)
    {
        try
        {
            var list = data.ToList();
            return JsonConvert.SerializeObject(list, Formatting.Indented);
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { error = ex.Message });
        }
    }

    public string Format(IEnumerable<object> data, IEnumerable<string> headers)
    {
        try
        {
            var list = data.ToList();
            return JsonConvert.SerializeObject(new { headers = headers, data = list }, Formatting.Indented);
        }
        catch (Exception ex)
        {
            return JsonConvert.SerializeObject(new { error = ex.Message });
        }
    }
}

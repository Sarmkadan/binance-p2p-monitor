#nullable enable
namespace BinanceP2pMonitor.Formatters;

/// <summary>
/// Formats output as JSON
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    private static readonly System.Text.Json.JsonSerializerOptions _indented = new() { WriteIndented = true };

    public string FormatType => "json";

    public string Format(object? data)
    {
        if (data is null)
            return "null";

        try
        {
            return JsonSerializer.Serialize(data, _indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    public string Format(IEnumerable<object> data)
    {
        try
        {
            return JsonSerializer.Serialize(data.ToList(), _indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    public string Format(IEnumerable<object> data, IEnumerable<string> headers)
    {
        try
        {
            return JsonSerializer.Serialize(new { headers, data = data.ToList() }, _indented);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}

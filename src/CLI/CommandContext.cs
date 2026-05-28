#nullable enable
namespace BinanceP2pMonitor.CLI;

/// <summary>
/// Context for command execution containing parsed arguments and configuration
/// </summary>
public class CommandContext
{
    public string CommandName { get; set; } = string.Empty;
    public string[] Arguments { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> Options { get; set; } = new();
    public Dictionary<string, string> Flags { get; set; } = new();
    public IServiceProvider ServiceProvider { get; set; } = null!;
    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public bool HasOption(string key) => Options.ContainsKey(key);
    public bool HasFlag(string key) => Flags.ContainsKey(key);

    public string? GetOption(string key)
    {
        return Options.TryGetValue(key, out var value) ? value : null;
    }

    public string GetOption(string key, string defaultValue)
    {
        return Options.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public T? GetService<T>() where T : notnull
    {
        return (T?)ServiceProvider.GetService(typeof(T));
    }

    public T GetRequiredService<T>() where T : notnull
    {
        var service = ServiceProvider.GetService(typeof(T));
        return service is null
            ? throw new InvalidOperationException($"Service of type {typeof(T).Name} not found")
            : (T)service;
    }
}

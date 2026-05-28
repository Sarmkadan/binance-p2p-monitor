#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for IEnumerable operations
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Safely gets first item or returns null
    /// </summary>
    public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : class
    {
        return source.FirstOrDefault();
    }

    /// <summary>
    /// Batches items by a key selector and yields when predicate returns true
    /// </summary>
    public static IEnumerable<List<T>> BatchWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var batch = new List<T>();
        foreach (var item in source)
        {
            if (!predicate(item) && batch.Any())
            {
                yield return batch;
                batch = new List<T>();
            }
            batch.Add(item);
        }

        if (batch.Any())
            yield return batch;
    }

    /// <summary>
    /// Performs an action on each item and returns the sequence
    /// </summary>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Checks if any items match the predicate, with a minimum count
    /// </summary>
    public static bool AnyCount<T>(this IEnumerable<T> source, int minCount, Func<T, bool> predicate)
    {
        var count = 0;
        foreach (var item in source)
        {
            if (predicate(item))
            {
                count++;
                if (count >= minCount)
                    return true;
            }
        }
        return false;
    }

}

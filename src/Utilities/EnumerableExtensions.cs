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
    /// Chunks enumerable into batches of specified size
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int size)
    {
        if (size <= 0)
            throw new ArgumentException("Chunk size must be greater than zero", nameof(size));

        var batch = new List<T>(size);
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

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

    /// <summary>
    /// Returns distinct items by a key selector
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seen.Add(key))
                yield return item;
        }
    }
}

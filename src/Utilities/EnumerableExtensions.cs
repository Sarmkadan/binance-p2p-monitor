#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace BinanceP2pMonitor.Utilities;

/// <summary>
/// Extension methods for IEnumerable operations
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Safely gets first item or returns null
    /// </summary>
    /// <typeparam name="T">The element type of the sequence</typeparam>
    /// <param name="source">The source sequence</param>
    /// <returns>The first element or null if the sequence is empty</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is null</exception>
    public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : class
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.FirstOrDefault();
    }

    /// <summary>
    /// Batches items while the predicate returns true for the current item.
    /// When the predicate returns false, the current batch is yielded and a new batch is started.
    /// </summary>
    /// <typeparam name="T">The element type of the sequence</typeparam>
    /// <param name="source">The source sequence</param>
    /// <param name="predicate">Function to determine if the current item should continue the current batch</param>
    /// <returns>Batches of items where each batch contains consecutive items satisfying the predicate</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="predicate"/> is null</exception>
    public static IEnumerable<IReadOnlyList<T>> BatchWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        return BatchWhileCore(source, predicate);
    }

    private static IEnumerable<IReadOnlyList<T>> BatchWhileCore<T>(IEnumerable<T> source, Func<T, bool> predicate)
    {
        var batch = new List<T>();
        foreach (var item in source)
        {
            if (!predicate(item) && batch.Count > 0)
            {
                yield return batch.AsReadOnly();
                batch = new List<T>();
            }
            batch.Add(item);
        }

        if (batch.Count > 0)
        {
            yield return batch.AsReadOnly();
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the sequence and returns the sequence.
    /// </summary>
    /// <typeparam name="T">The element type of the sequence</typeparam>
    /// <param name="source">The source sequence</param>
    /// <param name="action">Action to perform on each element</param>
    /// <returns>The original sequence for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="action"/> is null</exception>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in source)
        {
            action(item);
            yield return item;
        }
    }

    /// <summary>
    /// Determines whether any items in the sequence match the predicate, with a minimum count requirement.
    /// </summary>
    /// <typeparam name="T">The element type of the sequence</typeparam>
    /// <param name="source">The source sequence</param>
    /// <param name="minCount">Minimum number of matching items required</param>
    /// <param name="predicate">Function to test each element</param>
    /// <returns>True if at least <paramref name="minCount"/> items match the predicate; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="predicate"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minCount"/> is negative</exception>
    public static bool AnyCount<T>(this IEnumerable<T> source, int minCount, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentOutOfRangeException.ThrowIfNegative(minCount);

        if (minCount == 0)
        {
            return true;
        }

        var count = 0;
        foreach (var item in source)
        {
            if (predicate(item))
            {
                count++;
                if (count >= minCount)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
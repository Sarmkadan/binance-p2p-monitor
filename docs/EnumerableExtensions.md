# EnumerableExtensions
The `EnumerableExtensions` class provides a set of extension methods for working with enumerables in C#. It offers various methods to simplify common operations, such as retrieving the first element or null, batching elements, iterating over a collection, and checking for the existence of elements. These extensions aim to make working with enumerables more efficient and expressive.

## API
* `public static T? FirstOrNull<T>(this IEnumerable<T> source)`: Retrieves the first element of the source enumerable, or null if the source is empty. This method does not throw any exceptions, but will return the default value of `T` (which is null for reference types) if the source is empty.
* `public static IEnumerable<List<T>> BatchWhile<T>(this IEnumerable<T> source)`: Batches elements from the source enumerable into lists based on a condition. The method returns an enumerable of lists, where each list contains consecutive elements that meet the condition. This method does not throw any exceptions.
* `public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source)`: Iterates over the source enumerable and returns the same enumerable. This method does not throw any exceptions.
* `public static bool AnyCount<T>(this IEnumerable<T> source)`: Checks if the source enumerable contains any elements. This method returns true if the source is not empty, and false otherwise. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `EnumerableExtensions` class:
```csharp
// Example 1: Retrieving the first element or null
var numbers = new List<int> { 1, 2, 3 };
var firstNumber = numbers.FirstOrNull();
Console.WriteLine(firstNumber); // Output: 1

// Example 2: Batching elements
var words = new List<string> { "apple", "banana", "cherry", "date", "elderberry" };
var batches = words.BatchWhile(w => w.Length > 5);
foreach (var batch in batches)
{
    Console.WriteLine(string.Join(", ", batch));
}
// Output:
// apple, banana
// cherry, date, elderberry
```

## Notes
When using the `BatchWhile` method, be aware that the batching condition is evaluated for each element in the source enumerable. This can lead to performance issues if the condition is complex or if the source enumerable is very large. Additionally, the `ForEach` method does not modify the original enumerable and only returns the same enumerable, making it suitable for chaining method calls. The `EnumerableExtensions` class is designed to be thread-safe, as it only operates on enumerables and does not maintain any internal state. However, the thread-safety of the methods depends on the thread-safety of the underlying enumerables and the batching condition.

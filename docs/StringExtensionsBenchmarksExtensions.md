# StringExtensionsBenchmarksExtensions

Provides a set of static utility methods for string manipulation and validation, primarily focused on casing conventions, truncation, and formatting. These methods are designed to support benchmarking scenarios and general-purpose string processing within the `binance-p2p-monitor` project.

## API

### `CountWordsFromCamelCase`

```csharp
public static int CountWordsFromCamelCase(this string input)
```

Counts the number of words in a camelCase or PascalCase string by detecting uppercase letters that mark word boundaries. A single lowercase word returns `1`, and an empty or null string returns `0`.

**Parameters:**
- `input` (`string`): The camelCase or PascalCase string to analyze.

**Return Value:**
- `int`: The number of words identified in the input string.

**Exceptions:**
- None. Null input is handled gracefully and returns `0`.

---

### `IsValidSnakeCase`

```csharp
public static bool IsValidSnakeCase(this string input)
```

Determines whether a string conforms to the snake_case convention: all characters are lowercase letters, digits, or underscores; the string does not start or end with an underscore; and it does not contain consecutive underscores.

**Parameters:**
- `input` (`string`): The string to validate.

**Return Value:**
- `bool`: `true` if the string is valid snake_case; otherwise `false`.

**Exceptions:**
- None. Null or empty input returns `false`.

---

### `TruncateWithEllipsis`

```csharp
public static string TruncateWithEllipsis(this string input, int maxLength)
```

Truncates a string to a specified maximum length and appends an ellipsis (`...`) if truncation occurs. If the input length is less than or equal to `maxLength`, the original string is returned unchanged. The ellipsis itself counts toward the `maxLength`.

**Parameters:**
- `input` (`string`): The string to potentially truncate.
- `maxLength` (`int`): The maximum allowed length, including the ellipsis if appended.

**Return Value:**
- `string`: The truncated string with ellipsis, or the original string if no truncation is needed.

**Exceptions:**
- `ArgumentOutOfRangeException`: Thrown when `maxLength` is less than the length of the ellipsis (3 characters), as truncation would be impossible.

---

### `ToTitleCase`

```csharp
public static string ToTitleCase(this string input)
```

Converts a string to title case, where the first character of each word is uppercase and the remaining characters are lowercase. Words are identified by whitespace separators.

**Parameters:**
- `input` (`string`): The string to convert.

**Return Value:**
- `string`: The title-cased string.

**Exceptions:**
- None. Null or empty input returns `string.Empty`.

## Usage

### Example 1: Validating and Formatting Identifiers

```csharp
string apiField = "last_trade_price";

if (apiField.IsValidSnakeCase())
{
    int wordCount = apiField.CountWordsFromCamelCase(); // Returns 1 (no camelCase boundaries)
    string displayName = apiField.ToTitleCase();         // "Last Trade Price"
    Console.WriteLine($"Field '{apiField}' contains {wordCount} word(s). Display: {displayName}");
}
else
{
    Console.WriteLine("Invalid snake_case identifier.");
}
```

### Example 2: Truncating Log Messages

```csharp
string logEntry = "Order executed successfully at 2025-01-15T10:30:00Z with transaction ID 0x9a3f2b1c";
string truncated = logEntry.TruncateWithEllipsis(50);

Console.WriteLine(truncated);
// Output: "Order executed successfully at 2025-01-15T10:..."

string shortEntry = "Connection established.";
string unchanged = shortEntry.TruncateWithEllipsis(50);

Console.WriteLine(unchanged);
// Output: "Connection established."
```

## Notes

- **Null Handling:** All methods accept `null` input without throwing `NullReferenceException`. `CountWordsFromCamelCase` returns `0`, `IsValidSnakeCase` returns `false`, `TruncateWithEllipsis` returns `null`, and `ToTitleCase` returns `string.Empty`.
- **`TruncateWithEllipsis` Edge Cases:** When `maxLength` is exactly 3, the result is always `"..."` regardless of input length. When `maxLength` is less than 3, an `ArgumentOutOfRangeException` is thrown. The method does not break on word boundaries; truncation is purely character-based.
- **`CountWordsFromCamelCase` Behavior:** A string consisting entirely of uppercase letters (e.g., `"ABC"`) is counted as a single word. Acronyms followed by lowercase letters (e.g., `"XMLParser"`) count as two words, splitting at the last uppercase letter before the lowercase sequence.
- **`IsValidSnakeCase` Validation Rules:** Strings containing any uppercase letters, leading/trailing underscores, or double underscores (`__`) are rejected. Digits are permitted anywhere except as the first character if it would violate the no-leading-underscore rule (digits are allowed at the start).
- **`ToTitleCase` Culture:** The method uses the current culture's casing rules. For invariant results across environments, ensure the executing thread's culture is set appropriately or consider that behavior may vary.
- **Thread Safety:** All methods are static, operate only on their input parameters, and maintain no shared state. They are safe to call concurrently from multiple threads without synchronization.

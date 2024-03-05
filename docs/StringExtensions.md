# StringExtensions

Provides a set of extension methods for the `string` type, designed to simplify common string manipulation, validation, and conversion tasks within the `binance-p2p-monitor` project. All methods are static and operate on the input string without modifying it, returning new strings or nullable value types as appropriate.

## API

### `Truncate`
```csharp
public static string Truncate(this string value, int maxLength, string suffix = "...")
```
Truncates the string to a specified maximum length, appending an optional suffix (default `"..."`) if the original string exceeds that length.  
**Parameters:**  
- `value` – The input string.  
- `maxLength` – Maximum number of characters to retain (including the suffix).  
- `suffix` – String appended when truncation occurs (default `"..."`).  

**Returns:** The original string if its length is less than or equal to `maxLength`; otherwise, the first `maxLength - suffix.Length` characters followed by the suffix.  
**Throws:** `ArgumentNullException` if `value` is `null`.  
`ArgumentOutOfRangeException` if `maxLength` is less than zero, or if `maxLength` is less than the length of `suffix` (making truncation impossible).

### `SplitCamelCase`
```csharp
public static string SplitCamelCase(this string value)
```
Inserts spaces before each uppercase letter in a camelCase or PascalCase string, effectively splitting it into separate words.  
**Parameters:**  
- `value` – The input string.  

**Returns:** A new string with spaces inserted before each uppercase character (except the first character if the string starts with uppercase). Consecutive uppercase letters are treated as a single acronym.  
**Throws:** `ArgumentNullException` if `value` is `null`.

### `ToSnakeCase`
```csharp
public static string ToSnakeCase(this string value)
```
Converts a camelCase or PascalCase string to snake_case by inserting underscores before uppercase letters and converting all characters to lowercase.  
**Parameters:**  
- `value` – The input string.  

**Returns:** A new string in snake_case format.  
**Throws:** `ArgumentNullException` if `value` is `null`.

### `ToPascalCase`
```csharp
public static string ToPascalCase(this string value)
```
Converts a string (e.g., snake_case, kebab-case, or space-separated words) to PascalCase by capitalizing the first letter of each word and removing separators.  
**Parameters:**  
- `value` – The input string.  

**Returns:** A new string in PascalCase format.  
**Throws:** `ArgumentNullException` if `value` is `null`.

### `ContainsAny`
```csharp
public static bool ContainsAny(this string value, params string[] values)
```
Determines whether the string contains any of the specified substrings.  
**Parameters:**  
- `value` – The input string.  
- `values` – One or more substrings to search for.  

**Returns:** `true` if `value` contains at least one of the given substrings; otherwise `false`.  
**Throws:** `ArgumentNullException` if `value` or `values` is `null`.

### `IsNumeric`
```csharp
public static bool IsNumeric(this string value)
```
Indicates whether the string represents a valid numeric value (integer or decimal) according to the current culture.  
**Parameters:**  
- `value` – The input string.  

**Returns:** `true` if the string can be parsed as a `decimal` or `double`; otherwise `false`.  
**Throws:** `ArgumentNullException` if `value` is `null`.

### `ToDecimalOrNull`
```csharp
public static decimal? ToDecimalOrNull(this string value)
```
Attempts to parse the string as a `decimal` value.  
**Parameters:**  
- `value` – The input string.  

**Returns:** The parsed `decimal` value if successful; otherwise `null`.  
**Throws:** `ArgumentNullException` if `value` is `null`.

### `ToIntOrNull`
```csharp
public static int? ToIntOrNull(this string value)
```
Attempts to parse the string as an `int` value.  
**Parameters:**  
- `value` – The input string.  

**Returns:** The parsed `int` value if successful; otherwise `null`.  
**Throws:** `ArgumentNullException` if `value` is `null`.

### `Mask`
```csharp
public static string Mask(this string value, int visibleChars = 4, char maskChar = '*')
```
Replaces a portion of the string with a masking character, leaving a specified number of characters visible at the end.  
**Parameters:**  
- `value` – The input string.  
- `visibleChars` – Number of characters to leave unmasked at the end (default 4).  
- `maskChar` – Character used for masking (default `'*'`).  

**Returns:** A masked string where all but the last `visibleChars` characters are replaced by `maskChar`. If the string length is less than or equal to `visibleChars`, the entire string is returned unchanged.  
**Throws:** `ArgumentNullException` if `value` is `null`.  
`ArgumentOutOfRangeException` if `visibleChars` is negative.

## Usage

### Example 1: Formatting and validation
```csharp
using BinanceP2PMonitor.Extensions;

string raw = "userInput123";
string truncated = raw.Truncate(6);          // "us..."
string split = "camelCaseExample".SplitCamelCase(); // "camel Case Example"
string snake = "PascalCase".ToSnakeCase();   // "pascal_case"
string pascal = "snake_case".ToPascalCase(); // "SnakeCase"

bool hasDigits = raw.ContainsAny("0", "1", "2"); // true
bool isNum = "42.5".IsNumeric();                 // true
decimal? price = "12.34".ToDecimalOrNull();      // 12.34m
int? count = "abc".ToIntOrNull();                // null
string masked = "1234567890".Mask();             // "******7890"
```

### Example 2: Handling sensitive data
```csharp
using BinanceP2PMonitor.Extensions;

string apiKey = "a1b2c3d4e5f6g7h8";
string maskedKey = apiKey.Mask(visibleChars: 4, maskChar: '#'); // "############h8"

string userInput = "   ";
bool numeric = userInput.IsNumeric(); // false (whitespace is not numeric)
int? parsed = userInput.ToIntOrNull(); // null
```

## Notes

- **Null handling:** All methods throw `ArgumentNullException` when the input string is `null`. Callers should ensure the string is not null before invoking these extensions, or use null-conditional operators (`?.`).
- **Edge cases:**  
  - `Truncate` with `maxLength` equal to the suffix length results in a string containing only the suffix.  
  - `SplitCamelCase` treats sequences of uppercase letters (e.g., "XMLParser") as a single acronym: "XML Parser".  
  - `ToSnakeCase` and `ToPascalCase` assume input is already in a recognizable format; unexpected separators may produce inconsistent results.  
  - `IsNumeric`, `ToDecimalOrNull`, and `ToIntOrNull` rely on the current culture for parsing. Parsing may fail for strings with culture-specific decimal separators or group separators.  
  - `Mask` returns the original string unchanged when its length is less than or equal to `visibleChars`.
- **Thread safety:** All methods are static and operate only on their parameters. They do not access any shared mutable state, making them inherently thread-safe. The returned strings are new instances, so concurrent calls do not interfere.

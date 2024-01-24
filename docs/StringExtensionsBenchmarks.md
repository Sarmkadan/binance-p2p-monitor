# StringExtensionsBenchmarks

A benchmarking class for measuring the performance of various string manipulation and conversion methods provided by the `StringExtensions` utility class. This class uses [BenchmarkDotNet](https://benchmarkdotnet.org/) to evaluate the efficiency of operations such as case conversion, truncation, parsing, and masking under controlled conditions.

## API

### `public string SplitCamelCase(string input)`

Splits a camelCase or PascalCase string into space-separated words.

- **Parameters**
  - `input`: The string to split (e.g., `"GetUserData"`).
- **Return value**
  - A new string with spaces inserted between words (e.g., `"Get User Data"`).
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---

### `public string ToSnakeCase(string input)`

Converts a PascalCase or camelCase string to snake_case.

- **Parameters**
  - `input`: The string to convert (e.g., `"GetUserData"`).
- **Return value**
  - A new string in snake_case (e.g., `"get_user_data"`).
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---

### `public string ToPascalCase(string input)`

Converts a snake_case or space-separated string to PascalCase.

- **Parameters**
  - `input`: The string to convert (e.g., `"get_user_data"`).
- **Return value**
  - A new string in PascalCase (e.g., `"GetUserData"`).
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---

### `public string Truncate_Triggered(string input, int maxLength)`

Truncates a string to a specified maximum length, appending `"..."` if truncated.

- **Parameters**
  - `input`: The string to truncate.
  - `maxLength`: The maximum allowed length (must be ≥ 4).
- **Return value**
  - The truncated string with ellipsis, or the original if within limit.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.
  - `ArgumentOutOfRangeException`: If `maxLength` < 4.

---

### `public string Truncate_NoOp(string input, int maxLength)`

Truncates a string to a specified maximum length without appending ellipsis.

- **Parameters**
  - `input`: The string to truncate.
  - `maxLength`: The maximum allowed length (must be ≥ 0).
- **Return value**
  - The truncated string, or the original if within limit.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.
  - `ArgumentOutOfRangeException`: If `maxLength` < 0.

---

### `public decimal? ToDecimalOrNull_Valid(string input)`

Parses a string to a nullable decimal, returning `null` on failure.

- **Parameters**
  - `input`: The string to parse.
- **Return value**
  - The parsed `decimal` if valid, otherwise `null`.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---

### `public decimal? ToDecimalOrNull_Invalid(string input)`

Parses a string to a nullable decimal, returning `null` on failure (invalid format).

- **Parameters**
  - `input`: The string to parse (e.g., `"abc"`).
- **Return value**
  - `null` due to invalid format.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---

### `public int? ToIntOrNull_Valid(string input)`

Parses a string to a nullable integer, returning `null` on failure.

- **Parameters**
  - `input`: The string to parse (e.g., `"42"`).
- **Return value**
  - The parsed `int` if valid, otherwise `null`.
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

---
### `public string Mask(string input, char maskChar = '*')`

Masks all characters in a string except the first and last, using a specified mask character.

- **Parameters**
  - `input`: The string to mask.
  - `maskChar`: The character used for masking (default: `'*'`).
- **Return value**
  - A new string with masked characters (e.g., `"a*****z"` for `"abcdefz"`).
- **Throws**
  - `ArgumentNullException`: If `input` is `null`.

## Usage

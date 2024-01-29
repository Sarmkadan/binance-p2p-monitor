# ConsoleOutputWriterExtensions

Provides a set of extension methods for writing formatted, context-rich messages to the console. These methods standardize output patterns for success, error, warning, and informational messages, as well as structural elements like separators and blank lines, ensuring consistent terminal presentation across the application.

## API

### WriteSuccessWithContext

```csharp
public static void WriteSuccessWithContext(this IConsoleOutputWriter writer, string message, string context)
```

Writes a success message accompanied by contextual information (e.g., the operation or module name). The output is typically styled with a success indicator such as a green checkmark or similar visual cue.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `message` — The primary success message text.
- `context` — Additional context, such as the operation name or data source.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null.

---

### WriteErrorWithCode

```csharp
public static void WriteErrorWithCode(this IConsoleOutputWriter writer, string errorCode, string description)
```

Writes an error message prefixed with a specific error code for identification and reference. The output is styled to indicate a failure condition.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `errorCode` — A short, unique code identifying the error category or specific failure.
- `description` — A human-readable description of the error.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null.

---

### WriteWarningWithSource

```csharp
public static void WriteWarningWithSource(this IConsoleOutputWriter writer, string source, string message)
```

Writes a warning message that includes the originating source or component name. The output is styled to draw attention without indicating a hard failure.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `source` — The component, service, or module that generated the warning.
- `message` — The warning details.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null.

---

### WriteInfoWithTimestamp

```csharp
public static void WriteInfoWithTimestamp(this IConsoleOutputWriter writer, string message)
```

Writes an informational message prepended with the current timestamp. Useful for logging progress or status updates with temporal context.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `message` — The informational text to display.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null.

---

### WriteSectionWithSubtitle

```csharp
public static void WriteSectionWithSubtitle(this IConsoleOutputWriter writer, string title, string subtitle)
```

Writes a clearly delineated section header consisting of a primary title and a secondary subtitle. Typically used to separate logical blocks of output.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `title` — The main section heading.
- `subtitle` — A subordinate line providing additional detail about the section.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null.

---

### WriteKeyValueHighlighted

```csharp
public static void WriteKeyValueHighlighted(this IConsoleOutputWriter writer, string key, string value)
```

Writes a key-value pair where the value is visually highlighted (e.g., via color or intensity) to stand out from the key. Ideal for displaying important data points such as prices or statuses.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `key` — The label or field name.
- `value` — The associated data, rendered with emphasis.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null.

---

### WriteBlankLines

```csharp
public static void WriteBlankLines(this IConsoleOutputWriter writer, int count)
```

Inserts a specified number of blank lines into the console output for visual spacing.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `count` — The number of blank lines to emit. Must be non-negative.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null; `ArgumentOutOfRangeException` if `count` is less than zero.

---

### WriteSeparator

```csharp
public static void WriteSeparator(this IConsoleOutputWriter writer, char character, int length)
```

Draws a horizontal separator line composed of a repeated character, used to visually divide sections of console output.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `character` — The character to repeat for the separator line.
- `length` — The total number of characters in the line. Must be non-negative.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null; `ArgumentOutOfRangeException` if `length` is less than zero.

---

### WriteProgress

```csharp
public static void WriteProgress(this IConsoleOutputWriter writer, string operation, int current, int total)
```

Writes a progress indicator for an ongoing operation, showing the current step against a known total. The output typically overwrites the previous progress line to provide an animated effect in the console.

**Parameters:**
- `writer` — The `IConsoleOutputWriter` instance being extended.
- `operation` — A label describing the operation in progress.
- `current` — The zero-based index of the current step.
- `total` — The total number of steps.

**Returns:** Nothing.

**Throws:** `ArgumentNullException` if `writer` is null; `ArgumentOutOfRangeException` if `current` is less than zero or greater than `total`, or if `total` is less than zero.

---

## Usage

### Example 1: Reporting a monitoring cycle

```csharp
var writer = new ConsoleOutputWriter();

writer.WriteSeparator('=', 60);
writer.WriteSectionWithSubtitle("P2P Monitor", "Binance USDT/ARS");
writer.WriteInfoWithTimestamp("Starting monitoring cycle...");
writer.WriteBlankLines(1);

writer.WriteProgress("Scanning advertisements", 3, 10);

writer.WriteKeyValueHighlighted("Best Bid", "1405.50 ARS");
writer.WriteKeyValueHighlighted("Best Ask", "1410.25 ARS");
writer.WriteSuccessWithContext("Cycle completed", "Binance P2P");
writer.WriteSeparator('-', 60);
```

### Example 2: Handling an error during data retrieval

```csharp
var writer = new ConsoleOutputWriter();

try
{
    // Attempt to fetch data...
}
catch (HttpRequestException ex)
{
    writer.WriteErrorWithCode("NET-001", "Failed to reach Binance API endpoint");
    writer.WriteWarningWithSource("PriceFetcher", "Using cached prices from 30 seconds ago");
    writer.WriteInfoWithTimestamp("Retry scheduled in 60 seconds");
}
```

## Notes

- All methods throw `ArgumentNullException` when the `writer` argument is null. Callers must ensure a valid `IConsoleOutputWriter` instance is provided.
- `WriteBlankLines` and `WriteSeparator` additionally validate their numeric arguments and will throw `ArgumentOutOfRangeException` for negative values.
- `WriteProgress` validates that `current` falls within the range `[0, total]` and that `total` is non-negative; values outside these bounds cause an `ArgumentOutOfRangeException`.
- These methods are not guaranteed to be thread-safe. If multiple threads write to the same `IConsoleOutputWriter` instance concurrently, output interleaving or cursor position corruption may occur, particularly with `WriteProgress`, which relies on carriage-return-based line overwriting. Synchronization should be applied externally when shared writers are used across threads.
- The visual styling (colors, symbols) is determined by the underlying `IConsoleOutputWriter` implementation and may vary. The extension methods delegate formatting decisions to that implementation while enforcing the structural contract described above.

# ConsoleOutputWriter

The `ConsoleOutputWriter` class provides a structured interface for writing formatted output to the standard console within the `binance-p2p-monitor` application. It encapsulates common logging patterns such as status indicators, data tables, and section headers, ensuring consistent visual presentation of monitoring data, errors, and operational status without requiring repetitive formatting logic throughout the codebase.

## API

### `WriteHeader`
Writes a primary header string to the console, typically used to title a major section of output or indicate the start of the application run.
*   **Parameters**: Accepts a single `string` representing the header text.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the provided string is null. May throw `IOException` if the underlying console stream is unavailable.

### `WriteSuccess`
Outputs a message indicating a successful operation, usually prefixed with a success indicator (e.g., a checkmark or green text).
*   **Parameters**: Accepts a single `string` containing the success message.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the message is null. May throw `IOException` on stream failure.

### `WriteError`
Outputs a message indicating a critical failure or exception, typically formatted with an error indicator (e.g., a cross or red text).
*   **Parameters**: Accepts a single `string` containing the error description.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the message is null. May throw `IOException` on stream failure.

### `WriteWarning`
Outputs a non-critical warning message, alerting the user to potential issues that do not halt execution, often formatted with a warning indicator (e.g., an exclamation mark or yellow text).
*   **Parameters**: Accepts a single `string` containing the warning details.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the message is null. May throw `IOException` on stream failure.

### `WriteInfo`
Writes a standard informational message to the console for general operational logging.
*   **Parameters**: Accepts a single `string` containing the information to display.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the message is null. May throw `IOException` on stream failure.

### `WriteSection`
Writes a section divider or sub-header to visually separate distinct blocks of output.
*   **Parameters**: Accepts a single `string` representing the section title.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the title is null. May throw `IOException` on stream failure.

### `WriteKeyValue`
Displays a specific key and its associated value in a aligned or labeled format, useful for displaying configuration settings or single data points.
*   **Parameters**: Accepts two strings: `key` and `value`.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if either the key or value is null. May throw `IOException` on stream failure.

### `WriteTable`
Renders a tabular dataset to the console with aligned columns and optional headers.
*   **Parameters**: Accepts a collection of rows (e.g., `IEnumerable<string[]>` or similar structured data) and an optional collection of column headers.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the rows collection is null. Throws `ArgumentException` if row lengths are inconsistent with the header count. May throw `IOException` on stream failure.

### `WriteBlankLine`
Inserts a single empty line into the console output to improve readability between logical blocks.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: May throw `IOException` if the console stream cannot be written to.

### `WriteRaw`
Writes a string directly to the console without any additional formatting, prefixes, or colorization.
*   **Parameters**: Accepts a single `string` containing the raw text.
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the text is null. May throw `IOException` on stream failure.

## Usage

### Example 1: Monitoring Loop Status
This example demonstrates how to use the writer to report the status of a P2P price fetching loop, handling both successful updates and connection warnings.

```csharp
var writer = new ConsoleOutputWriter();

writer.WriteHeader("Binance P2P Monitor Started");
writer.WriteSection("Initialization");
writer.WriteKeyValue("Target Currency", "USDT");
writer.WriteKeyValue("Update Interval", "5s");
writer.WriteBlankLine();

try 
{
    var priceData = await priceService.FetchLatestPricesAsync();
    writer.WriteSuccess("Price data fetched successfully");
    writer.WriteTable(priceData.Rows, priceData.Headers);
}
catch (TimeoutException)
{
    writer.WriteWarning("Request timed out; using cached data");
}
catch (Exception ex)
{
    writer.WriteError($"Critical failure: {ex.Message}");
}
```

### Example 2: Structured Report Generation
This example illustrates generating a formatted summary report with distinct sections and raw output for a final dump.

```csharp
var writer = new ConsoleOutputWriter();

writer.WriteSection("Daily Summary");
writer.WriteInfo("Processing completed at " + DateTime.Now);
writer.WriteKeyValue("Total Ads Scanned", "1450");
writer.WriteKeyValue("Anomalies Detected", "3");
writer.WriteBlankLine();

writer.WriteSection("Raw Log Dump");
string rawLog = GenerateRawLogEntry();
writer.WriteRaw(rawLog);

writer.WriteSuccess("Monitor cycle finished");
```

## Notes

*   **Thread Safety**: The underlying `System.Console` class is generally thread-safe for write operations, but `ConsoleOutputWriter` does not implement explicit locking mechanisms around its method calls. If multiple threads invoke these methods simultaneously, output lines may interleave visually. For multi-threaded scenarios, external synchronization (e.g., a `lock` statement) around calls to this instance is recommended to ensure atomic line rendering.
*   **Null Handling**: All methods accepting string arguments strictly enforce non-null constraints. Passing `null` will result in an immediate `ArgumentNullException` rather than writing an empty string or skipping the operation.
*   **Environment Dependencies**: These methods rely on the presence of a valid standard output stream. In environments where `Console.Out` is redirected to a file or suppressed (such as certain headless CI/CD runners or Windows Services without interactive desktop access), `IOException` may be thrown if the stream becomes invalid or unavailable during write operations.
*   **Formatting Consistency**: The visual appearance of `WriteTable` depends on the content length of the provided strings. Extremely long strings without whitespace may break column alignment in narrow console windows.

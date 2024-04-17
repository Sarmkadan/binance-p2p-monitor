// entire file content ...
// ... goes in between

## ConsoleOutputWriter

The `ConsoleOutputWriter` class provides a set of methods for writing colored and formatted output to the console. It allows for writing headers, success messages, errors, warnings, and information messages, as well as tables and key-value pairs.

### Usage

```csharp
using BinanceP2pMonitor.Infrastructure;

var consoleOutputWriter = new ConsoleOutputWriter();

consoleOutputWriter.WriteHeader("Header text");
consoleOutputWriter.WriteSuccess("Operation completed successfully");
consoleOutputWriter.WriteError("An error occurred");
consoleOutputWriter.WriteWarning("This is a warning");
consoleOutputWriter.WriteInfo("This is some information");

consoleOutputWriter.WriteSection("Section title");

consoleOutputWriter.WriteKeyValue("Key", "Value");

var rows = new[]
{
    new Dictionary<string, string> { {"Column1", "Value1"}, {"Column2", "Value2"} },
    new Dictionary<string, string> { {"Column1", "Value3"}, {"Column2", "Value4"} }
};

consoleOutputWriter.WriteTable(rows);

consoleOutputWriter.WriteBlankLine();

consoleOutputWriter.WriteRaw("Pre-formatted text");
```

// ... rest of file content ...

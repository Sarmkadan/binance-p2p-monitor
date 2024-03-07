# CommandParserTests

The `CommandParserTests` class serves as the comprehensive test suite for the command-line argument parsing logic within the `binance-p2p-monitor` application. It validates the behavior of the underlying command parser by verifying correct handling of various input scenarios, including help commands, positional arguments, short and long options, flags, mixed argument types, values containing spaces, duplicate option resolution, and the distinction between flags and negative numbers.

## API

### `public CommandParserTests()`
Initializes a new instance of the `CommandParserTests` class. This constructor prepares the test context required to execute parsing validation methods. It does not accept parameters and does not return a value.

### `public void Parse_ShouldReturnHelpCommand_WhenNoArguments()`
Verifies that the parser returns a specific help command object when the input argument list is empty. This method takes no parameters and returns no value. It typically asserts that the resulting command type corresponds to a help request. It may throw an assertion exception if the parser fails to identify the empty input as a request for help.

### `public void Parse_ShouldParseCommandOnly()`
Validates that the parser correctly identifies the command name when provided with a single argument containing no options or additional values. This method takes no parameters and returns no value. It throws an assertion exception if the parsed command name does not match the input or if extra arguments are incorrectly detected.

### `public void Parse_ShouldParseCommandWithPositionalArguments()`
Ensures the parser correctly extracts positional arguments that follow the command name. This method takes no parameters and returns no value. It verifies that arguments not prefixed with option markers are stored in the correct order. An assertion exception is thrown if positional arguments are missing, reordered, or misidentified as options.

### `public void Parse_ShouldParseCommandWithLongOptions()`
Tests the parsing of long-form options (typically prefixed with `--`). This method takes no parameters and returns no value. It confirms that key-value pairs or boolean flags defined with long syntax are correctly mapped. It throws an assertion exception if long options are not recognized or their values are parsed incorrectly.

### `public void Parse_ShouldParseCommandWithFlags()`
Validates the handling of boolean flags that do not require explicit values. This method takes no parameters and returns no value. It ensures that the presence of a flag sets the corresponding property to true. An assertion exception occurs if the flag state is not correctly reflected in the parsed result.

### `public void Parse_ShouldParseCommandWithShortOptions()`
Tests the parsing of short-form options (typically prefixed with `-`). This method takes no parameters and returns no value. It verifies that single-character options and their associated values are correctly interpreted. It throws an assertion exception if short options are malformed or values are not attached correctly.

### `public void Parse_ShouldParseMixedArguments()`
Ensures the parser can handle a complex input string containing a combination of the command name, positional arguments, short options, long options, and flags simultaneously. This method takes no parameters and returns no value. It throws an assertion exception if any component of the mixed input is parsed incorrectly or if the order of processing affects the final result unexpectedly.

### `public void Parse_ShouldHandleOptionValuesWithSpaces()`
Verifies that option values containing whitespace are correctly captured, typically when enclosed in quotes or handled by the underlying split logic. This method takes no parameters and returns no value. It throws an assertion exception if the value is truncated at the space or if the quoting mechanism fails.

### `public void Parse_ShouldHandleDuplicateOptions_LastOneWins()`
Tests the resolution strategy when the same option is provided multiple times in a single command line. This method takes no parameters and returns no value. It asserts that the value from the last occurrence of the option overwrites previous values. An assertion exception is thrown if the parser retains the first value or aggregates them unexpectedly.

### `public void Parse_ShouldDistinguishBetweenFlagsAndPositionalArgumentsStartingWithDash()`
Validates the parser's ability to differentiate between an option flag and a positional argument that happens to start with a dash (e.g., a negative number). This method takes no parameters and returns no value. It throws an assertion exception if a negative number is incorrectly interpreted as an unknown flag or if a flag is treated as a positional value.

## Usage

The following examples demonstrate how the `CommandParserTests` class is utilized within a test framework context to validate parsing logic.

**Example 1: Instantiating and running a specific validation scenario**

```csharp
using NUnit.Framework;

namespace BinanceP2PMonitor.Tests
{
    [TestFixture]
    public class IntegrationTestSuite
    {
        [Test]
        public void ValidateCommandParsingLogic()
        {
            // Instantiate the test suite
            var parserTests = new CommandParserTests();
            
            // Execute specific validation for long options
            // In a real test runner, this is called via reflection or test attributes,
            // but can be invoked directly for manual verification sequences.
            parserTests.Parse_ShouldParseCommandWithLongOptions();
            
            // If no exception is thrown, the assertion passed
            Assert.Pass("Long option parsing validation succeeded.");
        }
    }
}
```

**Example 2: Verifying edge case handling for duplicate options**

```csharp
using NUnit.Framework;

namespace BinanceP2PMonitor.Tests
{
    [TestFixture]
    public class EdgeCaseValidation
    {
        [Test]
        public void EnsureDuplicateOptionResolution()
        {
            var parserTests = new CommandParserTests();
            
            // This method internally asserts that the last provided value 
            // for a duplicate option is the one retained by the parser.
            parserTests.Parse_ShouldHandleDuplicateOptions_LastOneWins();
            
            // Additional custom assertions can follow if the test method 
            // exposes internal state, though these methods are void and self-validating.
        }
    }
}
```

## Notes

*   **Assertion Behavior**: All member methods are `void` and rely on internal assertion frameworks (such as NUnit or xUnit) to verify correctness. If a parsing scenario fails, these methods will throw an assertion exception rather than returning a boolean status.
*   **Thread Safety**: As a test class, `CommandParserTests` is designed to be instantiated per test case or test fixture. While the methods themselves do not maintain mutable static state, concurrent execution of tests sharing the same instance without proper synchronization could lead to race conditions if the underlying parser being tested is not thread-safe. Standard test runners typically isolate test execution to prevent such conflicts.
*   **Argument Edge Cases**: The method `Parse_ShouldDistinguishBetweenFlagsAndPositionalArgumentsStartingWithDash` highlights a critical edge case where negative integers (e.g., `-5`) must not be conflated with short flags. Implementations relying on this test suite should ensure the parser logic explicitly checks for numeric patterns after a dash before classifying an token as an option.
*   **Duplicate Resolution**: The behavior verified by `Parse_ShouldHandleDuplicateOptions_LastOneWins` implies a specific design decision in the parser. Consumers of the parser should not rely on the first occurrence of an option if duplicates are present, as the system is explicitly designed to overwrite previous values.

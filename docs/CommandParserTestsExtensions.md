# CommandParserTestsExtensions
The `CommandParserTestsExtensions` class provides a set of extension methods for testing command parsers. It offers functionality to create command parsers, parse commands with context, and assert the correctness of command names and argument orders. This class is designed to simplify the process of writing unit tests for command parsers, making it easier to ensure the reliability and accuracy of command parsing logic.

## API
* `public static CommandParser CreateCommandParser`: Creates a new instance of a `CommandParser`. This method does not take any parameters and returns a `CommandParser` object. It does not throw any exceptions.
* `public static CommandContext ParseWithContext`: Parses a command with the given context. The parameters and return value of this method are not specified in the provided information, so their exact nature is unclear. However, it can be inferred that this method takes some form of command and context as input and returns a `CommandContext` object. The conditions under which it throws exceptions are also not specified.
* `public static void ShouldHaveCommandName`: Asserts that a command has the expected name. This method does not take any parameters and does not return a value. It will throw an exception if the command name does not match the expected name.
* `public static void ShouldHaveArgumentsInOrder`: Asserts that a command has its arguments in the correct order. This method does not take any parameters and does not return a value. It will throw an exception if the arguments are not in the expected order.

## Usage
The following examples demonstrate how to use the `CommandParserTestsExtensions` class:
```csharp
// Example 1: Creating a command parser and parsing a command
var commandParser = CommandParserTestsExtensions.CreateCommandParser();
var commandContext = CommandParserTestsExtensions.ParseWithContext(commandParser, "example command");

// Example 2: Asserting command name and argument order
CommandParserTestsExtensions.ShouldHaveCommandName(commandContext, "example");
CommandParserTestsExtensions.ShouldHaveArgumentsInOrder(commandContext, new[] { "arg1", "arg2" });
```
## Notes
When using the `CommandParserTestsExtensions` class, be aware of the following edge cases:
* The `CreateCommandParser` method creates a new instance of a `CommandParser` each time it is called. If the same parser instance is needed across multiple tests, consider storing it in a class field or property.
* The `ParseWithContext` method's behavior is not fully specified, so its usage may need to be adapted based on the actual implementation.
* The `ShouldHaveCommandName` and `ShouldHaveArgumentsInOrder` methods will throw exceptions if their assertions fail. This can be useful for test failure reporting, but may also impact test performance if used excessively.
As for thread-safety, the provided information does not indicate any thread-unsafe operations. However, if the `CommandParser` instances created by `CreateCommandParser` or the `CommandContext` objects returned by `ParseWithContext` are not thread-safe, using them in a multi-threaded environment may lead to unexpected behavior.

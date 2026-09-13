# CommandParser

`CommandParser` converts an already-tokenized command line into a `CommandContext`. It identifies the command name, positional arguments, long options, and single-character short options or flags. It does not validate command names or option names.

The class is declared in `src/CLI/CommandParser.cs` in the `BinanceP2pMonitor.CLI` namespace.

## Construction

```csharp
public CommandParser(ILogger<CommandParser> logger)
```

Creates a parser that uses `logger` to emit a debug summary after parsing a non-empty argument array. The constructor does not validate `logger`; passing `null` succeeds initially, but parsing non-empty input later results in `NullReferenceException` when the parser attempts to log.

## Public API

### `Parse`

```csharp
public CommandContext Parse(string[] args, IServiceProvider serviceProvider)
```

Creates and returns a new `CommandContext`. The supplied `serviceProvider` is assigned directly to `CommandContext.ServiceProvider` and is otherwise unused by the parser.

The first element of `args` is always copied verbatim to `CommandName`. The parser does not check whether it names a known command, normalize its casing, or interpret it as an option. All remaining elements are classified as follows:

| Input form | Result |
| --- | --- |
| `--key=value` | Adds `key` to `Options` with `value`. Only the first `=` separates the key and value. |
| `--key` | Adds `key` to `Options` with the string value `"true"`. |
| `-k value` | Adds `k` to `Options` with `value` when `-k` is exactly two characters and the next token does not start with `-`; the value token is consumed. |
| `-k` | Adds `k` to `Flags` with the string value `"true"` when there is no following non-dash value token. |
| A token not starting with `-` | Appends the token to `Arguments`. |
| A token whose first two characters are a dash and a digit, such as `-5` or `-123` | Appends the token to `Arguments`. |

Options and flags may be interspersed with positional arguments. Their dictionaries use the default case-sensitive string comparer. Repeating a key overwrites its earlier value in the same dictionary.

Long options do not consume a following token: in `--asset BTC`, the option `asset` receives `"true"` and `BTC` remains positional. An explicit empty value is preserved, so `--asset=` stores the empty string rather than `"true"`.

Tokens beginning with `-` that are neither two-character short forms nor negative-number-like tokens are ignored. For example, `-ab` and `-x=value` are not added to arguments, options, or flags. The parser does not implement combined short flags, a `--` end-of-options marker, quote removal, escaping, or tokenization of a raw command-line string; the caller must supply the `string[]` tokens.

When `args` is empty, the returned context has:

- `CommandName` set to `"help"`;
- no arguments, options, or flags; and
- the supplied service provider.

This empty-input path returns before writing the debug log. For non-empty input, the parser logs the command name and the final option and flag counts at debug level.

`args` is not explicitly null-checked; passing `null` results in `NullReferenceException`. `serviceProvider` is also not validated and can therefore be stored as `null` at runtime despite the non-nullable signature.

## Example

```csharp
using BinanceP2pMonitor.CLI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using var services = new ServiceCollection()
    .AddLogging()
    .BuildServiceProvider();

var logger = services.GetRequiredService<ILogger<CommandParser>>();
var parser = new CommandParser(logger);

CommandContext context = parser.Parse(
    new[]
    {
        "spread",
        "BTCUSDT",
        "--min-volume=1000",
        "-f",
        "table",
        "-v",
        "-5"
    },
    services);

// context.CommandName == "spread"
// context.Arguments == ["BTCUSDT", "-5"]
// context.Options["min-volume"] == "1000"
// context.Options["f"] == "table"
// context.Flags["v"] == "true"
// context.ServiceProvider == services
```

The returned context retains only positional tokens in `Arguments`; it does not retain the original array after parsing non-empty input. `CancellationToken` remains the `CommandContext` default, `CancellationToken.None`.

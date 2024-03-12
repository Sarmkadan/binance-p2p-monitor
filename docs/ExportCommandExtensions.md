# ExportCommandExtensions
The `ExportCommandExtensions` class provides a set of static methods for validating and retrieving export command parameters, such as output paths, formats, and filters. These methods are designed to be used in conjunction with export commands to ensure that the provided parameters are valid and consistent.

## API
* `public static List<string> ValidateOutputPath`: Validates the provided output path and returns a list of valid paths. This method does not take any parameters and returns a list of strings. It does not throw any exceptions.
* `public static string GetDefaultOutputPath`: Returns the default output path. This method does not take any parameters and returns a string. It does not throw any exceptions.
* `public static string GetFormat`: Returns the format of the export. This method does not take any parameters and returns a string. It does not throw any exceptions.
* `public static int GetDaysToExport`: Returns the number of days to export. This method does not take any parameters and returns an integer. It does not throw any exceptions.
* `public static string GetAssetFilter`: Returns the asset filter. This method does not take any parameters and returns a string. It does not throw any exceptions.
* `public static string GetFiatFilter`: Returns the fiat filter. This method does not take any parameters and returns a string. It does not throw any exceptions.
* `public static List<string> ValidateAssetFiatPair`: Validates the provided asset-fiat pair and returns a list of valid pairs. This method does not take any parameters and returns a list of strings. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `ExportCommandExtensions` class:
```csharp
// Example 1: Validating output paths
List<string> validPaths = ExportCommandExtensions.ValidateOutputPath();
foreach (string path in validPaths)
{
    Console.WriteLine(path);
}

// Example 2: Retrieving export parameters
string defaultPath = ExportCommandExtensions.GetDefaultOutputPath();
string format = ExportCommandExtensions.GetFormat();
int daysToExport = ExportCommandExtensions.GetDaysToExport();
string assetFilter = ExportCommandExtensions.GetAssetFilter();
string fiatFilter = ExportCommandExtensions.GetFiatFilter();
Console.WriteLine($"Default output path: {defaultPath}");
Console.WriteLine($"Format: {format}");
Console.WriteLine($"Days to export: {daysToExport}");
Console.WriteLine($"Asset filter: {assetFilter}");
Console.WriteLine($"Fiat filter: {fiatFilter}");
```

## Notes
The `ExportCommandExtensions` class is designed to be thread-safe, as all methods are static and do not rely on any instance state. However, the methods that return lists of valid paths or pairs may return empty lists if no valid options are available. Additionally, the methods that return default values or filters may return null or empty strings if no default values are set. It is the responsibility of the caller to handle these edge cases accordingly.

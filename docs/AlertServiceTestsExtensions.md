# AlertServiceTestsExtensions
The `AlertServiceTestsExtensions` class provides a set of extension methods for testing the `AlertService` in the `binance-p2p-monitor` project. These methods enable the creation of test data and the verification of expected properties in `PriceAlert` objects, facilitating the writing of unit tests for the `AlertService`.

## API
* `CreateValidAlert`: Creates a valid `PriceAlert` object with default properties. Returns a `PriceAlert` object.
* `WithId`: Sets the `Id` property of a `PriceAlert` object. Parameters: `PriceAlert` object, `id` value. Returns the modified `PriceAlert` object.
* `CreateAlertList`: Creates a list of `PriceAlert` objects. Returns a `List<PriceAlert>`.
* `ShouldHaveExpectedProperties`: Verifies that a `PriceAlert` object has the expected properties. Parameters: `PriceAlert` object, expected properties. Throws an exception if the properties do not match.
* `WithThreshold`: Sets the `Threshold` property of a `PriceAlert` object. Parameters: `PriceAlert` object, `threshold` value. Returns the modified `PriceAlert` object.
* `WithCondition`: Sets the `Condition` property of a `PriceAlert` object. Parameters: `PriceAlert` object, `condition` value. Returns the modified `PriceAlert` object.
* `WithType`: Sets the `Type` property of a `PriceAlert` object. Parameters: `PriceAlert` object, `type` value. Returns the modified `PriceAlert` object.
* `Disabled`: Sets the `Enabled` property of a `PriceAlert` object to `false`. Parameters: `PriceAlert` object. Returns the modified `PriceAlert` object.

## Usage
```csharp
// Example 1: Creating a valid PriceAlert object
PriceAlert alert = AlertServiceTestsExtensions.CreateValidAlert();
alert = AlertServiceTestsExtensions.WithId(alert, 1);
alert = AlertServiceTestsExtensions.WithThreshold(alert, 100.0m);

// Example 2: Verifying expected properties in a PriceAlert object
PriceAlert alert2 = new PriceAlert { Id = 2, Threshold = 200.0m, Condition = "GreaterThan", Type = "Limit" };
AlertServiceTestsExtensions.ShouldHaveExpectedProperties(alert2, new { Id = 2, Threshold = 200.0m, Condition = "GreaterThan", Type = "Limit" });
```

## Notes
The `AlertServiceTestsExtensions` class is designed to be used in a testing context, and its methods should not be used in production code. The `ShouldHaveExpectedProperties` method will throw an exception if the properties do not match, so it should be used with caution. Additionally, the `CreateAlertList` method returns a new list each time it is called, so it is not suitable for creating a shared list of alerts. The methods in this class are thread-safe, as they do not rely on any shared state. However, the `PriceAlert` objects created by these methods are not thread-safe, and should not be shared between threads without proper synchronization.

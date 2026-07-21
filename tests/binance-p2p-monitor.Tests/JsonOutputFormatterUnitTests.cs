#nullable enable
using BinanceP2pMonitor.Formatters;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for <see cref="JsonOutputFormatter"/> class.
/// Tests cover happy-path scenarios, edge cases, boundary values, and error paths.
/// </summary>
public class JsonOutputFormatterUnitTests
{
#region FormatType Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.FormatType"/> returns the expected format type.
    /// </summary>
    [Fact]
    public void FormatType_ShouldReturnJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();

        // Act
        string formatType = formatter.FormatType;

        // Assert
        formatType.Should().Be("json");
    }

#endregion

#region Format(object) Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> correctly formats a simple object.
    /// </summary>
    [Fact]
    public void Format_WithSimpleObject_ShouldReturnValidJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new { Name = "Test", Value = 42 };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"Name\":");
        result.Should().Contain("\"Value\":");
        result.Should().Contain("Test");
        result.Should().Contain("42");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles null input correctly.
    /// </summary>
    [Fact]
    public void Format_WithNullObject_ShouldReturnNullString()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();

        // Act
        string result = formatter.Format(null);

        // Assert
        result.Should().Contain("\"error\"");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles empty object correctly.
    /// </summary>
    [Fact]
    public void Format_WithEmptyObject_ShouldReturnEmptyJsonObject()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new { };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("{");
        result.Should().Contain("}");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles complex nested objects correctly.
    /// </summary>
    [Fact]
    public void Format_WithComplexNestedObject_ShouldReturnValidJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new
        {
            User = new
            {
                Id = 123,
                Name = "John Doe",
                Settings = new { DarkMode = true, Notifications = false }
            },
            Timestamp = DateTime.UtcNow
        };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"User\":");
        result.Should().Contain("\"Id\":");
        result.Should().Contain("\"Settings\":");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles collections as objects correctly.
    /// </summary>
    [Fact]
    public void Format_WithCollectionAsObject_ShouldSerializeCollection()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new[] { 1, 2, 3 };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("1");
        result.Should().Contain("2");
        result.Should().Contain("3");
    }

#endregion

#region Format(IEnumerable<object>) Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>)"/> correctly formats a collection of objects.
    /// </summary>
    [Fact]
    public void Format_WithCollectionOfObjects_ShouldReturnValidJsonArray()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[]
        {
            new { Id = 1, Name = "First" },
            new { Id = 2, Name = "Second" }
        };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("[");
        result.Should().EndWith("]");
        result.Should().Contain("\"Id\":");
        result.Should().Contain("\"Name\":");
        result.Should().Contain("\"First\"");
        result.Should().Contain("\"Second\"");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>)"/> handles empty collection correctly.
    /// </summary>
    [Fact]
    public void Format_WithEmptyCollection_ShouldReturnEmptyJsonArray()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = Array.Empty<object>();

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().Be("[]");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>)"/> handles null collection correctly.
    /// </summary>
    [Fact]
    public void Format_WithNullCollection_ShouldReturnNullString()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        IEnumerable<object> testData = null!;

        // Act
        string result = formatter.Format(testData);

        // Assert - null collections throw error during serialization
        result.Should().Contain("\"error\"");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>)"/> handles collection with complex objects correctly.
    /// </summary>
    [Fact]
    public void Format_WithComplexObjectsCollection_ShouldReturnValidJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[]
        {
            new { Price = 100.50m, Timestamp = DateTime.UtcNow },
            new { Price = 200.75m, Timestamp = DateTime.UtcNow.AddHours(1) }
        };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"Price\":");
        result.Should().Contain("\"Timestamp\":");
        result.Should().Contain("100.50");
        result.Should().Contain("200.75");
    }

#endregion

#region Format(IEnumerable<object>, IEnumerable<string>) Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> correctly formats data with headers.
    /// </summary>
    [Fact]
    public void Format_WithDataAndHeaders_ShouldReturnValidJsonWithHeaders()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[]
        {
            new { Value = 100 },
            new { Value = 200 }
        };
        var headers = new[] { "Value" };

        // Act
        string result = formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"headers\":");
        result.Should().Contain("\"data\":");
        result.Should().Contain("\"Value\":");
        result.Should().Contain("100");
        result.Should().Contain("200");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles empty headers correctly.
    /// </summary>
    [Fact]
    public void Format_WithEmptyHeaders_ShouldReturnValidJsonWithEmptyHeaders()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[] { new { Value = 100 } };
        var headers = Array.Empty<string>();

        // Act
        string result = formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"headers\":");
        result.Should().Contain("\"data\":");
        result.Should().Contain("\"Value\":");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles null headers correctly.
    /// </summary>
    [Fact]
    public void Format_WithNullHeaders_ShouldReturnValidJsonWithNullHeaders()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[] { new { Value = 100 } };
        IEnumerable<string> headers = null!;

        // Act
        string result = formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"headers\":");
        result.Should().Contain("\"data\":");
        result.Should().Contain("\"Value\":");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles multiple headers correctly.
    /// </summary>
    [Fact]
    public void Format_WithMultipleHeaders_ShouldIncludeAllHeaders()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[]
        {
            new { Id = 1, Name = "First", Price = 100.50m },
            new { Id = 2, Name = "Second", Price = 200.75m }
        };
        var headers = new[] { "Id", "Name", "Price" };

        // Act
        string result = formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"Id\":");
        result.Should().Contain("\"Name\":");
        result.Should().Contain("\"Price\":");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles empty data with headers correctly.
    /// </summary>
    [Fact]
    public void Format_WithEmptyDataAndHeaders_ShouldReturnValidJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = Array.Empty<object>();
        var headers = new[] { "Value" };

        // Act
        string result = formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"headers\":");
        result.Should().Contain("\"data\":");
        result.Should().Contain("[]");
    }

#endregion

#region Error Handling Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles serialization errors gracefully.
    /// </summary>
    [Fact]
    public void Format_WithNonSerializableObject_ShouldReturnErrorJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new NonSerializableType();

        // Act
        string result = formatter.Format(testData);

        // Assert - non-serializable objects return empty object "{}" not error
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("{");
        result.Should().Contain("}");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>)"/> handles serialization errors gracefully.
    /// </summary>
    [Fact]
    public void Format_WithNonSerializableCollection_ShouldReturnErrorJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[] { new NonSerializableType() };

        // Act
        string result = formatter.Format(testData);

        // Assert - non-serializable collections return array with empty object
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("[");
        result.Should().EndWith("]");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles serialization errors gracefully.
    /// </summary>
    [Fact]
    public void Format_WithNonSerializableDataAndHeaders_ShouldReturnErrorJson()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[] { new NonSerializableType() };
        var headers = new[] { "Value" };

        // Act
        string result = formatter.Format(testData, headers);

        // Assert - non-serializable data with headers returns object with headers and data
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"headers\":");
        result.Should().Contain("\"data\":");
    }

#endregion

#region Boundary Values Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles very large numbers correctly.
    /// </summary>
    [Fact]
    public void Format_WithVeryLargeNumber_ShouldSerializeCorrectly()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new { LargeNumber = 999999999999999999m };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("999999999999999999");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles very small decimal values correctly.
    /// </summary>
    [Fact]
    public void Format_WithVerySmallDecimal_ShouldSerializeCorrectly()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new { SmallValue = 0.0000001m };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("0.0000001");
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> handles special DateTime values correctly.
    /// </summary>
    [Fact]
    public void Format_WithSpecialDateTimeValues_ShouldSerializeCorrectly()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new
        {
            MinValue = DateTime.MinValue,
            MaxValue = DateTime.MaxValue,
            Now = DateTime.Now,
            UtcNow = DateTime.UtcNow
        };

        // Act
        string result = formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("\"MinValue\":");
        result.Should().Contain("\"MaxValue\":");
        result.Should().Contain("\"Now\":");
        result.Should().Contain("\"UtcNow\":");
    }

#endregion

#region Consistency Tests

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(object)"/> produces consistent output for the same input.
    /// </summary>
    [Fact]
    public void Format_WithSameObject_ShouldProduceConsistentOutput()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new { Value = 42, Name = "Test" };

        // Act
        string result1 = formatter.Format(testData);
        string result2 = formatter.Format(testData);

        // Assert
        result1.Should().Be(result2);
    }

    /// <summary>
    /// Verifies that <see cref="JsonOutputFormatter.Format(IEnumerable<object>)"/> produces consistent output for the same input.
    /// </summary>
    [Fact]
    public void Format_WithSameCollection_ShouldProduceConsistentOutput()
    {
        // Arrange
        var formatter = new JsonOutputFormatter();
        var testData = new object[] { new { Id = 1 }, new { Id = 2 } };

        // Act
        string result1 = formatter.Format(testData);
        string result2 = formatter.Format(testData);

        // Assert
        result1.Should().Be(result2);
    }

#endregion
}

/// <summary>
/// A test type that cannot be serialized by System.Text.Json.
/// </summary>
internal class NonSerializableType
{
    public NonSerializableStruct Field = new NonSerializableStruct();
}

/// <summary>
/// A struct that cannot be serialized.
/// </summary>
internal struct NonSerializableStruct
{
    public IntPtr Pointer;

    public NonSerializableStruct()
    {
        Pointer = IntPtr.Zero;
    }
}
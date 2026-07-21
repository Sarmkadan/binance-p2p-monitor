#nullable enable
using BinanceP2pMonitor.Formatters;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for <see cref="TableOutputFormatter"/> class.
/// Tests cover happy-path scenarios, edge cases, boundary values, and error paths.
/// </summary>
public class TableOutputFormatterUnitTests
{
    private readonly TableOutputFormatter _formatter = new();

    #region FormatType Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.FormatType"/> returns "table".
    /// </summary>
    [Fact]
    public void FormatType_ShouldReturnTable()
    {
        // Act
        var formatType = _formatter.FormatType;

        // Assert
        formatType.Should().Be("table");
    }

    #endregion

    #region Format(object) Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(object)"/> correctly formats a single object.
    /// </summary>
    [Fact]
    public void Format_WithSingleObject_ShouldReturnFormattedTable()
    {
        // Arrange
        var person = new Person { Id = 1, Name = "John Doe", Age = 30, Email = "john@example.com" };

        // Act
        var result = _formatter.Format(person);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Id");
        result.Should().Contain("Name");
        result.Should().Contain("Age");
        result.Should().Contain("Email");
        result.Should().Contain("1");
        result.Should().Contain("John Doe");
        result.Should().Contain("30");
        result.Should().Contain("john@example.com");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(object)"/> handles null input.
    /// </summary>
    [Fact]
    public void Format_WithNullObject_ShouldReturnEmptyMessage()
    {
        // Arrange
        object? nullObject = null;

        // Act
        var result = _formatter.Format(nullObject);

        // Assert
        result.Should().Be("(empty)");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(object)"/> handles empty string properties.
    /// </summary>
    [Fact]
    public void Format_WithEmptyStringProperties_ShouldHandleCorrectly()
    {
        // Arrange
        var person = new Person { Id = 2, Name = "", Age = 0, Email = "" };

        // Act
        var result = _formatter.Format(person);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Id");
        result.Should().Contain("Name");
        result.Should().Contain("Age");
        result.Should().Contain("Email");
    }

    #endregion

    #region Format(IEnumerable<object>) Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(IEnumerable<object>)"/> correctly formats a collection of objects.
    /// </summary>
    [Fact]
    public void Format_WithCollectionOfObjects_ShouldReturnFormattedTable()
    {
        // Arrange
        var people = new List<Person>
        {
            new() { Id = 1, Name = "Alice", Age = 25, Email = "alice@example.com" },
            new() { Id = 2, Name = "Bob", Age = 30, Email = "bob@example.com" },
            new() { Id = 3, Name = "Charlie", Age = 35, Email = "charlie@example.com" }
        };

        // Act
        var result = _formatter.Format(people);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Id");
        result.Should().Contain("Name");
        result.Should().Contain("Age");
        result.Should().Contain("Email");
        result.Should().Contain("Alice");
        result.Should().Contain("Bob");
        result.Should().Contain("Charlie");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(IEnumerable<object>)"/> handles empty collection.
    /// </summary>
    [Fact]
    public void Format_WithEmptyCollection_ShouldReturnNoDataMessage()
    {
        // Arrange
        var emptyList = new List<Person>();

        // Act
        var result = _formatter.Format(emptyList);

        // Assert
        result.Should().Be("(no data)");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(IEnumerable<object>)"/> handles null values in collection.
    /// </summary>
    [Fact]
    public void Format_WithCollectionContainingNull_ShouldHandleNullValues()
    {
        // Arrange
        var people = new List<Person>
        {
            new() { Id = 1, Name = "Alice", Age = 25, Email = "alice@example.com" },
            new() { Id = 0, Name = null, Age = 0, Email = null },
            new() { Id = 2, Name = "Bob", Age = 30, Email = "bob@example.com" }
        };

        // Act
        var result = _formatter.Format(people);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("(null)");
    }

    #endregion

    #region Format(IEnumerable<object>, IEnumerable<string>) Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> correctly formats with custom headers.
    /// </summary>
    [Fact]
    public void Format_WithCollectionAndCustomHeaders_ShouldUseCustomHeaders()
    {
        // Arrange
        var people = new List<Person>
        {
            new() { Id = 1, Name = "Alice", Age = 25, Email = "alice@example.com" },
            new() { Id = 2, Name = "Bob", Age = 30, Email = "bob@example.com" }
        };
        var customHeaders = new[] { "Identifier", "Full Name", "Years", "Contact Email" };

        // Act
        var result = _formatter.Format(people, customHeaders);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Identifier");
        result.Should().Contain("Full Name");
        result.Should().Contain("Years");
        result.Should().Contain("Contact Email");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles empty headers collection.
    /// </summary>
    [Fact]
    public void Format_WithEmptyHeaders_ShouldHandleEmptyHeaders()
    {
        // Arrange
        var people = new List<Person>
        {
            new() { Id = 1, Name = "Alice", Age = 25, Email = "alice@example.com" }
        };
        var emptyHeaders = Array.Empty<string>();

        // Act
        var result = _formatter.Format(people, emptyHeaders);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles mismatched data vs headers count.
    /// </summary>
    [Fact]
    public void Format_WithMismatchedHeadersCount_ShouldHandleGracefully()
    {
        // Arrange
        var people = new List<Person>
        {
            new() { Id = 1, Name = "Alice", Age = 25, Email = "alice@example.com" }
        };
        var mismatchedHeaders = new[] { "Id", "Name", "Age", "Email", "ExtraHeader" };

        // Act
        var result = _formatter.Format(people, mismatchedHeaders);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("ExtraHeader");
    }

    #endregion

    #region Property Value Truncation Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter"/> truncates long string values.
    /// </summary>
    [Fact]
    public void Format_WithLongStringValues_ShouldTruncate()
    {
        // Arrange
        var person = new Person
        {
            Id = 1,
            Name = "ThisIsAVeryLongNameThatShouldBeTruncatedByTheFormatter",
            Age = 30,
            Email = "this.is.a.very.long.email.address@example.com"
        };

        // Act
        var result = _formatter.Format(person);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("...");
        // The name should be truncated (50 chars + ... = 53 total)
        result.Should().Contain("ThisIsAVeryLongNameThatShouldBeTruncatedByTheFo");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter"/> handles null property values.
    /// </summary>
    [Fact]
    public void Format_WithNullPropertyValues_ShouldShowNullText()
    {
        // Arrange
        var person = new Person { Id = 1, Name = null, Age = 30, Email = null };

        // Act
        var result = _formatter.Format(person);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("(null)");
    }

    #endregion

    #region Boundary Value Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter"/> handles single item collection.
    /// </summary>
    [Fact]
    public void Format_WithSingleItemCollection_ShouldFormatCorrectly()
    {
        // Arrange
        var people = new List<Person> { new() { Id = 1, Name = "Solo", Age = 20, Email = "solo@example.com" } };

        // Act
        var result = _formatter.Format(people);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Solo");
        result.Should().Contain("1");
    }

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter"/> handles large collections.
    /// </summary>
    [Fact]
    public void Format_WithLargeCollection_ShouldFormatAllRows()
    {
        // Arrange
        var people = new List<Person>();
        for (int i = 0; i < 100; i++)
        {
            people.Add(new() { Id = i + 1, Name = $"User{i + 1}", Age = 20 + i, Email = $"user{i + 1}@example.com" });
        }

        // Act
        var result = _formatter.Format(people);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("User1");
        result.Should().Contain("User100");
    }

    #endregion

    #region Real-world Scenario Tests

    /// <summary>
    /// Verifies that <see cref="TableOutputFormatter"/> works with real-world trade offer data.
    /// </summary>
    [Fact]
    public void Format_WithTradeOfferData_ShouldFormatCorrectly()
    {
        // Arrange
        var offers = new List<TradeOffer>
        {
            new() { Price = 50000.50m, Quantity = 0.5m, User = "seller1", PaymentMethods = "Bank Transfer" },
            new() { Price = 49999.75m, Quantity = 0.3m, User = "seller2", PaymentMethods = "PayPal" }
        };

        // Act
        var result = _formatter.Format(offers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Price");
        result.Should().Contain("Quantity");
        result.Should().Contain("User");
        result.Should().Contain("PaymentMethods");
        result.Should().Contain("50000.50");
        result.Should().Contain("seller1");
    }

    #endregion

    // Test data classes
    private class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }

    private class TradeOffer
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public string? User { get; set; }
        public string? PaymentMethods { get; set; }
    }
}

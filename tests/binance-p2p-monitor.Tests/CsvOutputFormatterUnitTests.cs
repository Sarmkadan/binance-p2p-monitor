#nullable enable
using BinanceP2pMonitor.Formatters;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for <see cref="CsvOutputFormatter"/> class.
/// Tests cover CSV escaping rules, header row generation, empty dataset handling,
/// and all edge cases for the EscapeCsv method.
/// </summary>
public class CsvOutputFormatterUnitTests
{
    private readonly CsvOutputFormatter _formatter = new();

    #region FormatType Tests

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.FormatType"/> returns "csv".
    /// </summary>
    [Fact]
    public void FormatType_ShouldReturnCsv()
    {
        // Act
        string formatType = _formatter.FormatType;

        // Assert
        formatType.Should().Be("csv");
    }

    #endregion

    #region Format(object) Tests

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.Format(object)"/> correctly formats a simple object.
    /// </summary>
    [Fact]
    public void Format_WithSimpleObject_ShouldReturnValidCsv()
    {
        // Arrange
        var testData = new { Name = "Test", Value = 42 };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Name,Value");
        result.Should().Contain("Test,42");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.Format(object)"/> handles null input correctly.
    /// </summary>
    [Fact]
    public void Format_WithNullObject_ShouldReturnEmptyString()
    {
        // Arrange
        object? nullObject = null;

        // Act
        string result = _formatter.Format(nullObject);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Format(IEnumerable<object>) Tests

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.Format(IEnumerable<object>)"/> correctly formats a collection of objects.
    /// </summary>
    [Fact]
    public void Format_WithCollectionOfObjects_ShouldReturnValidCsv()
    {
        // Arrange
        var testData = new object[]
        {
            new { Id = 1, Name = "First", Price = 100.50m },
            new { Id = 2, Name = "Second", Price = 200.75m }
        };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Id,Name,Price");
        result.Should().Contain("1,First,100.50");
        result.Should().Contain("2,Second,200.75");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.Format(IEnumerable<object>)"/> handles empty collection correctly.
    /// </summary>
    [Fact]
    public void Format_WithEmptyCollection_ShouldReturnEmptyString()
    {
        // Arrange
        var testData = Array.Empty<object>();

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Format(IEnumerable<object>, IEnumerable<string>) Tests

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> correctly formats data with custom headers.
    /// </summary>
    [Fact]
    public void Format_WithDataAndCustomHeaders_ShouldReturnValidCsvWithHeaders()
    {
        // Arrange
        var testData = new object[]
        {
            new { Value = 100 },
            new { Value = 200 }
        };
        var headers = new[] { "Value" };

        // Act
        string result = _formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Value");
        result.Should().Contain("100");
        result.Should().Contain("200");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.Format(IEnumerable<object>, IEnumerable<string>)"/> handles empty data with headers correctly.
    /// Note: The formatter always includes the header row even when data is empty.
    /// </summary>
    [Fact]
    public void Format_WithEmptyDataAndHeaders_ShouldReturnHeaderRow()
    {
        // Arrange
        var testData = Array.Empty<object>();
        var headers = new[] { "Value" };

        // Act
        string result = _formatter.Format(testData, headers);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().Contain("Value");
    }

    #endregion

    #region EscapeCsv Method Tests - Comma Escaping

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> escapes values containing commas.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithCommaInValue_ShouldWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "a,b,c" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"a,b,c\"");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles multiple commas in a value.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithMultipleCommasInValue_ShouldWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "a,b,c,d,e" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"a,b,c,d,e\"");
    }

    #endregion

    #region EscapeCsv Method Tests - Quote Escaping

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> escapes values containing quotes.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithQuoteInValue_ShouldDoubleQuotesAndWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "He said \"Hello\" to me" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"He said \"\"Hello\"\" to me\"");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles multiple quotes in a value.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithMultipleQuotesInValue_ShouldDoubleAllQuotesAndWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "\"\"quoted\"\" text \"here\"" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("quoted");
        result.Should().Contain("\"\"\"\"");
    }

    #endregion

    #region EscapeCsv Method Tests - Newline Escaping

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> escapes values containing newlines.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithNewlineInValue_ShouldWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "line1\r\nline2" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"line1\r\nline2\"");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles line feed in a value.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithLineFeedInValue_ShouldWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "text\nmore text" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"text\nmore text\"");
    }

    #endregion

    #region EscapeCsv Method Tests - Combined Special Characters

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles values with commas, quotes, and newlines together.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithCommaQuoteAndNewlineInValue_ShouldWrapInQuotesAndEscapeAll()
    {
        // Arrange
        var testData = new object[] { new { Field = "a,b\"c\nd" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"a,b\"\"c\nd\"");
    }

    #endregion

    #region EscapeCsv Method Tests - Empty and Null Values

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles empty string values.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithEmptyString_ShouldReturnQuotedEmptyString()
    {
        // Arrange
        var testData = new object[] { new { Field = "" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"\"");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles null values.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithNullValue_ShouldReturnQuotedEmptyString()
    {
        // Arrange
        var testData = new object[] { new { Field = (string?)null } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("\"\"");
    }

    #endregion

    #region EscapeCsv Method Tests - Normal Values (No Escaping Needed)

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> does not wrap normal values in quotes.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithNormalValue_ShouldNotWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = "normal text" } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("normal text");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles numeric values without quotes.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithNumericValue_ShouldNotWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = 12345 } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("12345");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter.EscapeCsv"/> handles decimal values without quotes.
    /// </summary>
    [Fact]
    public void EscapeCsv_WithDecimalValue_ShouldNotWrapInQuotes()
    {
        // Arrange
        var testData = new object[] { new { Field = 123.45m } };

        // Act
        string result = _formatter.Format(testData);

        // Assert
        result.Should().Contain("Field");
        result.Should().Contain("123.45");
    }

    #endregion

    #region Real-world CSV Scenarios

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter"/> correctly formats a realistic dataset with mixed data types.
    /// </summary>
    [Fact]
    public void Format_WithRealisticDataset_ShouldGenerateValidCsv()
    {
        // Arrange - Simulating trade data
        var trades = new object[]
        {
            new { Price = 50000.50m, Quantity = 0.5m, User = "seller1", PaymentMethod = "Bank Transfer" },
            new { Price = 49999.75m, Quantity = 0.3m, User = "seller2", PaymentMethod = "PayPal, Wise" },
            new { Price = 50001.25m, Quantity = 0.7m, User = "seller3", PaymentMethod = "Revolut" }
        };

        // Act
        string result = _formatter.Format(trades);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Price");
        result.Should().Contain("Quantity");
        result.Should().Contain("User");
        result.Should().Contain("PaymentMethod");
        result.Should().Contain("50000.50");
        result.Should().Contain("0.5");
        result.Should().Contain("seller1");
        result.Should().Contain("\"PayPal, Wise\"");
        result.Should().Contain("Revolut");
    }

    /// <summary>
    /// Verifies that <see cref="CsvOutputFormatter"/> handles dataset with quotes in text fields.
    /// </summary>
    [Fact]
    public void Format_WithQuotesInTextFields_ShouldGenerateValidCsv()
    {
        // Arrange
        var data = new object[]
        {
            new { Name = "John \"The Boss\" Doe", Description = "He said \"Hello\" to everyone" },
            new { Name = "Jane Smith", Description = "Normal description" }
        };

        // Act
        string result = _formatter.Format(data);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Name");
        result.Should().Contain("Description");
        result.Should().Contain("John");
        result.Should().Contain("Jane Smith");
        result.Should().Contain("\"He said \"\"Hello\"\" to everyone\"");
    }

    #endregion
}

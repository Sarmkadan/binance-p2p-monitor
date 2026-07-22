using System;
using System.Collections.Generic;
using System.IO;
using BinanceP2pMonitor.Infrastructure;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public class LoggingExtensionsValidationTests
{
    #region ILogger Validate tests

    [Fact]
    public void Validate_ILogger_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        ILogger logger = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => LoggingExtensionsValidation.Validate(logger));
    }

    [Fact]
    public void Validate_ILogger_DisabledLogger_ReturnsProblem()
    {
        // Arrange
        var factory = new LoggerFactory();
        var logger = factory.CreateLogger("DisabledLogger");

        // Act
        var problems = LoggingExtensionsValidation.Validate(logger);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Logger is disabled for all log levels", problems[0]);
    }

    [Fact]
    public void Validate_ILogger_EnabledLogger_ReturnsEmptyList()
    {
        // Arrange
        var factory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = factory.CreateLogger("EnabledLogger");

        // Act
        var problems = LoggingExtensionsValidation.Validate(logger);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_ILogger_EnabledLogger_ReturnsTrue()
    {
        // Arrange
        var factory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = factory.CreateLogger("EnabledLogger");

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(logger);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_ILogger_DisabledLogger_ReturnsFalse()
    {
        // Arrange
        var factory = new LoggerFactory();
        var logger = factory.CreateLogger("DisabledLogger");

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(logger);

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region string Validate tests (logPath)

    [Fact]
    public void Validate_LogPath_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        string logPath = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => LoggingExtensionsValidation.Validate(logPath));
    }

    [Fact]
    public void Validate_LogPath_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var logPath = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(logPath));
    }

    [Fact]
    public void Validate_LogPath_WithWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var logPath = "   ";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(logPath));
    }

    [Fact]
    public void Validate_LogPath_WithInvalidCharacters_ReturnsProblem()
    {
        // Arrange
        var logPath = "invalid<>path.log";

        // Act
        var problems = LoggingExtensionsValidation.Validate(logPath);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Log path contains invalid characters", problems[0]);
    }

    [Fact]
    public void Validate_LogPath_WithValidPath_ReturnsEmptyList()
    {
        // Arrange
        var logPath = "/var/log/app.log";

        // Act
        var problems = LoggingExtensionsValidation.Validate(logPath);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_LogPath_WithValidRelativePath_ReturnsEmptyList()
    {
        // Arrange
        var logPath = "logs/app.log";

        // Act
        var problems = LoggingExtensionsValidation.Validate(logPath);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_LogPath_WithValidPath_ReturnsTrue()
    {
        // Arrange
        var logPath = "/var/log/app.log";

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(logPath);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_LogPath_WithInvalidPath_ReturnsFalse()
    {
        // Arrange
        var logPath = "invalid<>path.log";

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(logPath);

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region LogPerformance Validate tests

    [Fact]
    public void Validate_LogPerformance_WithNullOperationName_ThrowsArgumentException()
    {
        // Arrange
        string operationName = null!;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(operationName, elapsed));
    }

    [Fact]
    public void Validate_LogPerformance_WithEmptyOperationName_ThrowsArgumentException()
    {
        // Arrange
        var operationName = string.Empty;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(operationName, elapsed));
    }

    [Fact]
    public void Validate_LogPerformance_WithWhitespaceOperationName_ThrowsArgumentException()
    {
        // Arrange
        var operationName = "   ";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(operationName, elapsed));
    }

    [Fact]
    public void Validate_LogPerformance_WithNegativeElapsed_ReturnsProblem()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsed = TimeSpan.FromSeconds(-1);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operationName, elapsed);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Elapsed time cannot be negative", problems[0]);
    }

    [Fact]
    public void Validate_LogPerformance_WithTooLargeElapsed_ReturnsProblem()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsed = TimeSpan.FromMilliseconds(int.MaxValue + 1L);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operationName, elapsed);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Elapsed time exceeds maximum representable milliseconds", problems[0]);
    }

    [Fact]
    public void Validate_LogPerformance_WithLongMetadata_ReturnsProblem()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsed = TimeSpan.FromSeconds(1);
        var metadata = new string('x', 1025); // Exceeds MaxMetadataLength (1024)

        // Act
        var problems = LoggingExtensionsValidation.Validate(operationName, elapsed, metadata: metadata);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Metadata string exceeds maximum length of 1024 characters", problems[0]);
    }

    [Fact]
    public void Validate_LogPerformance_WithValidParameters_ReturnsEmptyList()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operationName, elapsed);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_LogPerformance_WithValidParametersAndMetadata_ReturnsEmptyList()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsed = TimeSpan.FromSeconds(1);
        var metadata = "test=value";

        // Act
        var problems = LoggingExtensionsValidation.Validate(operationName, elapsed, metadata: metadata);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_LogPerformance_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var operationName = "TestOperation";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(operationName, elapsed);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_LogPerformance_WithInvalidParameters_ReturnsFalse()
    {
        // Arrange
        var operationName = string.Empty;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(operationName, elapsed);

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region LogPriceChange Validate tests

    [Fact]
    public void Validate_LogPriceChange_WithNullAsset_ThrowsArgumentException()
    {
        // Arrange
        string asset = null!;
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice));
    }

    [Fact]
    public void Validate_LogPriceChange_WithEmptyAsset_ThrowsArgumentException()
    {
        // Arrange
        var asset = string.Empty;
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice));
    }

    [Fact]
    public void Validate_LogPriceChange_WithWhitespaceAsset_ThrowsArgumentException()
    {
        // Arrange
        var asset = "   ";
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice));
    }

    [Fact]
    public void Validate_LogPriceChange_WithNullFiat_ThrowsArgumentException()
    {
        // Arrange
        var asset = "BTC";
        string fiat = null!;
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice));
    }

    [Fact]
    public void Validate_LogPriceChange_WithNegativePreviousPrice_ReturnsProblem()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var previousPrice = -1m;
        var currentPrice = 101m;

        // Act
        var problems = LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Previous price cannot be negative", problems[0]);
    }

    [Fact]
    public void Validate_LogPriceChange_WithNegativeCurrentPrice_ReturnsProblem()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = -1m;

        // Act
        var problems = LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Current price cannot be negative", problems[0]);
    }

    [Fact]
    public void Validate_LogPriceChange_WithExcessivePreviousPrice_ReturnsProblem()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var previousPrice = 2_000_000m; // Exceeds MaxReasonableCryptoPrice (1_000_000)
        var currentPrice = 101m;

        // Act
        var problems = LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Previous price exceeds reasonable maximum value of $1,000,000", problems[0]);
    }

    [Fact]
    public void Validate_LogPriceChange_WithExcessiveCurrentPrice_ReturnsProblem()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 2_000_000m; // Exceeds MaxReasonableCryptoPrice (1_000_000)

        // Act
        var problems = LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Current price exceeds reasonable maximum value of $1,000,000", problems[0]);
    }

    [Fact]
    public void Validate_LogPriceChange_WithValidParameters_ReturnsEmptyList()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act
        var problems = LoggingExtensionsValidation.Validate(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_LogPriceChange_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var asset = "BTC";
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_LogPriceChange_WithInvalidParameters_ReturnsFalse()
    {
        // Arrange
        var asset = "";
        var fiat = "USD";
        var previousPrice = 100m;
        var currentPrice = 101m;

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(asset, fiat, previousPrice, currentPrice);

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region LogAlert Validate tests

    [Fact]
    public void Validate_LogAlert_WithNullAlertType_ThrowsArgumentException()
    {
        // Arrange
        string alertType = null!;
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason));
    }

    [Fact]
    public void Validate_LogAlert_WithEmptyAlertType_ThrowsArgumentException()
    {
        // Arrange
        var alertType = string.Empty;
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason));
    }

    [Fact]
    public void Validate_LogAlert_WithNullAsset_ThrowsArgumentException()
    {
        // Arrange
        var alertType = "PriceAlert";
        string asset = null!;
        var fiat = "USD";
        var reason = "Test reason";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason));
    }

    [Fact]
    public void Validate_LogAlert_WithNullFiat_ThrowsArgumentException()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        string fiat = null!;
        var reason = "Test reason";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason));
    }

    [Fact]
    public void Validate_LogAlert_WithNullReason_ThrowsArgumentException()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        string reason = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason));
    }

    [Fact]
    public void Validate_LogAlert_WithEmptyReason_ThrowsArgumentException()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason));
    }

    [Fact]
    public void Validate_LogAlert_WithLongReason_ReturnsProblem()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = new string('x', 513); // Exceeds MaxReasonLength (512)

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Alert reason exceeds maximum length of 512 characters", problems[0]);
    }

    [Fact]
    public void Validate_LogAlert_WithTooManyMetadataEntries_ReturnsProblem()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";
        var metadata = new Dictionary<string, string>();

        for (int i = 0; i < 21; i++) // Exceeds MaxMetadataEntries (20)
        {
            metadata[$"key{i}"] = $"value{i}";
        }

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason, metadata);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Metadata dictionary exceeds maximum of 20 entries", problems[0]);
    }

    [Fact]
    public void Validate_LogAlert_WithInvalidMetadataKey_ReturnsProblem()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";
        var metadata = new Dictionary<string, string>
        {
            { "", "value" } // Empty key
        };

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason, metadata);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Metadata key cannot be null, empty, or exceed 128 characters", problems[0]);
    }

    [Fact]
    public void Validate_LogAlert_WithTooLongMetadataKey_ReturnsProblem()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";
        var metadata = new Dictionary<string, string>
        {
            { new string('x', 129), "value" } // Exceeds MaxMetadataKeyLength (128)
        };

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason, metadata);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Metadata key cannot be null, empty, or exceed 128 characters", problems[0]);
    }

    [Fact]
    public void Validate_LogAlert_WithTooLongMetadataValue_ReturnsProblem()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";
        var metadata = new Dictionary<string, string>
        {
            { "key", new string('x', 257) } // Exceeds MaxMetadataValueLength (256)
        };

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason, metadata);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Metadata value exceeds maximum length of 256 characters", problems[0]);
    }

    [Fact]
    public void Validate_LogAlert_WithValidParameters_ReturnsEmptyList()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_LogAlert_WithValidParametersAndMetadata_ReturnsEmptyList()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";
        var metadata = new Dictionary<string, string>
        {
            { "price", "100" },
            { "threshold", "200" }
        };

        // Act
        var problems = LoggingExtensionsValidation.Validate(alertType, asset, fiat, reason, metadata);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_LogAlert_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var alertType = "PriceAlert";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(alertType, asset, fiat, reason);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_LogAlert_WithInvalidParameters_ReturnsFalse()
    {
        // Arrange
        var alertType = "";
        var asset = "BTC";
        var fiat = "USD";
        var reason = "Test reason";

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(alertType, asset, fiat, reason);

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region LogDatabaseOperation Validate tests

    [Fact]
    public void Validate_LogDatabaseOperation_WithNullOperation_ThrowsArgumentException()
    {
        // Arrange
        string operation = null!;
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed));
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithEmptyOperation_ThrowsArgumentException()
    {
        // Arrange
        var operation = string.Empty;
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed));
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithNullTable_ThrowsArgumentException()
    {
        // Arrange
        var operation = "Update";
        string table = null!;
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed));
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithNegativeAffectedRows_ReturnsProblem()
    {
        // Arrange
        var operation = "Update";
        var table = "Users";
        var affectedRows = -1;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Affected rows count cannot be negative", problems[0]);
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithNegativeElapsed_ReturnsProblem()
    {
        // Arrange
        var operation = "Update";
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(-1);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Elapsed time cannot be negative", problems[0]);
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithTooManyAffectedRows_ReturnsProblem()
    {
        // Arrange
        var operation = "Update";
        var table = "Users";
        var affectedRows = 2_000_000; // Exceeds MaxAffectedRows (1_000_000)
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Affected rows count exceeds reasonable maximum of 1,000,000", problems[0]);
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithTooLongElapsed_ReturnsProblem()
    {
        // Arrange
        var operation = "Update";
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromMinutes(6); // Exceeds MaxDatabaseOperationMs (300_000ms = 5min)

        // Act
        var problems = LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Database operation elapsed time exceeds reasonable maximum of 5 minutes", problems[0]);
    }

    [Fact]
    public void Validate_LogDatabaseOperation_WithValidParameters_ReturnsEmptyList()
    {
        // Arrange
        var operation = "Update";
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var problems = LoggingExtensionsValidation.Validate(operation, table, affectedRows, elapsed);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_LogDatabaseOperation_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var operation = "Update";
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(operation, table, affectedRows, elapsed);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_LogDatabaseOperation_WithInvalidParameters_ReturnsFalse()
    {
        // Arrange
        var operation = "";
        var table = "Users";
        var affectedRows = 10;
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var isValid = LoggingExtensionsValidation.IsValid(operation, table, affectedRows, elapsed);

        // Assert
        Assert.False(isValid);
    }

    #endregion
}
#nullable enable
using BinanceP2pMonitor.Utilities;
using FluentAssertions;
using Xunit;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for <see cref="FormatHelper"/> utility class.
/// Tests cover happy-path scenarios, edge cases, boundary values, and error paths.
/// </summary>
public class FormatHelperUnitTests
{
    #region FormatCurrency Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatCurrency"/> correctly formats currency values.
    /// </summary>
    [Fact]
    public void FormatCurrency_WithPositiveAmount_ShouldFormatWithCommasAndDecimals()
    {
        // Arrange
        decimal amount = 1234.56m;

        // Act
        string result = FormatHelper.FormatCurrency(amount);

        // Assert
        result.Should().Be("1,234.56");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatCurrency"/> handles zero correctly.
    /// </>
    [Fact]
    public void FormatCurrency_WithZeroAmount_ShouldReturnZeroFormatted()
    {
        // Arrange
        decimal amount = 0m;

        // Act
        string result = FormatHelper.FormatCurrency(amount);

        // Assert
        result.Should().Be("0.00");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatCurrency"/> handles negative amounts correctly.
    /// </summary>
    [Fact]
    public void FormatCurrency_WithNegativeAmount_ShouldFormatWithMinusSign()
    {
        // Arrange
        decimal amount = -1234.56m;

        // Act
        string result = FormatHelper.FormatCurrency(amount);

        // Assert
        result.Should().Be("-1,234.56");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatCurrency"/> respects custom decimal places.
    /// </summary>
    [Fact]
    public void FormatCurrency_WithCustomDecimalPlaces_ShouldUseSpecifiedPrecision()
    {
        // Arrange
        decimal amount = 1234.56789m;

        // Act
        string result = FormatHelper.FormatCurrency(amount, 4);

        // Assert
        result.Should().Be("1,234.5679");
    }

    #endregion

    #region FormatPercentage Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPercentage"/> correctly formats percentage values.
    /// </summary>
    [Fact]
    public void FormatPercentage_WithPositivePercentage_ShouldFormatWithPercentSign()
    {
        // Arrange
        decimal percentage = 12.34m;

        // Act
        string result = FormatHelper.FormatPercentage(percentage);

        // Assert
        result.Should().Be("12.34%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPercentage"/> handles zero percentage correctly.
    /// </summary>
    [Fact]
    public void FormatPercentage_WithZeroPercentage_ShouldReturnZeroPercent()
    {
        // Arrange
        decimal percentage = 0m;

        // Act
        string result = FormatHelper.FormatPercentage(percentage);

        // Assert
        result.Should().Be("0.00%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPercentage"/> handles negative percentages correctly.
    /// </summary>
    [Fact]
    public void FormatPercentage_WithNegativePercentage_ShouldIncludeMinusSign()
    {
        // Arrange
        decimal percentage = -5.67m;

        // Act
        string result = FormatHelper.FormatPercentage(percentage);

        // Assert
        result.Should().Be("-5.67%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPercentage"/> respects custom decimal places.
    /// </summary>
    [Fact]
    public void FormatPercentage_WithCustomDecimalPlaces_ShouldUseSpecifiedPrecision()
    {
        // Arrange
        decimal percentage = 12.34567m;

        // Act
        string result = FormatHelper.FormatPercentage(percentage, 4);

        // Assert
        result.Should().Be("12.3457%");
    }

    #endregion

    #region FormatTimestamp Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTimestamp"/> correctly formats timestamps.
    /// </summary>
    [Fact]
    public void FormatTimestamp_WithValidDateTime_ShouldFormatAccordingToPattern()
    {
        // Arrange
        DateTime dateTime = new DateTime(2026, 7, 21, 14, 30, 45, DateTimeKind.Utc);

        // Act
        string result = FormatHelper.FormatTimestamp(dateTime);

        // Assert
        result.Should().Be("2026-07-21 14:30:45 UTC");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTimestamp"/> respects custom format strings.
    /// </summary>
    [Fact]
    public void FormatTimestamp_WithCustomFormat_ShouldUseProvidedFormat()
    {
        // Arrange
        DateTime dateTime = new DateTime(2026, 7, 21, 14, 30, 45, DateTimeKind.Utc);

        // Act
        string result = FormatHelper.FormatTimestamp(dateTime, "MM/dd/yyyy HH:mm");

        // Assert
        result.Should().Be("07/21/2026 14:30");
    }

    #endregion

    #region FormatTimeAgo Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTimeAgo"/> correctly formats recent times.
    /// </summary>
    [Fact]
    public void FormatTimeAgo_WithRecentTime_ShouldReturnJustNow()
    {
        // Arrange
        DateTime dateTime = DateTime.UtcNow.AddSeconds(-30);

        // Act
        string result = FormatHelper.FormatTimeAgo(dateTime);

        // Assert
        result.Should().Be("just now");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTimeAgo"/> correctly formats minute-based times.
    /// </summary>
    [Fact]
    public void FormatTimeAgo_WithMinutesAgo_ShouldReturnMinutesAgo()
    {
        // Arrange
        DateTime dateTime = DateTime.UtcNow.AddMinutes(-5);

        // Act
        string result = FormatHelper.FormatTimeAgo(dateTime);

        // Assert
        result.Should().Be("5m ago");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTimeAgo"/> correctly formats hour-based times.
    /// </summary>
    [Fact]
    public void FormatTimeAgo_WithHoursAgo_ShouldReturnHoursAgo()
    {
        // Arrange
        DateTime dateTime = DateTime.UtcNow.AddHours(-3);

        // Act
        string result = FormatHelper.FormatTimeAgo(dateTime);

        // Assert
        result.Should().Be("3h ago");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTimeAgo"/> correctly formats day-based times.
    /// </summary>
    [Fact]
    public void FormatTimeAgo_WithDaysAgo_ShouldReturnDaysAgo()
    {
        // Arrange
        DateTime dateTime = DateTime.UtcNow.AddDays(-2);

        // Act
        string result = FormatHelper.FormatTimeAgo(dateTime);

        // Assert
        result.Should().Be("2d ago");
    }

    #endregion

    #region FormatLargeNumber Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatLargeNumber"/> correctly formats billions.
    /// </summary>
    [Fact]
    public void FormatLargeNumber_WithBillions_ShouldReturnBillionFormat()
    {
        // Arrange
        long number = 2_500_000_000;

        // Act
        string result = FormatHelper.FormatLargeNumber(number);

        // Assert
        result.Should().Be("2.5B");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatLargeNumber"/> correctly formats millions.
    /// </summary>
    [Fact]
    public void FormatLargeNumber_WithMillions_ShouldReturnMillionFormat()
    {
        // Arrange
        long number = 3_400_000;

        // Act
        string result = FormatHelper.FormatLargeNumber(number);

        // Assert
        result.Should().Be("3.4M");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatLargeNumber"/> correctly formats thousands.
    /// </summary>
    [Fact]
    public void FormatLargeNumber_WithThousands_ShouldReturnThousandFormat()
    {
        // Arrange
        long number = 1_500;

        // Act
        string result = FormatHelper.FormatLargeNumber(number);

        // Assert
        result.Should().Be("1.5K");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatLargeNumber"/> handles small numbers correctly.
    /// </summary>
    [Fact]
    public void FormatLargeNumber_WithSmallNumber_ShouldReturnOriginalNumber()
    {
        // Arrange
        long number = 42;

        // Act
        string result = FormatHelper.FormatLargeNumber(number);

        // Assert
        result.Should().Be("42");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatLargeNumber"/> handles zero correctly.
    /// </summary>
    [Fact]
    public void FormatLargeNumber_WithZero_ShouldReturnZero()
    {
        // Arrange
        long number = 0;

        // Act
        string result = FormatHelper.FormatLargeNumber(number);

        // Assert
        result.Should().Be("0");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatLargeNumber"/> handles negative numbers correctly.
    /// </summary>
    [Fact]
    public void FormatLargeNumber_WithNegativeNumber_ShouldReturnNegativeFormatted()
    {
        // Arrange
        long number = -1_500_000;

        // Act
        string result = FormatHelper.FormatLargeNumber(number);

        // Assert - FormatLargeNumber doesn't format negative numbers specially, just returns as string
        result.Should().Be("-1500000");
    }

    #endregion

    #region FormatTradingPair Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTradingPair/> correctly formats trading pairs.
    /// </summary>
    [Fact]
    public void FormatTradingPair_WithValidAssets_ShouldReturnUppercasePair()
    {
        // Arrange
        string asset = "btc";
        string fiat = "usd";

        // Act
        string result = FormatHelper.FormatTradingPair(asset, fiat);

        // Assert
        result.Should().Be("BTC/USD");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTradingPair/> handles already uppercase inputs.
    /// </summary>
    [Fact]
    public void FormatTradingPair_WithUppercaseInputs_ShouldReturnSameUppercasePair()
    {
        // Arrange
        string asset = "BTC";
        string fiat = "USDT";

        // Act
        string result = FormatHelper.FormatTradingPair(asset, fiat);

        // Assert
        result.Should().Be("BTC/USDT");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatTradingPair/> handles mixed case inputs.
    /// </summary>
    [Fact]
    public void FormatTradingPair_WithMixedCaseInputs_ShouldReturnUppercasePair()
    {
        // Arrange
        string asset = "Eth";
        string fiat = "bUsD";

        // Act
        string result = FormatHelper.FormatTradingPair(asset, fiat);

        // Assert
        result.Should().Be("ETH/BUSD");
    }

    #endregion

    #region FormatAlertDescription Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatAlertDescription/> correctly formats alert descriptions.
    /// </summary>
    [Fact]
    public void FormatAlertDescription_WithValidInputs_ShouldReturnFormattedDescription()
    {
        // Arrange
        string assetPair = "BTC/USDT";
        string conditionText = "above";
        decimal threshold = 5.5m;

        // Act
        string result = FormatHelper.FormatAlertDescription(assetPair, conditionText, threshold);

        // Assert
        result.Should().Be("Alert on BTC/USDT: above 5.50%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatAlertDescription/> handles zero threshold.
    /// </summary>
    [Fact]
    public void FormatAlertDescription_WithZeroThreshold_ShouldShowZeroPercent()
    {
        // Arrange
        string assetPair = "ETH/USDT";
        string conditionText = "below";
        decimal threshold = 0m;

        // Act
        string result = FormatHelper.FormatAlertDescription(assetPair, conditionText, threshold);

        // Assert
        result.Should().Be("Alert on ETH/USDT: below 0.00%");
    }

    #endregion

    #region FormatPriceChange Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPriceChange/> correctly formats positive changes without color codes.
    /// </summary>
    [Fact]
    public void FormatPriceChange_WithPositiveChange_NoColorCodes_ShouldReturnUpArrow()
    {
        // Arrange
        decimal changePercent = 3.5m;

        // Act
        string result = FormatHelper.FormatPriceChange(changePercent, false);

        // Assert
        result.Should().Be("↑ 3.50%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPriceChange/> correctly formats negative changes without color codes.
    /// </summary>
    [Fact]
    public void FormatPriceChange_WithNegativeChange_NoColorCodes_ShouldReturnDownArrow()
    {
        // Arrange
        decimal changePercent = -2.75m;

        // Act
        string result = FormatHelper.FormatPriceChange(changePercent, false);

        // Assert
        result.Should().Be("↓ 2.75%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatPriceChange/> correctly formats zero changes without color codes.
    /// </summary>
    [Fact]
    public void FormatPriceChange_WithZeroChange_NoColorCodes_ShouldReturnRightArrow()
    {
        // Arrange
        decimal changePercent = 0m;

        // Act
        string result = FormatHelper.FormatPriceChange(changePercent, false);

        // Assert
        result.Should().Be("→ 0.00%");
    }

    #endregion

    #region FormatMarketInfo Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatMarketInfo/> correctly formats market information.
    /// </summary>
    [Fact]
    public void FormatMarketInfo_WithValidInputs_ShouldReturnFormattedMarketInfo()
    {
        // Arrange
        string asset = "BTC";
        string fiat = "USDT";
        decimal price = 45000.50m;
        decimal change = 2.5m;

        // Act
        string result = FormatHelper.FormatMarketInfo(asset, fiat, price, change);

        // Assert
        result.Should().Be("BTC/USDT: 45,000.50 ↑ 2.50%");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.FormatMarketInfo/> handles negative price changes.
    /// </summary>
    [Fact]
    public void FormatMarketInfo_WithNegativeChange_ShouldShowDownArrow()
    {
        // Arrange
        string asset = "ETH";
        string fiat = "USDT";
        decimal price = 3000.75m;
        decimal change = -1.25m;

        // Act
        string result = FormatHelper.FormatMarketInfo(asset, fiat, price, change);

        // Assert
        result.Should().Be("ETH/USDT: 3,000.75 ↓ 1.25%");
    }

    #endregion

    #region WrapText Tests

    /// <summary>
    /// Verifies that <see cref="FormatHelper.WrapText/> correctly wraps text within width limit.
    /// </summary>
    [Fact]
    public void WrapText_WithShortText_ShouldReturnSingleLine()
    {
        // Arrange
        string text = "Hello world";

        // Act
        List<string> result = FormatHelper.WrapText(text, 20);

        // Assert
        result.Should().ContainSingle()
                  .Which.Should().Be("Hello world");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.WrapText/> correctly wraps long text.
    /// </summary>
    [Fact]
    public void WrapText_WithLongText_ShouldWrapAtWordBoundaries()
    {
        // Arrange
        string text = "This is a very long sentence that should be wrapped";

        // Act
        List<string> result = FormatHelper.WrapText(text, 10);

        // Assert
        result.Should().ContainInOrder(
            "This is a",
            "very long",
            "sentence",
            "that",
            "should be",
            "wrapped");
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.WrapText/> handles empty string correctly.
    /// </summary>
    [Fact]
    public void WrapText_WithEmptyString_ShouldReturnEmptyList()
    {
        // Arrange
        string text = "";

        // Act
        List<string> result = FormatHelper.WrapText(text, 80);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.WrapText/> handles null string correctly.
    /// </summary>
    [Fact]
    public void WrapText_WithNullString_ShouldThrowNullReferenceException()
    {
        // Arrange
        string text = null!;

        // Act
        Action act = () => FormatHelper.WrapText(text, 80);

        // Assert - WrapText doesn't validate null input
        act.Should().Throw<NullReferenceException>();
    }

    /// <summary>
    /// Verifies that <see cref="FormatHelper.WrapText/> respects custom max width.
    /// </summary>
    [Fact]
    public void WrapText_WithCustomMaxWidth_ShouldUseSpecifiedWidth()
    {
        // Arrange
        string text = "Word1 Word2 Word3 Word4 Word5";

        // Act
        List<string> result = FormatHelper.WrapText(text, 6);

        // Assert
        result.Should().ContainInOrder(
            "Word1",
            "Word2",
            "Word3",
            "Word4",
            "Word5");
    }

    #endregion
}
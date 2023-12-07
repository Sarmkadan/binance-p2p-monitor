#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Tests;

public class PriceCalculatorTests
{
    [Fact]
    public void CalculatePercentageChange_PriceIncreases_ReturnsPositivePercentage()
    {
        var result = PriceCalculator.CalculatePercentageChange(100m, 110m);

        result.Should().Be(10m);
    }

    [Fact]
    public void CalculatePercentageChange_PriceDecreases_ReturnsNegativePercentage()
    {
        var result = PriceCalculator.CalculatePercentageChange(200m, 150m);

        result.Should().Be(-25m);
    }

    [Fact]
    public void CalculatePercentageChange_ZeroOriginalPrice_ReturnsZero()
    {
        var result = PriceCalculator.CalculatePercentageChange(0m, 100m);

        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateSpread_BuyAndSellPrices_ReturnsCorrectSpreadPercent()
    {
        // buy=100, sell=102 => (102-100)/100 * 100 = 2%
        var result = PriceCalculator.CalculateSpread(100m, 102m);

        result.Should().Be(2m);
    }

    [Fact]
    public void CalculateSpread_ZeroBuyPrice_ReturnsZero()
    {
        var result = PriceCalculator.CalculateSpread(0m, 102m);

        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateMidPrice_TwoPrices_ReturnsArithmeticMean()
    {
        var result = PriceCalculator.CalculateMidPrice(100m, 200m);

        result.Should().Be(150m);
    }

    [Fact]
    public void CalculateMovingAverage_FewerPricesThanPeriod_ReturnsOverallAverage()
    {
        var prices = new[] { 10m, 20m, 30m };

        var result = PriceCalculator.CalculateMovingAverage(prices, period: 10);

        result.Should().Be(20m);
    }

    [Fact]
    public void CalculateMovingAverage_ExactPeriod_ReturnsLastNAverage()
    {
        var prices = new[] { 10m, 20m, 30m, 40m, 50m };

        var result = PriceCalculator.CalculateMovingAverage(prices, period: 3);

        // last 3: 30, 40, 50 => average = 40
        result.Should().Be(40m);
    }

    [Fact]
    public void CalculateStandardDeviation_SinglePrice_ReturnsZero()
    {
        var result = PriceCalculator.CalculateStandardDeviation(new[] { 42m });

        result.Should().Be(0m);
    }

    [Fact]
    public void CalculateStandardDeviation_IdenticalPrices_ReturnsZero()
    {
        var result = PriceCalculator.CalculateStandardDeviation(new[] { 5m, 5m, 5m });

        result.Should().Be(0m);
    }

    [Fact]
    public void FormatPrice_WithCurrencySymbol_PrependsCurrencySymbol()
    {
        var result = PriceCalculator.FormatPrice(1234.5m, "$", 2);

        result.Should().Be("$1234.50");
    }

    [Fact]
    public void FormatPrice_NoCurrencySymbol_ReturnsPlainDecimal()
    {
        var result = PriceCalculator.FormatPrice(99m, null, 2);

        result.Should().Be("99.00");
    }
}

public class ValidationHelperTests
{
    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("bad-email", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsValidEmail_VariousInputs_ReturnsExpectedResult(string email, bool expected)
    {
        var result = ValidationHelper.IsValidEmail(email);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("BTC", true)]
    [InlineData("USDT", true)]
    [InlineData("btc", false)]      // must be uppercase
    [InlineData("BTC/USDT", false)] // slash not allowed
    [InlineData("", false)]
    public void IsValidTicker_VariousInputs_ReturnsExpectedResult(string ticker, bool expected)
    {
        var result = ValidationHelper.IsValidTicker(ticker);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("USD", true)]
    [InlineData("EUR", true)]
    [InlineData("US", false)]   // too short
    [InlineData("USDT", false)] // too long
    [InlineData("usd", false)]  // lowercase
    public void IsValidFiatCode_VariousInputs_ReturnsExpectedResult(string code, bool expected)
    {
        var result = ValidationHelper.IsValidFiatCode(code);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidPrice_PriceWithinDefaultRange_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidPrice(50000m);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidPrice_ZeroPrice_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidPrice(0m);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTelegramChatId_PositiveId_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidTelegramChatId(123456789L);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidTelegramChatId_ZeroOrNegative_ReturnsFalse()
    {
        ValidationHelper.IsValidTelegramChatId(0L).Should().BeFalse();
        ValidationHelper.IsValidTelegramChatId(-1L).Should().BeFalse();
    }

    [Fact]
    public void IsValidDateRange_StartBeforeEnd_ReturnsTrue()
    {
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow.AddDays(-1);

        var result = ValidationHelper.IsValidDateRange(start, end);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidDateRange_StartAfterEnd_ReturnsFalse()
    {
        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow.AddDays(-7);

        var result = ValidationHelper.IsValidDateRange(start, end);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCollection_NonEmptyList_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidCollection(new[] { 1, 2, 3 });

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidCollection_EmptyList_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidCollection(Array.Empty<int>());

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCollection_NullCollection_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidCollection<int>(null);

        result.Should().BeFalse();
    }
}

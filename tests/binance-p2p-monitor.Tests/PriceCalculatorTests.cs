#nullable enable
using BinanceP2pMonitor.Utilities;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Contains unit tests for the <see cref="PriceCalculator"/> class methods.
/// Tests verify the accuracy of price calculation utilities including percentage changes,
/// spreads, moving averages, standard deviations, and price formatting.
/// </summary>
public class PriceCalculatorTests
{
    /// <summary>
    /// Tests that when the new price is higher than the original price,
    /// the percentage change calculation returns a positive percentage value.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_PriceIncreases_ReturnsPositivePercentage()
    {
        var result = PriceCalculator.CalculatePercentageChange(100m, 110m);

        result.Should().Be(10m);
    }

    /// <summary>
    /// Tests that when the new price is lower than the original price,
    /// the percentage change calculation returns a negative percentage value.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_PriceDecreases_ReturnsNegativePercentage()
    {
        var result = PriceCalculator.CalculatePercentageChange(200m, 150m);

        result.Should().Be(-25m);
    }

    /// <summary>
    /// Tests that when the original price is zero, the percentage change calculation
    /// returns zero to avoid division by zero errors and maintain mathematical correctness.
    /// </summary>
    [Fact]
    public void CalculatePercentageChange_ZeroOriginalPrice_ReturnsZero()
    {
        var result = PriceCalculator.CalculatePercentageChange(0m, 100m);

        result.Should().Be(0m);
    }

    /// <summary>
    /// Tests that the spread calculation returns the correct percentage when both buy and sell prices are non‑zero.
    /// </summary>
    [Fact]
    public void CalculateSpread_BuyAndSellPrices_ReturnsCorrectSpreadPercent()
    {
        // buy=100, sell=102 => (102-100)/100 * 100 = 2%
        var result = PriceCalculator.CalculateSpread(100m, 102m);

        result.Should().Be(2m);
    }

    /// <summary>
    /// Tests that a zero buy price results in a spread of zero to avoid division by zero.
    /// </summary>
    [Fact]
    public void CalculateSpread_ZeroBuyPrice_ReturnsZero()
    {
        var result = PriceCalculator.CalculateSpread(0m, 102m);

        result.Should().Be(0m);
    }

    /// <summary>
    /// Tests that the mid‑price calculation returns the arithmetic mean of two prices.
    /// </summary>
    [Fact]
    public void CalculateMidPrice_TwoPrices_ReturnsArithmeticMean()
    {
        var result = PriceCalculator.CalculateMidPrice(100m, 200m);

        result.Should().Be(150m);
    }

    /// <summary>
    /// Tests that when the number of supplied prices is fewer than the requested period,
    /// the moving average calculation returns the average of all available prices.
    /// </summary>
    [Fact]
    public void CalculateMovingAverage_FewerPricesThanPeriod_ReturnsOverallAverage()
    {
        var prices = new[] { 10m, 20m, 30m };

        var result = PriceCalculator.CalculateMovingAverage(prices, period: 10);

        result.Should().Be(20m);
    }

    /// <summary>
    /// Tests that when the number of supplied prices equals the requested period,
    /// the moving average calculation returns the average of the last N prices.
    /// </summary>
    [Fact]
    public void CalculateMovingAverage_ExactPeriod_ReturnsLastNAverage()
    {
        var prices = new[] { 10m, 20m, 30m, 40m, 50m };

        var result = PriceCalculator.CalculateMovingAverage(prices, period: 3);

        // last 3: 30, 40, 50 => average = 40
        result.Should().Be(40m);
    }

    /// <summary>
    /// Tests that the standard deviation of a single price is zero.
    /// </summary>
    [Fact]
    public void CalculateStandardDeviation_SinglePrice_ReturnsZero()
    {
        var result = PriceCalculator.CalculateStandardDeviation(new[] { 42m });

        result.Should().Be(0m);
    }

    /// <summary>
    /// Tests that the standard deviation of identical prices is zero.
    /// </summary>
    [Fact]
    public void CalculateStandardDeviation_IdenticalPrices_ReturnsZero()
    {
        var result = PriceCalculator.CalculateStandardDeviation(new[] { 5m, 5m, 5m });

        result.Should().Be(0m);
    }

    /// <summary>
    /// Tests that formatting a price with a currency symbol prefixes the symbol and respects the specified precision.
    /// </summary>
    [Fact]
    public void FormatPrice_WithCurrencySymbol_PrependsCurrencySymbol()
    {
        var result = PriceCalculator.FormatPrice(1234.5m, "$", 2);

        result.Should().Be("$1234.50");
    }

    /// <summary>
    /// Tests that formatting a price without a currency symbol returns only the numeric representation with the requested precision.
    /// </summary>
    [Fact]
    public void FormatPrice_NoCurrencySymbol_ReturnsPlainDecimal()
    {
        var result = PriceCalculator.FormatPrice(99m, null, 2);

        result.Should().Be("99.00");
    }
}

/// <summary>
/// Contains unit tests for the <see cref="ValidationHelper"/> static validation methods.
/// Each test verifies that the helper returns the expected boolean result for a variety of inputs.
/// </summary>
public class ValidationHelperTests
{
    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidEmail"/> returns the expected result for various email strings.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <param name="expected">The expected boolean outcome of the validation.</param>
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

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidTicker"/> returns the expected result for various ticker strings.
    /// </summary>
    /// <param name="ticker">The ticker string to validate.</param>
    /// <param name="expected">The expected boolean outcome of the validation.</param>
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

    /// <summary>
    /// Verifies that <see cref="ValidationHelper.IsValidFiatCode"/> returns the expected result for various fiat code strings.
    /// </summary>
    /// <param name="code">The fiat code string to validate.</param>
    /// <param name="expected">The expected boolean outcome of the validation.</param>
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

    /// <summary>
    /// Verifies that a price within the default acceptable range is considered valid.
    /// </summary>
    [Fact]
    public void IsValidPrice_PriceWithinDefaultRange_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidPrice(50000m);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a price of zero is considered invalid.
    /// </summary>
    [Fact]
    public void IsValidPrice_ZeroPrice_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidPrice(0m);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a positive Telegram chat identifier is considered valid.
    /// </summary>
    [Fact]
    public void IsValidTelegramChatId_PositiveId_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidTelegramChatId(123456789L);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that zero or negative Telegram chat identifiers are considered invalid.
    /// </summary>
    [Fact]
    public void IsValidTelegramChatId_ZeroOrNegative_ReturnsFalse()
    {
        ValidationHelper.IsValidTelegramChatId(0L).Should().BeFalse();
        ValidationHelper.IsValidTelegramChatId(-1L).Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a date range where the start precedes the end is considered valid.
    /// </summary>
    [Fact]
    public void IsValidDateRange_StartBeforeEnd_ReturnsTrue()
    {
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow.AddDays(-1);

        var result = ValidationHelper.IsValidDateRange(start, end);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a date range where the start follows the end is considered invalid.
    /// </summary>
    [Fact]
    public void IsValidDateRange_StartAfterEnd_ReturnsFalse()
    {
        var start = DateTime.UtcNow.AddDays(-1);
        var end = DateTime.UtcNow.AddDays(-7);

        var result = ValidationHelper.IsValidDateRange(start, end);

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a non‑empty collection is considered valid.
    /// </summary>
    [Fact]
    public void IsValidCollection_NonEmptyList_ReturnsTrue()
    {
        var result = ValidationHelper.IsValidCollection(new[] { 1, 2, 3 });

        result.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that an empty collection is considered invalid.
    /// </summary>
    [Fact]
    public void IsValidCollection_EmptyList_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidCollection(Array.Empty<int>());

        result.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that a null collection is considered invalid.
    /// </summary>
    [Fact]
    public void IsValidCollection_NullCollection_ReturnsFalse()
    {
        var result = ValidationHelper.IsValidCollection<int>(null);

        result.Should().BeFalse();
    }
}

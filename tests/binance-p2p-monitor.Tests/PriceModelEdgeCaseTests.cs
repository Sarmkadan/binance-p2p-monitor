#nullable enable
using FluentAssertions;
using BinanceP2pMonitor.Models;
using Xunit;

namespace BinanceP2pMonitor.Tests;

public sealed class PriceModelEdgeCaseTests
{
    [Fact]
    public void CalculateSpread_ZeroBuyPrice_ReturnsZero()
    {
        var price = new Price { BuyPrice = 0, SellPrice = 100 };
        price.CalculateSpread().Should().Be(0);
    }

    [Fact]
    public void CalculateSpread_EqualPrices_ReturnsZero()
    {
        var price = new Price { BuyPrice = 100, SellPrice = 100 };
        price.CalculateSpread().Should().Be(0);
    }

    [Fact]
    public void CalculateSpread_PositiveSpread_ReturnsCorrectPercentage()
    {
        var price = new Price { BuyPrice = 100, SellPrice = 105 };
        price.CalculateSpread().Should().Be(5);
    }

    [Fact]
    public void CalculateSpread_NegativeSpread_ReturnsNegativePercentage()
    {
        var price = new Price { BuyPrice = 100, SellPrice = 95 };
        price.CalculateSpread().Should().Be(-5);
    }

    [Fact]
    public void CalculateSpread_VerySmallBuyPrice_DoesNotThrow()
    {
        var price = new Price { BuyPrice = 0.00000001m, SellPrice = 0.00000002m };
        var act = () => price.CalculateSpread();
        act.Should().NotThrow();
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var price = new Price();
        price.Asset.Should().BeEmpty();
        price.Fiat.Should().BeEmpty();
        price.History.Should().BeEmpty();
    }

    [Fact]
    public void CalculateSpread_LargeValues_DoesNotOverflow()
    {
        var price = new Price { BuyPrice = 1_000_000m, SellPrice = 1_000_001m };
        var spread = price.CalculateSpread();
        spread.Should().BeGreaterThan(0);
    }
}

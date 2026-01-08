// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Tests;

public class PriceAlertTests
{
    private static PriceAlert BuildAlert(AlertCondition condition, decimal threshold, bool enabled = true) =>
        new()
        {
            Asset = "BTC",
            Fiat = "USD",
            AlertType = AlertType.PriceChange,
            Condition = condition,
            Threshold = threshold,
            IsEnabled = enabled,
            UserId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    [Fact]
    public void ShouldTrigger_GreaterThanConditionAndValueExceedsThreshold_ReturnsTrue()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);

        alert.ShouldTrigger(currentChange: 6m).Should().BeTrue();
    }

    [Fact]
    public void ShouldTrigger_GreaterThanConditionButValueBelowThreshold_ReturnsFalse()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);

        alert.ShouldTrigger(currentChange: 4m).Should().BeFalse();
    }

    [Fact]
    public void ShouldTrigger_AlertIsDisabled_AlwaysReturnsFalse()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m, enabled: false);

        alert.ShouldTrigger(currentChange: 100m).Should().BeFalse();
    }

    [Fact]
    public void ShouldTrigger_LessThanConditionAndValueBelowThreshold_ReturnsTrue()
    {
        var alert = BuildAlert(AlertCondition.LessThan, threshold: 3m);

        alert.ShouldTrigger(currentChange: 2m).Should().BeTrue();
    }

    [Fact]
    public void ShouldTrigger_EqualsConditionWithinTolerance_ReturnsTrue()
    {
        var alert = BuildAlert(AlertCondition.Equals, threshold: 10m);

        // tolerance is 0.01m
        alert.ShouldTrigger(currentChange: 10.005m).Should().BeTrue();
    }

    [Fact]
    public void RecordTrigger_IncrementsTriggerCountAndSetsTimestamp()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);
        alert.TriggerCount = 0;

        alert.RecordTrigger();

        alert.TriggerCount.Should().Be(1);
        alert.LastTriggeredAt.Should().NotBeNull();
    }

    [Fact]
    public void IsInCooldownPeriod_NeverTriggered_ReturnsFalse()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);
        alert.LastTriggeredAt = null;

        alert.IsInCooldownPeriod(cooldownMinutes: 5).Should().BeFalse();
    }

    [Fact]
    public void Toggle_EnabledAlert_BecomesDisabled()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m, enabled: true);

        alert.Toggle();

        alert.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Toggle_DisabledAlert_BecomesEnabled()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m, enabled: false);

        alert.Toggle();

        alert.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void GetDescription_GreaterThanCondition_ContainsCorrectOperator()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 2.5m);

        var description = alert.GetDescription();

        description.Should().Contain(">");
        description.Should().Contain("BTC");
        description.Should().Contain("USD");
        description.Should().Contain("2.5");
    }

    [Fact]
    public void IsValid_WellFormedAlert_ReturnsTrue()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);

        alert.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_EmptyAsset_ReturnsFalse()
    {
        var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);
        alert.Asset = string.Empty;

        alert.IsValid().Should().BeFalse();
    }
}

public class SpreadTests
{
    private static Spread BuildSpread(decimal current, decimal avg, decimal min, decimal max) =>
        new()
        {
            Asset = "BTC",
            Fiat = "USD",
            CurrentSpreadPercent = current,
            AverageSpreadPercent = avg,
            MinSpreadPercent = min,
            MaxSpreadPercent = max,
            SampleCount = 10,
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

    [Theory]
    [InlineData(0.1, "Very Low")]
    [InlineData(0.5, "Low")]
    [InlineData(0.8, "Medium")]
    [InlineData(1.2, "High")]
    [InlineData(2.0, "Very High")]
    public void GetRiskLevel_VariousSpreadValues_ReturnsCorrectLevel(double spread, string expected)
    {
        var s = BuildSpread((decimal)spread, 0.5m, 0m, 3m);

        s.GetRiskLevel().Should().Be(expected);
    }

    [Fact]
    public void IsHighSpread_AboveDefaultThreshold_ReturnsTrue()
    {
        var s = BuildSpread(current: 2m, avg: 1m, min: 0.2m, max: 3m);

        s.IsHighSpread().Should().BeTrue();
    }

    [Fact]
    public void IsLowSpread_BelowDefaultThreshold_ReturnsTrue()
    {
        var s = BuildSpread(current: 0.1m, avg: 0.5m, min: 0.1m, max: 1m);

        s.IsLowSpread().Should().BeTrue();
    }

    [Fact]
    public void IsNormal_CurrentWithinMinMax_ReturnsTrue()
    {
        var s = BuildSpread(current: 0.5m, avg: 0.5m, min: 0.3m, max: 0.8m);

        s.IsNormal().Should().BeTrue();
    }

    [Fact]
    public void IsNormal_CurrentAboveMax_ReturnsFalse()
    {
        var s = BuildSpread(current: 1.5m, avg: 0.5m, min: 0.3m, max: 0.8m);

        s.IsNormal().Should().BeFalse();
    }

    [Fact]
    public void GetVarianceFromAverage_ZeroAverage_ReturnsZero()
    {
        var s = BuildSpread(current: 1m, avg: 0m, min: 0m, max: 2m);

        s.GetVarianceFromAverage().Should().Be(0m);
    }

    [Fact]
    public void GetVarianceFromAverage_CurrentDoubleAverage_Returns100Percent()
    {
        var s = BuildSpread(current: 2m, avg: 1m, min: 0m, max: 3m);

        s.GetVarianceFromAverage().Should().Be(100m);
    }

    [Fact]
    public void UpdateStatistics_NewSample_UpdatesCurrentAndSampleCount()
    {
        var s = BuildSpread(current: 1m, avg: 1m, min: 0.5m, max: 1.5m);
        s.SampleCount = 10;

        s.UpdateStatistics(1.2m);

        s.CurrentSpreadPercent.Should().Be(1.2m);
        s.SampleCount.Should().Be(11);
    }

    [Fact]
    public void UpdateStatistics_NewMinimum_UpdatesMinSpread()
    {
        var s = BuildSpread(current: 1m, avg: 1m, min: 0.5m, max: 1.5m);
        s.SampleCount = 5;

        s.UpdateStatistics(0.1m);

        s.MinSpreadPercent.Should().Be(0.1m);
    }

    [Fact]
    public void IsValid_WellFormedSpread_ReturnsTrue()
    {
        var s = BuildSpread(current: 0.5m, avg: 0.5m, min: 0.3m, max: 0.8m);

        s.IsValid().Should().BeTrue();
    }
}

public class TradeOfferTests
{
    private static TradeOffer BuildOffer(bool isActive = true, decimal rating = 95m,
        decimal minAmount = 100m, decimal maxAmount = 10000m) =>
        new()
        {
            OfferIdFromBinance = "offer-001",
            Asset = "BTC",
            Fiat = "USD",
            TradeType = TradeType.Buy,
            Price = 50000m,
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            TraderRating = rating,
            CompletedTrades = 500,
            IsActive = isActive,
            Timestamp = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    [Fact]
    public void MatchesCriteria_ActiveOfferMeetsAllCriteria_ReturnsTrue()
    {
        var offer = BuildOffer(isActive: true, rating: 95m, minAmount: 100m, maxAmount: 10000m);

        offer.MatchesCriteria(minRating: 90m, minAmount: 200m, maxAmount: 5000m).Should().BeTrue();
    }

    [Fact]
    public void MatchesCriteria_InactiveOffer_ReturnsFalse()
    {
        var offer = BuildOffer(isActive: false);

        offer.MatchesCriteria(minRating: 90m, minAmount: 100m, maxAmount: 5000m).Should().BeFalse();
    }

    [Fact]
    public void MatchesCriteria_RatingBelowMinimum_ReturnsFalse()
    {
        var offer = BuildOffer(rating: 80m);

        offer.MatchesCriteria(minRating: 90m, minAmount: 100m, maxAmount: 5000m).Should().BeFalse();
    }

    [Fact]
    public void CanTradeAmount_AmountWithinRange_ReturnsTrue()
    {
        var offer = BuildOffer(minAmount: 100m, maxAmount: 10000m);

        offer.CanTradeAmount(500m).Should().BeTrue();
    }

    [Fact]
    public void CanTradeAmount_AmountBelowMinimum_ReturnsFalse()
    {
        var offer = BuildOffer(minAmount: 100m, maxAmount: 10000m);

        offer.CanTradeAmount(50m).Should().BeFalse();
    }

    [Fact]
    public void CanTradeAmount_AmountAboveMaximum_ReturnsFalse()
    {
        var offer = BuildOffer(minAmount: 100m, maxAmount: 10000m);

        offer.CanTradeAmount(20000m).Should().BeFalse();
    }

    [Fact]
    public void CalculatePremium_PriceAboveReference_ReturnsPositivePremium()
    {
        var offer = BuildOffer();
        offer.Price = 51000m;

        var premium = offer.CalculatePremium(referencePrice: 50000m);

        premium.Should().Be(2m);
    }

    [Fact]
    public void CalculatePremium_ZeroReferencePrice_ReturnsZero()
    {
        var offer = BuildOffer();

        offer.CalculatePremium(referencePrice: 0m).Should().Be(0m);
    }

    [Fact]
    public void GetAvailableRange_ReturnsMaxMinusMin()
    {
        var offer = BuildOffer(minAmount: 100m, maxAmount: 5000m);

        offer.GetAvailableRange().Should().Be(4900m);
    }

    [Fact]
    public void IsValid_WellFormedOffer_ReturnsTrue()
    {
        var offer = BuildOffer();

        offer.IsValid().Should().BeTrue();
    }

    [Fact]
    public void IsValid_MaxAmountLessThanMinAmount_ReturnsFalse()
    {
        var offer = BuildOffer(minAmount: 5000m, maxAmount: 100m);

        offer.IsValid().Should().BeFalse();
    }
}

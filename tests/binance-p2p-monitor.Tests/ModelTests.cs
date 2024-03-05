#nullable enable
using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Tests;

/// <summary>
/// Unit tests for the <see cref="PriceAlert"/> class functionality.
/// Tests the alert triggering logic, state management, and validation.
/// </summary>
public class PriceAlertTests
{
	/// <summary>
	/// Helper method to create a test <see cref="PriceAlert"/> with specified parameters.
	/// </summary>
	/// <param name="condition">The alert condition to test (GreaterThan, LessThan, or Equals).</param>
	/// <param name="threshold">The threshold value for the alert.</param>
	/// <param name="enabled">Whether the alert should be enabled (default: true).</param>
	/// <returns>A configured <see cref="PriceAlert"/> instance for testing.</returns>
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

	/// <summary>
	/// Tests that ShouldTrigger returns true when using GreaterThan condition and the current change exceeds the threshold.
	/// </summary>
	[Fact]
	public void ShouldTrigger_GreaterThanConditionAndValueExceedsThreshold_ReturnsTrue()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);

		alert.ShouldTrigger(currentChange: 6m).Should().BeTrue();
	}

	/// <summary>
	/// Tests that ShouldTrigger returns false when using GreaterThan condition but the current change is below the threshold.
	/// </summary>
	[Fact]
	public void ShouldTrigger_GreaterThanConditionButValueBelowThreshold_ReturnsFalse()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);

		alert.ShouldTrigger(currentChange: 4m).Should().BeFalse();
	}

	/// <summary>
	/// Tests that ShouldTrigger always returns false when the alert is disabled, regardless of threshold.
	/// </summary>
	[Fact]
	public void ShouldTrigger_AlertIsDisabled_AlwaysReturnsFalse()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m, enabled: false);

		alert.ShouldTrigger(currentChange: 100m).Should().BeFalse();
	}

	/// <summary>
	/// Tests that ShouldTrigger returns true when using LessThan condition and the current change is below the threshold.
	/// </summary>
	[Fact]
	public void ShouldTrigger_LessThanConditionAndValueBelowThreshold_ReturnsTrue()
	{
		var alert = BuildAlert(AlertCondition.LessThan, threshold: 3m);

		alert.ShouldTrigger(currentChange: 2m).Should().BeTrue();
	}

	/// <summary>
	/// Tests that ShouldTrigger returns true when using Equals condition and the current change is within tolerance.
	/// </summary>
	[Fact]
	public void ShouldTrigger_EqualsConditionWithinTolerance_ReturnsTrue()
	{
		var alert = BuildAlert(AlertCondition.Equals, threshold: 10m);

		// tolerance is 0.01m
		alert.ShouldTrigger(currentChange: 10.005m).Should().BeTrue();
	}

	/// <summary>
	/// Tests that RecordTrigger increments the trigger count and sets the LastTriggeredAt timestamp.
	/// </summary>
	[Fact]
	public void RecordTrigger_IncrementsTriggerCountAndSetsTimestamp()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);
		alert.TriggerCount = 0;

		alert.RecordTrigger();

		alert.TriggerCount.Should().Be(1);
		alert.LastTriggeredAt.Should().NotBeNull();
	}

	/// <summary>
	/// Tests that IsInCooldownPeriod returns false when the alert has never been triggered.
	/// </summary>
	[Fact]
	public void IsInCooldownPeriod_NeverTriggered_ReturnsFalse()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);
		alert.LastTriggeredAt = null;

		alert.IsInCooldownPeriod(cooldownMinutes: 5).Should().BeFalse();
	}

	/// <summary>
	/// Tests that Toggle changes an enabled alert to disabled.
	/// </summary>
	[Fact]
	public void Toggle_EnabledAlert_BecomesDisabled()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m, enabled: true);

		alert.Toggle();

		alert.IsEnabled.Should().BeFalse();
	}

	/// <summary>
	/// Tests that Toggle changes a disabled alert to enabled.
	/// </summary>
	[Fact]
	public void Toggle_DisabledAlert_BecomesEnabled()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m, enabled: false);

		alert.Toggle();

		alert.IsEnabled.Should().BeTrue();
	}

	/// <summary>
	/// Tests that GetDescription returns a description containing the correct operator, asset, fiat, and threshold for GreaterThan condition.
	/// </summary>
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

	/// <summary>
	/// Tests that IsValid returns true for a well-formed alert with all required fields populated.
	/// </summary>
	[Fact]
	public void IsValid_WellFormedAlert_ReturnsTrue()
	{
		var alert = BuildAlert(AlertCondition.GreaterThan, threshold: 5m);

		alert.IsValid().Should().BeTrue();
	}

	/// <summary>
	/// Tests that IsValid returns false when the Asset field is empty.
	/// </summary>
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

	/// <summary>
	/// Tests that GetRiskLevel returns the correct risk level for various spread values.
	/// </summary>
	/// <param name="spread">The spread value to test.</param>
	/// <param name="expected">The expected risk level string.</param>
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

	/// <summary>
	/// Tests that IsHighSpread returns true when the current spread is above the default threshold.
	/// </summary>
	[Fact]
	public void IsHighSpread_AboveDefaultThreshold_ReturnsTrue()
	{
		var s = BuildSpread(current: 2m, avg: 1m, min: 0.2m, max: 3m);

		s.IsHighSpread().Should().BeTrue();
	}

	/// <summary>
	/// Tests that IsLowSpread returns true when the current spread is below the default threshold.
	/// </summary>
	[Fact]
	public void IsLowSpread_BelowDefaultThreshold_ReturnsTrue()
	{
		var s = BuildSpread(current: 0.1m, avg: 0.5m, min: 0.1m, max: 1m);

		s.IsLowSpread().Should().BeTrue();
	}

	/// <summary>
	/// Tests that IsNormal returns true when the current spread is within the min/max range.
	/// </summary>
	[Fact]
	public void IsNormal_CurrentWithinMinMax_ReturnsTrue()
	{
		var s = BuildSpread(current: 0.5m, avg: 0.5m, min: 0.3m, max: 0.8m);

		s.IsNormal().Should().BeTrue();
	}

	/// <summary>
	/// Tests that IsNormal returns false when the current spread is above the max value.
	/// </summary>
	[Fact]
	public void IsNormal_CurrentAboveMax_ReturnsFalse()
	{
		var s = BuildSpread(current: 1.5m, avg: 0.5m, min: 0.3m, max: 0.8m);

		s.IsNormal().Should().BeFalse();
	}

	/// <summary>
	/// Tests that GetVarianceFromAverage returns zero when the average spread is zero.
	/// </summary>
	[Fact]
	public void GetVarianceFromAverage_ZeroAverage_ReturnsZero()
	{
		var s = BuildSpread(current: 1m, avg: 0m, min: 0m, max: 2m);

		s.GetVarianceFromAverage().Should().Be(0m);
	}

	/// <summary>
	/// Tests that GetVarianceFromAverage returns 100% when the current spread is double the average.
	/// </summary>
	[Fact]
	public void GetVarianceFromAverage_CurrentDoubleAverage_Returns100Percent()
	{
		var s = BuildSpread(current: 2m, avg: 1m, min: 0m, max: 3m);

		s.GetVarianceFromAverage().Should().Be(100m);
	}

	/// <summary>
	/// Tests that UpdateStatistics updates the current spread and increments the sample count.
	/// </summary>
	[Fact]
	public void UpdateStatistics_NewSample_UpdatesCurrentAndSampleCount()
	{
		var s = BuildSpread(current: 1m, avg: 1m, min: 0.5m, max: 1.5m);
		s.SampleCount = 10;

		s.UpdateStatistics(1.2m);

		s.CurrentSpreadPercent.Should().Be(1.2m);
		s.SampleCount.Should().Be(11);
	}

	/// <summary>
	/// Tests that UpdateStatistics updates the minimum spread when a new minimum is found.
	/// </summary>
	[Fact]
	public void UpdateStatistics_NewMinimum_UpdatesMinSpread()
	{
		var s = BuildSpread(current: 1m, avg: 1m, min: 0.5m, max: 1.5m);
		s.SampleCount = 5;

		s.UpdateStatistics(0.1m);

		s.MinSpreadPercent.Should().Be(0.1m);
	}

	/// <summary>
	/// Tests that IsValid returns true for a well-formed Spread with all required fields populated.
	/// </summary>
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

	/// <summary>
	/// Tests that MatchesCriteria returns true when an active offer meets all specified criteria.
	/// </summary>
	[Fact]
	public void MatchesCriteria_ActiveOfferMeetsAllCriteria_ReturnsTrue()
	{
		var offer = BuildOffer(isActive: true, rating: 95m, minAmount: 100m, maxAmount: 10000m);

		offer.MatchesCriteria(minRating: 90m, minAmount: 200m, maxAmount: 5000m).Should().BeTrue();
	}

	/// <summary>
	/// Tests that MatchesCriteria returns false when an offer is inactive.
	/// </summary>
	[Fact]
	public void MatchesCriteria_InactiveOffer_ReturnsFalse()
	{
		var offer = BuildOffer(isActive: false);

		offer.MatchesCriteria(minRating: 90m, minAmount: 100m, maxAmount: 5000m).Should().BeFalse();
	}

	/// <summary>
	/// Tests that MatchesCriteria returns false when the offer's rating is below the minimum required.
	/// </summary>
	[Fact]
	public void MatchesCriteria_RatingBelowMinimum_ReturnsFalse()
	{
		var offer = BuildOffer(rating: 80m);

		offer.MatchesCriteria(minRating: 90m, minAmount: 100m, maxAmount: 5000m).Should().BeFalse();
	}

	/// <summary>
	/// Tests that CanTradeAmount returns true when the requested amount is within the offer's min/max range.
	/// </summary>
	[Fact]
	public void CanTradeAmount_AmountWithinRange_ReturnsTrue()
	{
		var offer = BuildOffer(minAmount: 100m, maxAmount: 10000m);

		offer.CanTradeAmount(500m).Should().BeTrue();
	}

	/// <summary>
	/// Tests that CanTradeAmount returns false when the requested amount is below the offer's minimum.
	/// </summary>
	[Fact]
	public void CanTradeAmount_AmountBelowMinimum_ReturnsFalse()
	{
		var offer = BuildOffer(minAmount: 100m, maxAmount: 10000m);

		offer.CanTradeAmount(50m).Should().BeFalse();
	}

	/// <summary>
	/// Tests that CanTradeAmount returns false when the requested amount is above the offer's maximum.
	/// </summary>
	[Fact]
	public void CanTradeAmount_AmountAboveMaximum_ReturnsFalse()
	{
		var offer = BuildOffer(minAmount: 100m, maxAmount: 10000m);

		offer.CanTradeAmount(20000m).Should().BeFalse();
	}

	/// <summary>
	/// Tests that CalculatePremium returns a positive value when the offer price is above the reference price.
	/// </summary>
	[Fact]
	public void CalculatePremium_PriceAboveReference_ReturnsPositivePremium()
	{
		var offer = BuildOffer();
		offer.Price = 51000m;

		var premium = offer.CalculatePremium(referencePrice: 50000m);

		premium.Should().Be(2m);
	}

	/// <summary>
	/// Tests that CalculatePremium returns zero when the reference price is zero.
	/// </summary>
	[Fact]
	public void CalculatePremium_ZeroReferencePrice_ReturnsZero()
	{
		var offer = BuildOffer();

		offer.CalculatePremium(referencePrice: 0m).Should().Be(0m);
	}

	/// <summary>
	/// Tests that GetAvailableRange returns the difference between max and min amounts.
	/// </summary>
	[Fact]
	public void GetAvailableRange_ReturnsMaxMinusMin()
	{
		var offer = BuildOffer(minAmount: 100m, maxAmount: 5000m);

		offer.GetAvailableRange().Should().Be(4900m);
	}

	/// <summary>
	/// Tests that IsValid returns true for a well-formed TradeOffer with all required fields populated.
	/// </summary>
	[Fact]
	public void IsValid_WellFormedOffer_ReturnsTrue()
	{
		var offer = BuildOffer();

		offer.IsValid().Should().BeTrue();
	}

	/// <summary>
	/// Tests that IsValid returns false when the max amount is less than the min amount.
	/// </summary>
	[Fact]
	public void IsValid_MaxAmountLessThanMinAmount_ReturnsFalse()
	{
		var offer = BuildOffer(minAmount: 5000m, maxAmount: 100m);

		offer.IsValid().Should().BeFalse();
	}
}
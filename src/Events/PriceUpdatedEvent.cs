#nullable enable
namespace BinanceP2pMonitor.Events;

/// <summary>
/// Event fired when a price is updated
/// </summary>
public class PriceUpdatedEvent : DomainEvent
{
    /// <summary>
    /// Gets the event type identifier, <c>price.updated</c>.
    /// </summary>
    public override string EventType => "price.updated";

    /// <summary>
    /// Gets or sets the code of the crypto asset whose price changed.
    /// </summary>
    public string Asset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the code of the fiat currency the price is quoted in.
    /// </summary>
    public string Fiat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current best buy price.
    /// </summary>
    public decimal BuyPrice { get; set; }

    /// <summary>
    /// Gets or sets the current best sell price.
    /// </summary>
    public decimal SellPrice { get; set; }

    /// <summary>
    /// Gets or sets the buy price observed before this update.
    /// </summary>
    public decimal PreviousBuyPrice { get; set; }

    /// <summary>
    /// Gets or sets the sell price observed before this update.
    /// </summary>
    public decimal PreviousSellPrice { get; set; }

    /// <summary>
    /// Gets or sets the number of buy offers taken into account for this update.
    /// </summary>
    public int BuyOfferCount { get; set; }

    /// <summary>
    /// Gets or sets the number of sell offers taken into account for this update.
    /// </summary>
    public int SellOfferCount { get; set; }
}

/// <summary>
/// Event fired when a spread threshold is exceeded
/// </summary>
public class SpreadAlertTriggeredEvent : DomainEvent
{
    /// <summary>
    /// Gets the event type identifier, <c>spread.alert</c>.
    /// </summary>
    public override string EventType => "spread.alert";

    /// <summary>
    /// Gets or sets the code of the crypto asset the spread was measured for.
    /// </summary>
    public string Asset { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the code of the fiat currency the spread is quoted in.
    /// </summary>
    public string Fiat { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the measured spread, in percent.
    /// </summary>
    public decimal SpreadPercentage { get; set; }

    /// <summary>
    /// Gets or sets the spread percentage that was exceeded to trigger this event.
    /// </summary>
    public decimal Threshold { get; set; }

    /// <summary>
    /// Gets or sets the buy price used to calculate the spread.
    /// </summary>
    public decimal BuyPrice { get; set; }

    /// <summary>
    /// Gets or sets the sell price used to calculate the spread.
    /// </summary>
    public decimal SellPrice { get; set; }
}

/// <summary>
/// Event fired when an alert is sent
/// </summary>
public class AlertSentEvent : DomainEvent
{
    /// <summary>
    /// Gets the event type identifier, <c>alert.sent</c>.
    /// </summary>
    public override string EventType => "alert.sent";

    /// <summary>
    /// Gets or sets the unique identifier of the alert that was sent.
    /// </summary>
    public Guid AlertId { get; set; }

    /// <summary>
    /// Gets or sets the kind of alert that was sent (e.g. price or spread).
    /// </summary>
    public string AlertType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text of the alert message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recipient the alert was addressed to (e.g. a Telegram chat or webhook target).
    /// </summary>
    public string Recipient { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the alert was delivered successfully.
    /// </summary>
    public bool Success { get; set; }
}

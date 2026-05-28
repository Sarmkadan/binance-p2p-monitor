#nullable enable
namespace BinanceP2pMonitor.Events;

/// <summary>
/// Event fired when a price is updated
/// </summary>
public class PriceUpdatedEvent : DomainEvent
{
    public override string EventType => "price.updated";

    public string Asset { get; set; } = string.Empty;
    public string Fiat { get; set; } = string.Empty;
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public decimal PreviousBuyPrice { get; set; }
    public decimal PreviousSellPrice { get; set; }
    public int BuyOfferCount { get; set; }
    public int SellOfferCount { get; set; }
}

/// <summary>
/// Event fired when a spread threshold is exceeded
/// </summary>
public class SpreadAlertTriggeredEvent : DomainEvent
{
    public override string EventType => "spread.alert";

    public string Asset { get; set; } = string.Empty;
    public string Fiat { get; set; } = string.Empty;
    public decimal SpreadPercentage { get; set; }
    public decimal Threshold { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
}

/// <summary>
/// Event fired when an alert is sent
/// </summary>
public class AlertSentEvent : DomainEvent
{
    public override string EventType => "alert.sent";

    public Guid AlertId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public bool Success { get; set; }
}

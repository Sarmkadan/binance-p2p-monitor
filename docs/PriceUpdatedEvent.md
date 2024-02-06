# PriceUpdatedEvent

`PriceUpdatedEvent` represents a notification payload emitted when the monitored price for a given asset–fiat pair crosses a configured threshold. It captures the current buy and sell prices, the previous snapshot for comparison, the computed spread, offer counts, and the outcome of any alert delivery attempt triggered by the price movement.

## API

### `public string Asset`
The cryptocurrency asset code (e.g. `"USDT"`, `"BTC"`). Identifies the base asset being monitored on the Binance P2P platform.

### `public string Fiat`
The fiat currency code (e.g. `"ARS"`, `"VES"`). Identifies the quote currency against which the asset is priced.

### `public decimal BuyPrice`
The current best buy price (bid) for the asset–fiat pair, expressed in units of the fiat currency.

### `public decimal SellPrice`
The current best sell price (ask) for the asset–fiat pair, expressed in units of the fiat currency.

### `public decimal PreviousBuyPrice`
The best buy price recorded in the immediately preceding evaluation cycle. Used to compute the direction and magnitude of the price change.

### `public decimal PreviousSellPrice`
The best sell price recorded in the immediately preceding evaluation cycle. Used to compute the direction and magnitude of the price change.

### `public int BuyOfferCount`
The number of active P2P buy offers available for the pair at the time of evaluation.

### `public int SellOfferCount`
The number of active P2P sell offers available for the pair at the time of evaluation.

### `public decimal SpreadPercentage`
The computed spread between the current buy and sell prices, expressed as a percentage. Typically calculated as `((SellPrice - BuyPrice) / BuyPrice) * 100`.

### `public decimal Threshold`
The configured price-movement threshold that triggered this event. The exact interpretation (absolute difference, percentage change, or spread limit) depends on the alert rule definition.

### `public Guid AlertId`
A unique identifier assigned to the alert instance that produced this event. Remains consistent across multiple deliveries for the same alert configuration.

### `public string AlertType`
A string classifying the type of alert that fired (e.g. `"PriceDrop"`, `"PriceSpike"`, `"SpreadTooHigh"`). The value is determined by the alert rule that matched the price movement.

### `public string Message`
The human-readable notification text generated for this event. May include formatted prices, percentages, and the asset–fiat pair.

### `public string Recipient`
The target destination for the alert delivery. Depending on the notifier implementation, this may be an email address, a Telegram chat ID, a webhook URL, or another channel identifier.

### `public bool Success`
Indicates whether the alert delivery attempt completed without errors. A value of `true` means the notifier reported a successful send; `false` indicates a delivery failure (e.g., network error, invalid recipient).

## Usage

### Example 1: Handling a price spike alert

```csharp
void HandlePriceUpdated(PriceUpdatedEvent e)
{
    if (!e.Success)
    {
        Logger.Warn($"Alert {e.AlertId} delivery to {e.Recipient} failed");
        return;
    }

    Console.WriteLine(
        $"[{e.AlertType}] {e.Asset}/{e.Fiat}: " +
        $"Buy {e.PreviousBuyPrice} -> {e.BuyPrice}, " +
        $"Sell {e.PreviousSellPrice} -> {e.SellPrice}, " +
        $"Spread {e.SpreadPercentage:F2}%");
}
```

### Example 2: Filtering events by spread and offer availability

```csharp
bool IsLiquidSpreadAlert(PriceUpdatedEvent e)
{
    const decimal MaxSpreadPercent = 2.0m;
    const int MinOffers = 5;

    return e.SpreadPercentage <= MaxSpreadPercent
           && e.BuyOfferCount >= MinOffers
           && e.SellOfferCount >= MinOffers
           && e.Success;
}

IEnumerable<PriceUpdatedEvent> FilterLiquidAlerts(IEnumerable<PriceUpdatedEvent> events)
{
    return events.Where(IsLiquidSpreadAlert);
}
```

## Notes

- **Duplicate members**: The signatures list `Asset`, `Fiat`, `BuyPrice`, and `SellPrice` multiple times. In practice these represent a single set of properties; consumers should treat them as the canonical values for the event and not expect distinct overloads or separate backing fields.
- **Price precision**: All `decimal` price members carry the full precision provided by the Binance P2P data source. Rounding for display is the consumer’s responsibility.
- **Zero or negative values**: `BuyPrice`, `SellPrice`, `PreviousBuyPrice`, and `PreviousSellPrice` may be zero if no offers exist for the pair at evaluation time. `SpreadPercentage` can be negative if `BuyPrice` exceeds `SellPrice` (an inverted market), though this is rare on P2P exchanges.
- **Alert delivery failure**: When `Success` is `false`, the remaining fields still contain the price snapshot and alert metadata. Consumers should not discard the event solely based on delivery failure; the price data remains valid for logging, retry logic, or analytics.
- **Thread safety**: `PriceUpdatedEvent` is an immutable data transfer object. Its properties are read-only after construction. Instances can be safely shared across threads without synchronization.
- **Threshold semantics**: The `Threshold` field’s meaning is defined by the alert rule that generated the event. It may represent a percentage change, an absolute price difference, or a spread limit. Always interpret it in conjunction with `AlertType`.

# TradeOffer

Represents a single peer-to-peer trade advertisement retrieved from the Binance P2P platform. The class encapsulates all relevant offer details—asset pair, pricing, trade limits, counterparty reputation, and payment methods—and provides computed properties for premium calculation, validity checks, and trade amount feasibility. It serves as the core data model for monitoring, filtering, and analysing P2P trading opportunities.

## API

### `public int Id`
Unique internal identifier assigned by the local database or repository. Used for persistence and relational mappings.

### `public string OfferIdFromBinance`
The original advertisement identifier issued by the Binance P2P platform. This value is immutable once set and is used to detect duplicate offers and to correlate local records with the remote source.

### `public string Asset`
The cryptocurrency ticker being traded (e.g. `"USDT"`, `"BTC"`, `"ETH"`). Case-sensitive and must match Binance’s internal representation.

### `public string Fiat`
The fiat currency ticker (e.g. `"ARS"`, `"VES"`, `"USD"`). Case-sensitive and must match Binance’s internal representation.

### `public TradeType TradeType`
Indicates whether the offer is a buy or sell from the perspective of the advertiser. The `TradeType` enum typically contains `Buy` and `Sell` values.

### `public decimal Price`
The advertised unit price of the asset expressed in the fiat currency. This is the raw value from the platform and is used as the basis for premium calculations.

### `public decimal MinAmount`
The minimum trade amount allowed by the advertiser, denominated in the fiat currency. A trade order below this threshold will be rejected by the counterparty.

### `public decimal MaxAmount`
The maximum trade amount allowed by the advertiser, denominated in the fiat currency. A trade order above this threshold will be rejected by the counterparty.

### `public decimal TraderRating`
The counterparty’s aggregated rating score as reported by Binance. Typically a value between 0 and 5, where higher values indicate greater trustworthiness.

### `public int CompletedTrades`
The total number of successfully completed trades for the counterparty over the preceding 30 days, as reported by Binance.

### `public string PaymentMethods`
A comma-separated or otherwise delimited list of payment method names accepted by the advertiser (e.g. `"Bank Transfer, Mercado Pago"`). Parsing logic is external to this class.

### `public bool IsActive`
Indicates whether the offer is currently listed and available on Binance P2P. Inactive offers are retained for historical tracking but should be excluded from active monitoring logic.

### `public DateTime Timestamp`
The point in time when the offer was observed or scraped from the Binance P2P platform. This reflects the remote state, not the local record creation time.

### `public DateTime CreatedAt`
The UTC timestamp when this `TradeOffer` record was first persisted locally. Set once at insertion and never modified thereafter.

### `public DateTime UpdatedAt`
The UTC timestamp of the most recent modification to this local record. Updated automatically on any field change.

### `public bool MatchesCriteria`
A computed flag indicating whether the offer satisfies the currently configured monitoring criteria (e.g. minimum rating, premium thresholds, accepted payment methods). The evaluation logic is external and the property is set by the criteria engine after analysis.

### `public decimal CalculatePremium`
Computes the percentage premium or discount of the offer’s price relative to a reference price.  
- **Parameters:** None (relies on internal state and an externally provided reference price, typically injected via a service or static context).  
- **Returns:** A `decimal` representing the premium as a percentage. Positive values indicate the offer price is above the reference; negative values indicate it is below.  
- **Throws:** `InvalidOperationException` if the reference price has not been initialised or is zero.

### `public bool IsValid`
Performs a basic sanity check on the offer’s mandatory fields.  
- **Parameters:** None.  
- **Returns:** `true` if `Asset`, `Fiat`, `OfferIdFromBinance`, and `Price` are non-null/non-empty and `Price` is greater than zero, and `MinAmount` does not exceed `MaxAmount`; otherwise `false`.  
- **Throws:** Never throws—designed as a defensive check.

### `public decimal GetAvailableRange`
Returns the absolute spread between the minimum and maximum trade amounts.  
- **Parameters:** None.  
- **Returns:** `MaxAmount - MinAmount`.  
- **Throws:** Never throws. Returns zero if both limits are equal.

### `public bool CanTradeAmount`
Determines whether a proposed trade amount falls within the offer’s limits.  
- **Parameters:** A `decimal` representing the desired trade amount in the fiat currency.  
- **Returns:** `true` if the amount is greater than or equal to `MinAmount` and less than or equal to `MaxAmount`; otherwise `false`.  
- **Throws:** `ArgumentOutOfRangeException` if the provided amount is negative.

## Usage

### Example 1: Filtering active offers that match criteria and support a specific payment method

```csharp
IEnumerable<TradeOffer> activeOffers = allOffers
    .Where(o => o.IsActive && o.MatchesCriteria)
    .Where(o => o.PaymentMethods.Contains("Mercado Pago"))
    .OrderByDescending(o => o.CalculatePremium)
    .ToList();

foreach (var offer in activeOffers)
{
    Console.WriteLine(
        $"Offer {offer.OfferIdFromBinance}: {offer.Asset}/{offer.Fiat} " +
        $"@ {offer.Price} ({offer.CalculatePremium:P2}) | " +
        $"Limits: {offer.MinAmount}–{offer.MaxAmount} {offer.Fiat}");
}
```

### Example 2: Validating an offer and checking trade feasibility before placing an order

```csharp
TradeOffer candidate = repository.GetByBinanceId("abc123-def456-ghi789");

if (!candidate.IsValid)
{
    Console.WriteLine("Offer failed validation — skipping.");
    return;
}

decimal desiredAmount = 1500.00m;

if (!candidate.CanTradeAmount(desiredAmount))
{
    decimal range = candidate.GetAvailableRange();
    Console.WriteLine(
        $"Amount {desiredAmount} {candidate.Fiat} is outside the allowed range. " +
        $"Offer accepts {candidate.MinAmount}–{candidate.MaxAmount} {candidate.Fiat} " +
        $"(spread: {range} {candidate.Fiat}).");
    return;
}

Console.WriteLine(
    $"Proceeding with trade: {desiredAmount} {candidate.Fiat} of {candidate.Asset} " +
    $"at {candidate.Price} per unit. Counterparty rating: {candidate.TraderRating} " +
    $"({candidate.CompletedTrades} completed trades).");
```

## Notes

- **Thread safety:** This class is not inherently thread-safe. `MatchesCriteria`, `CalculatePremium`, and the mutable timestamps (`UpdatedAt`, `IsActive`) may be modified by background monitoring or criteria-evaluation threads. External synchronisation (e.g. locking or immutable snapshots) is required when instances are shared across concurrent operations.
- **`CalculatePremium` dependency:** The method requires a reference price to be available in the execution context. If the reference price has not been set (e.g. during initialisation or after a cache flush), the call will throw `InvalidOperationException`. Callers should guard with a try-catch or check the reference price availability beforehand.
- **`IsValid` vs. `MatchesCriteria`:** `IsValid` performs a structural integrity check (required fields present, amounts logically consistent). `MatchesCriteria` reflects business-rule evaluation (rating, premium, payment methods). An offer can be valid but not match criteria, and vice versa—though the latter typically indicates a configuration or data-quality issue.
- **`CanTradeAmount` edge case:** Passing a negative amount throws `ArgumentOutOfRangeException`. Passing zero returns `true` only if `MinAmount` is also zero, which is rare but possible for newly created or unrestricted offers.
- **`GetAvailableRange` precision:** The return value is a raw `decimal` subtraction. No rounding is applied. In scenarios where `MinAmount` and `MaxAmount` are equal, the range is zero, which may require special handling in UI or logging to avoid displaying a meaningless spread.
- **`PaymentMethods` parsing:** The property stores a raw string. Any splitting, normalisation, or case-insensitive matching must be performed by the consumer. No delimiter guarantees are enforced by the class itself.
- **`Timestamp` vs. `CreatedAt`:** `Timestamp` reflects the observation time on the Binance platform and may be updated on every scrape. `CreatedAt` is the local insertion time and never changes. This distinction is critical for change-detection and audit-trail logic.

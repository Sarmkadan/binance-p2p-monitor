# TradeOfferExtensions

Provides comparison, calculation, and formatting utilities for trade offers within the Binance P2P monitor domain. These extension methods operate on trade offer objects to evaluate competitiveness, derive midpoint pricing, compute volume coverage, and produce human-readable payment method summaries.

## API

### `IsBetterThan`

```csharp
public static bool IsBetterThan(this TradeOffer current, TradeOffer other)
```

Determines whether the `current` offer is strictly more favourable than the `other` offer from the perspective of the intended trade direction. The comparison logic is direction-aware: for buy-side offers a lower price is considered better, while for sell-side offers a higher price is considered better. When prices are equal, the offer with the higher available volume is deemed superior.

**Parameters**
- `current` *(this TradeOffer)* — The offer being evaluated.
- `other` *(TradeOffer)* — The reference offer to compare against.

**Return Value**
`true` if `current` is strictly better than `other`; otherwise `false`.

**Exceptions**
- `ArgumentNullException` — Thrown when either `current` or `other` is `null`.

---

### `GetMidpointAmount`

```csharp
public static decimal GetMidpointAmount(this TradeOffer offer)
```

Calculates the midpoint between the offer’s minimum and maximum trade limits. This value represents the average transaction size the counterparty is willing to accommodate.

**Parameters**
- `offer` *(this TradeOffer)* — The offer whose limits are used for calculation.

**Return Value**
A `decimal` representing `(MinAmount + MaxAmount) / 2`.

**Exceptions**
- `ArgumentNullException` — Thrown when `offer` is `null`.

---

### `GetVolumePercentage`

```csharp
public static decimal GetVolumePercentage(this TradeOffer offer, decimal targetAmount)
```

Computes what percentage of the `targetAmount` can be fulfilled by this offer’s available volume, capped at 100%. Useful for assessing how much of a desired trade quantity a single offer can cover.

**Parameters**
- `offer` *(this TradeOffer)* — The offer whose available volume is evaluated.
- `targetAmount` *(decimal)* — The desired trade amount to measure coverage against.

**Return Value**
A `decimal` between `0` and `100` inclusive, representing the fulfilment percentage. Returns `100` when the offer’s available volume meets or exceeds `targetAmount`.

**Exceptions**
- `ArgumentNullException` — Thrown when `offer` is `null`.
- `ArgumentOutOfRangeException` — Thrown when `targetAmount` is zero or negative.

---

### `FormatPaymentMethods`

```csharp
public static string FormatPaymentMethods(this TradeOffer offer)
```

Produces a comma-separated string of payment method names associated with the offer, suitable for display in logs or user interfaces. An offer with no payment methods returns a predefined placeholder string.

**Parameters**
- `offer` *(this TradeOffer)* — The offer whose payment methods are formatted.

**Return Value**
A `string` containing the joined payment method names, or a fallback indicator such as `"None"` when the collection is empty.

**Exceptions**
- `ArgumentNullException` — Thrown when `offer` is `null`.

## Usage

```csharp
// Compare two sell offers to find the most competitive one
TradeOffer offerA = repository.GetOffer("seller-alpha");
TradeOffer offerB = repository.GetOffer("seller-beta");

if (offerA.IsBetterThan(offerB))
{
    Console.WriteLine($"Offer A is better at {offerA.Price} with volume {offerA.AvailableVolume}");
    Console.WriteLine($"Payment methods: {offerA.FormatPaymentMethods()}");
}
else
{
    Console.WriteLine($"Offer B is better at {offerB.Price} with volume {offerB.AvailableVolume}");
    Console.WriteLine($"Payment methods: {offerB.FormatPaymentMethods()}");
}
```

```csharp
// Assess whether a single offer can fully cover a desired purchase
decimal desiredAmount = 1500m;
TradeOffer candidate = repository.GetBestBuyOffer("USDT");

decimal midpoint = candidate.GetMidpointAmount();
decimal coverage = candidate.GetVolumePercentage(desiredAmount);

Console.WriteLine($"Offer midpoint: {midpoint:F2} USDT");
Console.WriteLine($"Coverage for {desiredAmount} USDT: {coverage:F1}%");
Console.WriteLine($"Methods: {candidate.FormatPaymentMethods()}");

if (coverage < 100m)
{
    Console.WriteLine("Warning: single offer cannot fully cover the desired amount.");
}
```

## Notes

- **Null handling**: All methods throw `ArgumentNullException` when the `offer` argument is `null`. Callers should guard against null references before invoking these extensions.
- **`GetVolumePercentage` edge cases**: When `targetAmount` is zero or negative, an `ArgumentOutOfRangeException` is thrown. The result is always clamped to `100` even if the offer’s volume vastly exceeds the target.
- **`IsBetterThan` tie-breaking**: When two offers have identical prices, volume becomes the deciding factor. If both price and volume are equal, the method returns `false` (neither is strictly better).
- **`FormatPaymentMethods` empty collection**: Returns a non-null, non-empty fallback string when no payment methods are present. Callers can safely use the result in string concatenation without additional null checks.
- **Thread safety**: These methods are pure functions that operate on their input arguments without mutating state or accessing shared resources. They are safe to call concurrently from multiple threads provided the `TradeOffer` instances themselves are not being modified during invocation.

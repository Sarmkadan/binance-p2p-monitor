#nullable enable

using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository for managing TradeOffer data
/// </summary>
public class TradeOfferRepository : ITradeOfferRepository
{
    private readonly DatabaseContext _context;

    public TradeOfferRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<TradeOffer?> GetByIdAsync(int id)
    {
        try
        {
            const string sql = @"
                SELECT Id, OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                       TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt
                FROM TradeOffers WHERE Id = @Id";

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Id", id } });
                return reader.Read() ? MapToTradeOfferAsync(reader).Result : null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve trade offer by ID", ex);
        }
    }

    public async Task<TradeOffer?> GetByBinanceIdAsync(string binanceId)
    {
        try
        {
            const string sql = @"
                SELECT Id, OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                       TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt
                FROM TradeOffers WHERE OfferIdFromBinance = @BinanceId";

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "BinanceId", binanceId } });
                return reader.Read() ? MapToTradeOfferAsync(reader).Result : null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve trade offer by Binance ID", ex);
        }
    }

    public async Task<IEnumerable<TradeOffer>> GetAllActiveAsync()
    {
        try
        {
            const string sql = @"
                SELECT Id, OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                       TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt
                FROM TradeOffers WHERE IsActive = 1
                ORDER BY Price ASC, TraderRating DESC";

            var offers = new List<TradeOffer>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql);
                while (reader.Read())
                {
                    var offer = MapToTradeOfferAsync(reader).Result;
                    if (offer is not null)
                        offers.Add(offer);
                }
                return offers;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve active trade offers", ex);
        }
    }

    public async Task<IEnumerable<TradeOffer>> GetByAssetAndFiatAsync(string asset, string fiat)
    {
        try
        {
            const string sql = @"
                SELECT Id, OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                       TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt
                FROM TradeOffers
                WHERE Asset = @Asset AND Fiat = @Fiat AND IsActive = 1
                ORDER BY Price ASC, TraderRating DESC";

            var offers = new List<TradeOffer>();
            var parameters = new Dictionary<string, object> { { "Asset", asset }, { "Fiat", fiat } };

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, parameters);
                while (reader.Read())
                {
                    var offer = MapToTradeOfferAsync(reader).Result;
                    if (offer is not null)
                        offers.Add(offer);
                }
                return offers;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve trade offers by asset and fiat", ex);
        }
    }

    public async Task<IEnumerable<TradeOffer>> GetByTradeTypeAsync(int tradeType)
    {
        try
        {
            const string sql = @"
                SELECT Id, OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                       TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt
                FROM TradeOffers WHERE TradeType = @TradeType AND IsActive = 1
                ORDER BY Price ASC";

            var offers = new List<TradeOffer>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "TradeType", tradeType } });
                while (reader.Read())
                {
                    var offer = MapToTradeOfferAsync(reader).Result;
                    if (offer is not null)
                        offers.Add(offer);
                }
                return offers;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve trade offers by type", ex);
        }
    }

    public async Task<IEnumerable<TradeOffer>> GetBestOffersAsync(string asset, string fiat, int limit = 10)
    {
        try
        {
            const string sql = @"
                SELECT Id, OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                       TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt
                FROM TradeOffers
                WHERE Asset = @Asset AND Fiat = @Fiat AND IsActive = 1
                ORDER BY Price ASC, TraderRating DESC, CompletedTrades DESC
                LIMIT @Limit";

            var offers = new List<TradeOffer>();
            var parameters = new Dictionary<string, object> { { "Asset", asset }, { "Fiat", fiat }, { "Limit", limit } };

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, parameters);
                while (reader.Read())
                {
                    var offer = MapToTradeOfferAsync(reader).Result;
                    if (offer is not null)
                        offers.Add(offer);
                }
                return offers;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve best trade offers", ex);
        }
    }

    public async Task<int> AddAsync(TradeOffer offer)
    {
        try
        {
            if (offer is null || !offer.IsValid())
                throw new InvalidPriceException("Trade offer data is invalid");

            const string sql = @"
                INSERT INTO TradeOffers
                (OfferIdFromBinance, Asset, Fiat, TradeType, Price, MinAmount, MaxAmount,
                 TraderRating, CompletedTrades, PaymentMethods, IsActive, Timestamp, CreatedAt, UpdatedAt)
                VALUES (@BinanceId, @Asset, @Fiat, @TradeType, @Price, @MinAmount, @MaxAmount,
                        @Rating, @Completed, @Methods, @IsActive, @Timestamp, @CreatedAt, @UpdatedAt);
                SELECT last_insert_rowid();";

            var parameters = new Dictionary<string, object>
            {
                { "BinanceId", offer.OfferIdFromBinance },
                { "Asset", offer.Asset },
                { "Fiat", offer.Fiat },
                { "TradeType", offer.TradeType },
                { "Price", offer.Price },
                { "MinAmount", offer.MinAmount },
                { "MaxAmount", offer.MaxAmount },
                { "Rating", offer.TraderRating },
                { "Completed", offer.CompletedTrades },
                { "Methods", offer.PaymentMethods },
                { "IsActive", offer.IsActive ? 1 : 0 },
                { "Timestamp", offer.Timestamp },
                { "CreatedAt", offer.CreatedAt },
                { "UpdatedAt", offer.UpdatedAt }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result is not null ? Convert.ToInt32(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to add trade offer", ex);
        }
    }

    public async Task<bool> UpdateAsync(TradeOffer offer)
    {
        try
        {
            if (offer is null || offer.Id <= 0)
                throw new InvalidPriceException("Trade offer data is invalid");

            const string sql = @"
                UPDATE TradeOffers
                SET Price = @Price, MinAmount = @MinAmount, MaxAmount = @MaxAmount,
                    TraderRating = @Rating, CompletedTrades = @Completed, IsActive = @IsActive,
                    Timestamp = @Timestamp, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "Id", offer.Id },
                { "Price", offer.Price },
                { "MinAmount", offer.MinAmount },
                { "MaxAmount", offer.MaxAmount },
                { "Rating", offer.TraderRating },
                { "Completed", offer.CompletedTrades },
                { "IsActive", offer.IsActive ? 1 : 0 },
                { "Timestamp", offer.Timestamp },
                { "UpdatedAt", DateTime.UtcNow }
            };

            return await Task.Run(() => _context.ExecuteCommand(sql, parameters) > 0).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to update trade offer", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            const string sql = "DELETE FROM TradeOffers WHERE Id = @Id";
            return await Task.Run(() => _context.ExecuteCommand(sql, new Dictionary<string, object> { { "Id", id } }) > 0).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to delete trade offer", ex);
        }
    }

    public async Task<long> GetTotalOffersCountAsync(string asset, string fiat)
    {
        try
        {
            const string sql = @"
                SELECT COUNT(*) FROM TradeOffers
                WHERE Asset = @Asset AND Fiat = @Fiat AND IsActive = 1";

            var parameters = new Dictionary<string, object> { { "Asset", asset }, { "Fiat", fiat } };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result is not null ? Convert.ToInt64(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to count trade offers", ex);
        }
    }

    public async Task<decimal> GetAveragePriceAsync(string asset, string fiat)
    {
        try
        {
            const string sql = @"
                SELECT AVG(Price) FROM TradeOffers
                WHERE Asset = @Asset AND Fiat = @Fiat AND IsActive = 1";

            var parameters = new Dictionary<string, object> { { "Asset", asset }, { "Fiat", fiat } };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result is not null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to get average offer price", ex);
        }
    }

    private async Task<TradeOffer?> MapToTradeOfferAsync(SqliteDataReader reader)
    {
        return await Task.FromResult(new TradeOffer
        {
            Id = reader.GetInt32(0),
            OfferIdFromBinance = reader.GetString(1),
            Asset = reader.GetString(2),
            Fiat = reader.GetString(3),
            TradeType = (Constants.TradeType)reader.GetInt32(4),
            Price = (decimal)reader.GetDouble(5),
            MinAmount = (decimal)reader.GetDouble(6),
            MaxAmount = (decimal)reader.GetDouble(7),
            TraderRating = (decimal)reader.GetDouble(8),
            CompletedTrades = reader.GetInt32(9),
            PaymentMethods = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            IsActive = reader.GetBoolean(11),
            Timestamp = reader.GetDateTime(12),
            CreatedAt = reader.GetDateTime(13),
            UpdatedAt = reader.GetDateTime(14)
        });
    }
}

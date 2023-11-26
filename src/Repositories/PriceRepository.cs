// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Data.SQLite;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository for managing Price data
/// </summary>
public class PriceRepository : IPriceRepository
{
    private readonly DatabaseContext _context;

    public PriceRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves a price by ID
    /// </summary>
    public async Task<Price?> GetByIdAsync(int id)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent,
                       SellChangePercent, Timestamp, CreatedAt, UpdatedAt, Metadata
                FROM Prices WHERE Id = @Id";

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Id", id } });
                return reader.Read() ? MapToPriceAsync(reader).Result : null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve price by ID", ex);
        }
    }

    /// <summary>
    /// Retrieves the latest price for an asset/fiat pair
    /// </summary>
    public async Task<Price?> GetLatestByAssetAndFiatAsync(string asset, string fiat)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent,
                       SellChangePercent, Timestamp, CreatedAt, UpdatedAt, Metadata
                FROM Prices
                WHERE Asset = @Asset AND Fiat = @Fiat
                ORDER BY UpdatedAt DESC LIMIT 1";

            var parameters = new Dictionary<string, object> { { "Asset", asset }, { "Fiat", fiat } };

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, parameters);
                return reader.Read() ? MapToPriceAsync(reader).Result : null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve latest price", ex);
        }
    }

    /// <summary>
    /// Retrieves all active prices
    /// </summary>
    public async Task<IEnumerable<Price>> GetAllActiveAsync()
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent,
                       SellChangePercent, Timestamp, CreatedAt, UpdatedAt, Metadata
                FROM Prices
                ORDER BY UpdatedAt DESC";

            var prices = new List<Price>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql);
                while (reader.Read())
                {
                    var price = MapToPriceAsync(reader).Result;
                    if (price != null)
                        prices.Add(price);
                }
                return prices;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve all prices", ex);
        }
    }

    /// <summary>
    /// Retrieves prices by asset
    /// </summary>
    public async Task<IEnumerable<Price>> GetByAssetAsync(string asset)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent,
                       SellChangePercent, Timestamp, CreatedAt, UpdatedAt, Metadata
                FROM Prices WHERE Asset = @Asset
                ORDER BY Fiat ASC";

            var prices = new List<Price>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Asset", asset } });
                while (reader.Read())
                {
                    var price = MapToPriceAsync(reader).Result;
                    if (price != null)
                        prices.Add(price);
                }
                return prices;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve prices by asset", ex);
        }
    }

    /// <summary>
    /// Retrieves prices by fiat currency
    /// </summary>
    public async Task<IEnumerable<Price>> GetByFiatAsync(string fiat)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent,
                       SellChangePercent, Timestamp, CreatedAt, UpdatedAt, Metadata
                FROM Prices WHERE Fiat = @Fiat
                ORDER BY Asset ASC";

            var prices = new List<Price>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Fiat", fiat } });
                while (reader.Read())
                {
                    var price = MapToPriceAsync(reader).Result;
                    if (price != null)
                        prices.Add(price);
                }
                return prices;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve prices by fiat", ex);
        }
    }

    /// <summary>
    /// Inserts a new price
    /// </summary>
    public async Task<int> AddAsync(Price price)
    {
        try
        {
            if (price == null || !price.IsValid())
                throw new InvalidPriceException("Price data is invalid");

            const string sql = @"
                INSERT OR REPLACE INTO Prices
                (Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent, SellChangePercent,
                 Timestamp, CreatedAt, UpdatedAt, Metadata)
                VALUES (@Asset, @Fiat, @BuyPrice, @SellPrice, @BuyChangePercent,
                        @SellChangePercent, @Timestamp, @CreatedAt, @UpdatedAt, @Metadata);
                SELECT last_insert_rowid();";

            var parameters = new Dictionary<string, object>
            {
                { "Asset", price.Asset },
                { "Fiat", price.Fiat },
                { "BuyPrice", price.BuyPrice },
                { "SellPrice", price.SellPrice },
                { "BuyChangePercent", price.BuyChangePercent },
                { "SellChangePercent", price.SellChangePercent },
                { "Timestamp", price.Timestamp },
                { "CreatedAt", price.CreatedAt },
                { "UpdatedAt", price.UpdatedAt },
                { "Metadata", price.Metadata ?? (object)DBNull.Value }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result != null ? Convert.ToInt32(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to add price", ex);
        }
    }

    /// <summary>
    /// Updates an existing price
    /// </summary>
    public async Task<bool> UpdateAsync(Price price)
    {
        try
        {
            if (price == null || price.Id <= 0 || !price.IsValid())
                throw new InvalidPriceException("Price data is invalid");

            const string sql = @"
                UPDATE Prices
                SET BuyPrice = @BuyPrice, SellPrice = @SellPrice,
                    BuyChangePercent = @BuyChangePercent, SellChangePercent = @SellChangePercent,
                    Timestamp = @Timestamp, UpdatedAt = @UpdatedAt, Metadata = @Metadata
                WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "Id", price.Id },
                { "BuyPrice", price.BuyPrice },
                { "SellPrice", price.SellPrice },
                { "BuyChangePercent", price.BuyChangePercent },
                { "SellChangePercent", price.SellChangePercent },
                { "Timestamp", price.Timestamp },
                { "UpdatedAt", DateTime.UtcNow },
                { "Metadata", price.Metadata ?? (object)DBNull.Value }
            };

            return await Task.Run(() => _context.ExecuteCommand(sql, parameters) > 0);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to update price", ex);
        }
    }

    /// <summary>
    /// Deletes a price
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            const string sql = "DELETE FROM Prices WHERE Id = @Id";
            return await Task.Run(() => _context.ExecuteCommand(sql, new Dictionary<string, object> { { "Id", id } }) > 0);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to delete price", ex);
        }
    }

    /// <summary>
    /// Retrieves prices that changed since a specific time
    /// </summary>
    public async Task<IEnumerable<Price>> GetPricesChangedSinceAsync(DateTime since)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, BuyPrice, SellPrice, BuyChangePercent,
                       SellChangePercent, Timestamp, CreatedAt, UpdatedAt, Metadata
                FROM Prices WHERE UpdatedAt >= @Since
                ORDER BY UpdatedAt DESC";

            var prices = new List<Price>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Since", since } });
                while (reader.Read())
                {
                    var price = MapToPriceAsync(reader).Result;
                    if (price != null)
                        prices.Add(price);
                }
                return prices;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve changed prices", ex);
        }
    }

    /// <summary>
    /// Calculates average price over specified hours
    /// </summary>
    public async Task<decimal?> GetAveragePriceAsync(string asset, string fiat, int hours)
    {
        try
        {
            const string sql = @"
                SELECT AVG((BuyPrice + SellPrice) / 2) as AvgPrice
                FROM Prices
                WHERE Asset = @Asset AND Fiat = @Fiat
                AND datetime(Timestamp) >= datetime('now', '-' || @Hours || ' hours')";

            var parameters = new Dictionary<string, object>
            {
                { "Asset", asset },
                { "Fiat", fiat },
                { "Hours", hours }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : (decimal?)null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to calculate average price", ex);
        }
    }

    /// <summary>
    /// Maps SQLite reader to Price object
    /// </summary>
    private async Task<Price?> MapToPriceAsync(SQLiteDataReader reader)
    {
        return await Task.FromResult(new Price
        {
            Id = reader.GetInt32(0),
            Asset = reader.GetString(1),
            Fiat = reader.GetString(2),
            BuyPrice = (decimal)reader.GetDouble(3),
            SellPrice = (decimal)reader.GetDouble(4),
            BuyChangePercent = (decimal)reader.GetDouble(5),
            SellChangePercent = (decimal)reader.GetDouble(6),
            Timestamp = reader.GetDateTime(7),
            CreatedAt = reader.GetDateTime(8),
            UpdatedAt = reader.GetDateTime(9),
            Metadata = reader.IsDBNull(10) ? null : reader.GetString(10)
        });
    }
}

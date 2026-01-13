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
/// Repository for managing PriceHistory data
/// </summary>
public class HistoryRepository : IHistoryRepository
{
    private readonly DatabaseContext _context;

    public HistoryRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PriceHistory?> GetByIdAsync(int id)
    {
        try
        {
            const string sql = @"
                SELECT Id, PriceId, Asset, Fiat, BuyPrice, SellPrice, RecordedAt,
                       CreatedAt, SpreadPercentage, PriceChangePercent, Notes
                FROM PriceHistory WHERE Id = @Id";

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Id", id } });
                return reader.Read() ? MapToPriceHistory(reader) : null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve price history by ID", ex);
        }
    }

    public async Task<IEnumerable<PriceHistory>> GetHistoryByAssetAndFiatAsync(string asset, string fiat, int hours = 24)
    {
        try
        {
            const string sql = @"
                SELECT Id, PriceId, Asset, Fiat, BuyPrice, SellPrice, RecordedAt,
                       CreatedAt, SpreadPercentage, PriceChangePercent, Notes
                FROM PriceHistory
                WHERE Asset = @Asset AND Fiat = @Fiat
                AND datetime(RecordedAt) >= datetime('now', '-' || @Hours || ' hours')
                ORDER BY RecordedAt DESC";

            var history = new List<PriceHistory>();
            var parameters = new Dictionary<string, object>
            {
                { "Asset", asset },
                { "Fiat", fiat },
                { "Hours", hours }
            };

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, parameters);
                while (reader.Read())
                {
                    history.Add(MapToPriceHistory(reader));
                }
                return history;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve price history", ex);
        }
    }

    public async Task<IEnumerable<PriceHistory>> GetRecentHistoryAsync(int minutes = 60)
    {
        try
        {
            const string sql = @"
                SELECT Id, PriceId, Asset, Fiat, BuyPrice, SellPrice, RecordedAt,
                       CreatedAt, SpreadPercentage, PriceChangePercent, Notes
                FROM PriceHistory
                WHERE datetime(RecordedAt) >= datetime('now', '-' || @Minutes || ' minutes')
                ORDER BY RecordedAt DESC";

            var history = new List<PriceHistory>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Minutes", minutes } });
                while (reader.Read())
                {
                    history.Add(MapToPriceHistory(reader));
                }
                return history;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve recent history", ex);
        }
    }

    public async Task<IEnumerable<PriceHistory>> GetHistoryByDateRangeAsync(string asset, string fiat, DateTime from, DateTime to)
    {
        try
        {
            const string sql = @"
                SELECT Id, PriceId, Asset, Fiat, BuyPrice, SellPrice, RecordedAt,
                       CreatedAt, SpreadPercentage, PriceChangePercent, Notes
                FROM PriceHistory
                WHERE Asset = @Asset AND Fiat = @Fiat
                AND RecordedAt >= @From AND RecordedAt <= @To
                ORDER BY RecordedAt DESC";

            var history = new List<PriceHistory>();
            var parameters = new Dictionary<string, object>
            {
                { "Asset", asset },
                { "Fiat", fiat },
                { "From", from },
                { "To", to }
            };

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, parameters);
                while (reader.Read())
                {
                    history.Add(MapToPriceHistory(reader));
                }
                return history;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve history by date range", ex);
        }
    }

    public async Task<int> AddAsync(PriceHistory history)
    {
        try
        {
            if (history == null || !history.IsValid())
                throw new InvalidPriceException("Price history data is invalid");

            const string sql = @"
                INSERT INTO PriceHistory
                (PriceId, Asset, Fiat, BuyPrice, SellPrice, RecordedAt, CreatedAt,
                 SpreadPercentage, PriceChangePercent, Notes)
                VALUES (@PriceId, @Asset, @Fiat, @BuyPrice, @SellPrice, @RecordedAt,
                        @CreatedAt, @SpreadPercentage, @PriceChangePercent, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new Dictionary<string, object>
            {
                { "PriceId", history.PriceId },
                { "Asset", history.Asset },
                { "Fiat", history.Fiat },
                { "BuyPrice", history.BuyPrice },
                { "SellPrice", history.SellPrice },
                { "RecordedAt", history.RecordedAt },
                { "CreatedAt", history.CreatedAt },
                { "SpreadPercentage", (object)history.SpreadPercentage },
                { "PriceChangePercent", (object)history.PriceChangePercent },
                { "Notes", history.Notes ?? (object)DBNull.Value }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result != null ? Convert.ToInt32(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to add price history", ex);
        }
    }

    public async Task<bool> DeleteOldRecordsAsync(int daysOld)
    {
        try
        {
            const string sql = @"
                DELETE FROM PriceHistory
                WHERE datetime(CreatedAt) <= datetime('now', '-' || @DaysOld || ' days')";

            return await Task.Run(() =>
            {
                _context.ExecuteCommand(sql, new Dictionary<string, object> { { "DaysOld", daysOld } });
                return true;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to delete old history records", ex);
        }
    }

    public async Task<long> GetTotalHistoryCountAsync()
    {
        try
        {
            const string sql = "SELECT COUNT(*) FROM PriceHistory";

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql);
                return result != null ? Convert.ToInt64(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to get history count", ex);
        }
    }

    public async Task<decimal> GetHighestPriceAsync(string asset, string fiat, int hours)
    {
        try
        {
            const string sql = @"
                SELECT MAX(CASE WHEN BuyPrice > SellPrice THEN BuyPrice ELSE SellPrice END)
                FROM PriceHistory
                WHERE Asset = @Asset AND Fiat = @Fiat
                AND datetime(RecordedAt) >= datetime('now', '-' || @Hours || ' hours')";

            var parameters = new Dictionary<string, object>
            {
                { "Asset", asset },
                { "Fiat", fiat },
                { "Hours", hours }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to get highest price", ex);
        }
    }

    public async Task<decimal> GetLowestPriceAsync(string asset, string fiat, int hours)
    {
        try
        {
            const string sql = @"
                SELECT MIN(CASE WHEN BuyPrice < SellPrice THEN BuyPrice ELSE SellPrice END)
                FROM PriceHistory
                WHERE Asset = @Asset AND Fiat = @Fiat
                AND datetime(RecordedAt) >= datetime('now', '-' || @Hours || ' hours')";

            var parameters = new Dictionary<string, object>
            {
                { "Asset", asset },
                { "Fiat", fiat },
                { "Hours", hours }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to get lowest price", ex);
        }
    }

    private PriceHistory MapToPriceHistory(SQLiteDataReader reader)
    {
        return new PriceHistory
        {
            Id = reader.GetInt32(0),
            PriceId = reader.GetInt32(1),
            Asset = reader.GetString(2),
            Fiat = reader.GetString(3),
            BuyPrice = (decimal)reader.GetDouble(4),
            SellPrice = (decimal)reader.GetDouble(5),
            RecordedAt = reader.GetDateTime(6),
            CreatedAt = reader.GetDateTime(7),
            SpreadPercentage = reader.IsDBNull(8) ? 0m : (decimal)reader.GetDouble(8),
            PriceChangePercent = reader.IsDBNull(9) ? 0m : (decimal)reader.GetDouble(9),
            Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }
}

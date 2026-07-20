#nullable enable

using BinanceP2pMonitor.Constants;
using BinanceP2pMonitor.Data;
using BinanceP2pMonitor.Exceptions;
using BinanceP2pMonitor.Models;

namespace BinanceP2pMonitor.Repositories;

/// <summary>
/// Repository for managing PriceAlert data
/// </summary>
public class AlertRepository : IAlertRepository
{
    private readonly DatabaseContext _context;

    public AlertRepository(DatabaseContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PriceAlert?> GetByIdAsync(int id)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, AlertType, Threshold, Condition, IsEnabled,
        IsMuted,
                       UserId, CreatedAt, UpdatedAt, LastTriggeredAt, TriggerCount, Notes
        IsMuted,
                FROM PriceAlerts WHERE Id = @Id";

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "Id", id } });
                return reader.Read() ? MapToPriceAlert(reader) : null;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve alert by ID", ex);
        }
    }

    public async Task<IEnumerable<PriceAlert>> GetEnabledAlertsAsync()
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, AlertType, Threshold, Condition, IsEnabled,
        IsMuted,
                       UserId, CreatedAt, UpdatedAt, LastTriggeredAt, TriggerCount, Notes
                FROM PriceAlerts WHERE IsEnabled = 1";

            var alerts = new List<PriceAlert>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql);
                while (reader.Read())
                {
                    alerts.Add(MapToPriceAlert(reader));
                }
                return alerts;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve enabled alerts", ex);
        }
    }

    public async Task<IEnumerable<PriceAlert>> GetUserAlertsAsync(int userId)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, AlertType, Threshold, Condition, IsEnabled,
        IsMuted,
                       UserId, CreatedAt, UpdatedAt, LastTriggeredAt, TriggerCount, Notes
                FROM PriceAlerts WHERE UserId = @UserId ORDER BY CreatedAt DESC";

            var alerts = new List<PriceAlert>();

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, new Dictionary<string, object> { { "UserId", userId } });
                while (reader.Read())
                {
                    alerts.Add(MapToPriceAlert(reader));
                }
                return alerts;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve user alerts", ex);
        }
    }

    public async Task<IEnumerable<PriceAlert>> GetAlertsByAssetAndFiatAsync(string asset, string fiat)
    {
        try
        {
            const string sql = @"
                SELECT Id, Asset, Fiat, AlertType, Threshold, Condition, IsEnabled,
        IsMuted,
                       UserId, CreatedAt, UpdatedAt, LastTriggeredAt, TriggerCount, Notes
                FROM PriceAlerts WHERE Asset = @Asset AND Fiat = @Fiat AND IsEnabled = 1";

            var alerts = new List<PriceAlert>();
            var parameters = new Dictionary<string, object> { { "Asset", asset }, { "Fiat", fiat } };

            return await Task.Run(() =>
            {
                using var reader = _context.ExecuteReader(sql, parameters);
                while (reader.Read())
                {
                    alerts.Add(MapToPriceAlert(reader));
                }
                return alerts;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to retrieve alerts by asset and fiat", ex);
        }
    }

    public async Task<int> AddAsync(PriceAlert alert)
    {
        try
        {
            if (alert is null || !alert.IsValid())
                throw new InvalidAlertException("Alert data is invalid");

            const string sql = @"
                INSERT INTO PriceAlerts
                (Asset, Fiat, AlertType, Threshold, Condition, IsEnabled,
        IsMuted, UserId,
                 CreatedAt, UpdatedAt, LastTriggeredAt, TriggerCount, Notes)
                VALUES (@Asset, @Fiat, @AlertType, @Threshold, @Condition, @IsEnabled,
        IsMuted,
                        @UserId, @CreatedAt, @UpdatedAt, @LastTriggeredAt, @TriggerCount, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new Dictionary<string, object>
            {
                { "Asset", alert.Asset },
                { "Fiat", alert.Fiat },
                { "AlertType", alert.AlertType },
                { "Threshold", alert.Threshold },
                { "Condition", alert.Condition },
                { "IsEnabled", alert.IsEnabled ? 1 : 0 },
                { "UserId", alert.UserId },
                { "CreatedAt", alert.CreatedAt },
                { "UpdatedAt", alert.UpdatedAt },
                { "LastTriggeredAt", alert.LastTriggeredAt ?? (object)DBNull.Value },
                { "TriggerCount", alert.TriggerCount },
                { "Notes", alert.Notes ?? (object)DBNull.Value }
            };

            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, parameters);
                return result is not null ? Convert.ToInt32(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to add alert", ex);
        }
    }

    public async Task<bool> UpdateAsync(PriceAlert alert)
    {
        try
        {
            if (alert is null || alert.Id <= 0)
                throw new InvalidAlertException("Alert data is invalid");

            const string sql = @"
                UPDATE PriceAlerts
                SET Threshold = @Threshold, Condition = @Condition, IsEnabled = @IsEnabled,
        IsMuted,
                    UpdatedAt = @UpdatedAt, LastTriggeredAt = @LastTriggeredAt,
                    TriggerCount = @TriggerCount, Notes = @Notes
                WHERE Id = @Id";

            var parameters = new Dictionary<string, object>
            {
                { "Id", alert.Id },
                { "Threshold", alert.Threshold },
                { "Condition", alert.Condition },
                { "IsEnabled", alert.IsEnabled ? 1 : 0 },
                { "UpdatedAt", alert.UpdatedAt },
                { "LastTriggeredAt", alert.LastTriggeredAt ?? (object)DBNull.Value },
                { "TriggerCount", alert.TriggerCount },
                { "Notes", alert.Notes ?? (object)DBNull.Value }
            };

            return await Task.Run(() => _context.ExecuteCommand(sql, parameters) > 0).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to update alert", ex);
        }
    }

public async Task<bool> SetMutedAsync(int alertId, bool isMuted)
{
    try
    {
        const string sql = @"
        UPDATE PriceAlerts
        SET IsMuted = @IsMuted,
            UpdatedAt = @UpdatedAt
        WHERE Id = @Id";

        var parameters = new Dictionary<string, object>
        {
            { "Id", alertId },
            { "IsMuted", isMuted ? 1 : 0 },
            { "UpdatedAt", DateTime.UtcNow }
        };

        return await Task.Run(() => _context.ExecuteCommand(sql, parameters) > 0).ConfigureAwait(false);
    }
    catch (Exception ex)
    {
        throw new DataAccessException("Failed to update alert mute status", ex);
    }
}


    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            const string sql = "DELETE FROM PriceAlerts WHERE Id = @Id";
            return await Task.Run(() => _context.ExecuteCommand(sql, new Dictionary<string, object> { { "Id", id } }) > 0).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to delete alert", ex);
        }
    }

    public async Task<bool> DeleteUserAlertsAsync(int userId)
    {
        try
        {
            const string sql = "DELETE FROM PriceAlerts WHERE UserId = @UserId";
            return await Task.Run(() => _context.ExecuteCommand(sql, new Dictionary<string, object> { { "UserId", userId } }) > 0).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to delete user alerts", ex);
        }
    }

    public async Task<int> GetUserAlertCountAsync(int userId)
    {
        try
        {
            const string sql = "SELECT COUNT(*) FROM PriceAlerts WHERE UserId = @UserId AND IsEnabled = 1";
            return await Task.Run(() =>
            {
                var result = _context.ExecuteScalar(sql, new Dictionary<string, object> { { "UserId", userId } });
                return result is not null ? Convert.ToInt32(result) : 0;
            });
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to get user alert count", ex);
        }
    }

    private PriceAlert MapToPriceAlert(SqliteDataReader reader)
    {
        return new PriceAlert
        {
            Id = reader.GetInt32(0),
            Asset = reader.GetString(1),
            Fiat = reader.GetString(2),
            AlertType = (AlertType)reader.GetInt32(3),
            Threshold = (decimal)reader.GetDouble(4),
            Condition = (AlertCondition)reader.GetInt32(5),
            IsEnabled = reader.GetBoolean(6),
        IsMuted = reader.GetBoolean(7),
            UserId = reader.GetInt32(8),
            CreatedAt = reader.GetDateTime(9),
            UpdatedAt = reader.GetDateTime(10),
            LastTriggeredAt = reader.IsDBNull(11) ? null : reader.GetInt64(10),
            TriggerCount = reader.GetInt32(12),
            Notes = reader.IsDBNull(13) ? null : reader.GetString(12)
        };
    }
}

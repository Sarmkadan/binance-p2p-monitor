#nullable enable
using System.Data;

using BinanceP2pMonitor.Configuration;
using BinanceP2pMonitor.Exceptions;
using Microsoft.Extensions.Logging;

namespace BinanceP2pMonitor.Data;

/// <summary>
/// Database context for managing SQLite connections and operations
/// </summary>
public class DatabaseContext : IDisposable
{
    private readonly AppSettings? _settings;
    private readonly ILogger<DatabaseContext>? _logger;
    private SqliteConnection? _connection;

    public DatabaseContext(AppSettings settings, ILogger<DatabaseContext> logger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Constructor that accepts an already-open connection (for testing)
    /// </summary>
    public DatabaseContext(SqliteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    /// <summary>
    /// Gets or creates database connection
    /// </summary>
    public SqliteConnection GetConnection()
    {
        try
        {
            if (_connection?.State != ConnectionState.Open)
            {
                _connection?.Dispose();
                _connection = new SqliteConnection(_settings!.GetResolvedConnectionString());
                _connection.Open();
                EnableForeignKeys();
            }

            return _connection;
        }
        catch (Exception ex)
        {
            throw new DataAccessException("Failed to establish database connection", ex);
        }
    }

    /// <summary>
    /// Initializes the database schema
    /// </summary>
    public void Initialize()
    {
        try
        {
            _logger?.LogInformation("Initializing database schema");
            var connection = GetConnection();

            // Create tables
            CreateTables(connection);
            CreateIndexes(connection);

            _logger?.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Database initialization failed");
            throw new DataAccessException("Database initialization failed", ex);
        }
    }

    /// <summary>
    /// Executes a SQL command
    /// </summary>
    public int ExecuteCommand(string commandText, Dictionary<string, object>? parameters = null)
    {
        try
        {
            using var command = GetConnection().CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = _settings?.DatabaseCommandTimeoutSeconds ?? 30;

            if (parameters is not null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            return command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Command execution failed: {commandText}", ex);
        }
    }

    /// <summary>
    /// Executes a query and returns a data reader
    /// </summary>
    public SqliteDataReader ExecuteReader(string commandText, Dictionary<string, object>? parameters = null)
    {
        try
        {
            var command = GetConnection().CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = _settings?.DatabaseCommandTimeoutSeconds ?? 30;

            if (parameters is not null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            return command.ExecuteReader(CommandBehavior.SequentialAccess);
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Query execution failed: {commandText}", ex);
        }
    }

    /// <summary>
    /// Executes a scalar query
    /// </summary>
    public object? ExecuteScalar(string commandText, Dictionary<string, object>? parameters = null)
    {
        try
        {
            using var command = GetConnection().CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = _settings?.DatabaseCommandTimeoutSeconds ?? 30;

            if (parameters is not null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
                }
            }

            return command.ExecuteScalar();
        }
        catch (Exception ex)
        {
            throw new DataAccessException($"Scalar query failed: {commandText}", ex);
        }
    }

    /// <summary>
    /// Enables foreign key constraints
    /// </summary>
    private void EnableForeignKeys()
    {
        ExecuteCommand("PRAGMA foreign_keys = ON");
    }

    /// <summary>
    /// Creates all required tables
    /// </summary>
    private void CreateTables(SqliteConnection connection)
    {
        var tables = new[]
        {
            CreatePricesTable(),
            CreateTradeOffersTable(),
            CreatePriceAlertsTable(),
            CreateUserProfilesTable(),
            CreatePriceHistoryTable(),
            CreateSpreadsTable(),
            CreateMarketsTable(),
            CreateCurrenciesTable()
        };

        foreach (var sql in tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Creates indexes for performance
    /// </summary>
    private void CreateIndexes(SqliteConnection connection)
    {
        var indexes = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_prices_asset_fiat ON Prices(Asset, Fiat)",
            "CREATE INDEX IF NOT EXISTS idx_prices_timestamp ON Prices(Timestamp DESC)",
            "CREATE INDEX IF NOT EXISTS idx_trade_offers_asset_fiat ON TradeOffers(Asset, Fiat)",
            "CREATE INDEX IF NOT EXISTS idx_alerts_user_enabled ON PriceAlerts(UserId, IsEnabled)",
            "CREATE INDEX IF NOT EXISTS idx_history_asset_fiat ON PriceHistory(Asset, Fiat, RecordedAt DESC)",
            "CREATE INDEX IF NOT EXISTS idx_markets_asset_fiat ON Markets(Asset, Fiat)"
        };

        foreach (var sql in indexes)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
    }

    private string CreatePricesTable() => @"
        CREATE TABLE IF NOT EXISTS Prices (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Asset TEXT NOT NULL,
            Fiat TEXT NOT NULL,
            BuyPrice REAL NOT NULL,
            SellPrice REAL NOT NULL,
            BuyChangePercent REAL NOT NULL,
            SellChangePercent REAL NOT NULL,
            Timestamp DATETIME NOT NULL,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NOT NULL,
            Metadata TEXT,
            UNIQUE(Asset, Fiat)
        )";

    private string CreateTradeOffersTable() => @"
        CREATE TABLE IF NOT EXISTS TradeOffers (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            OfferIdFromBinance TEXT NOT NULL UNIQUE,
            Asset TEXT NOT NULL,
            Fiat TEXT NOT NULL,
            TradeType INTEGER NOT NULL,
            Price REAL NOT NULL,
            MinAmount REAL NOT NULL,
            MaxAmount REAL NOT NULL,
            TraderRating REAL NOT NULL,
            CompletedTrades INTEGER NOT NULL,
            PaymentMethods TEXT,
            IsActive BOOLEAN NOT NULL,
            Timestamp DATETIME NOT NULL,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NOT NULL
        )";

    private string CreatePriceAlertsTable() => @"
        CREATE TABLE IF NOT EXISTS PriceAlerts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Asset TEXT NOT NULL,
            Fiat TEXT NOT NULL,
            AlertType INTEGER NOT NULL,
            Threshold REAL NOT NULL,
            Condition INTEGER NOT NULL,
            IsEnabled BOOLEAN NOT NULL,
        IsMuted BOOLEAN NOT NULL DEFAULT 0,
            UserId INTEGER NOT NULL,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NOT NULL,
            LastTriggeredAt INTEGER,
            TriggerCount INTEGER NOT NULL DEFAULT 0,
    HysteresisThreshold REAL NOT NULL DEFAULT 0,
    LastTriggerDirection INTEGER NOT NULL DEFAULT 0,
            Notes TEXT,
            FOREIGN KEY(UserId) REFERENCES UserProfiles(Id)
        )";

    private string CreateUserProfilesTable() => @"
        CREATE TABLE IF NOT EXISTS UserProfiles (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            TelegramChatId INTEGER NOT NULL UNIQUE,
            TelegramUsername TEXT NOT NULL,
            Email TEXT NOT NULL UNIQUE,
            FirstName TEXT,
            LastName TEXT,
            IsActive BOOLEAN NOT NULL,
            ReceiveNotifications BOOLEAN NOT NULL,
            ReceiveDailyReport BOOLEAN NOT NULL,
            DailyReportHourUtc INTEGER NOT NULL DEFAULT 9,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NOT NULL,
            LastActivityAt INTEGER,
            Preferences TEXT
        )";

    private string CreatePriceHistoryTable() => @"
        CREATE TABLE IF NOT EXISTS PriceHistory (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            PriceId INTEGER NOT NULL,
            Asset TEXT NOT NULL,
            Fiat TEXT NOT NULL,
            BuyPrice REAL NOT NULL,
            SellPrice REAL NOT NULL,
            RecordedAt DATETIME NOT NULL,
            CreatedAt DATETIME NOT NULL,
            SpreadPercentage REAL,
            PriceChangePercent REAL,
            Notes TEXT,
            FOREIGN KEY(PriceId) REFERENCES Prices(Id)
        )";

    private string CreateSpreadsTable() => @"
        CREATE TABLE IF NOT EXISTS Spreads (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Asset TEXT NOT NULL,
            Fiat TEXT NOT NULL,
            CurrentSpreadPercent REAL NOT NULL,
            AverageSpreadPercent REAL NOT NULL,
            MinSpreadPercent REAL NOT NULL,
            MaxSpreadPercent REAL NOT NULL,
            SampleCount INTEGER NOT NULL,
            LastUpdatedAt DATETIME NOT NULL,
            CreatedAt DATETIME NOT NULL,
            StandardDeviation REAL,
            PercentileRank REAL,
            UNIQUE(Asset, Fiat)
        )";

    private string CreateMarketsTable() => @"
        CREATE TABLE IF NOT EXISTS Markets (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Asset TEXT NOT NULL,
            Fiat TEXT NOT NULL,
            IsActive BOOLEAN NOT NULL,
            IsMonitored BOOLEAN NOT NULL,
            Description TEXT,
            LastBuyPrice REAL NOT NULL,
            LastSellPrice REAL NOT NULL,
            TotalOffers INTEGER NOT NULL,
            DailyVolume INTEGER NOT NULL,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NOT NULL,
            LastPriceUpdateAt INTEGER,
            MonitoringPriority INTEGER NOT NULL DEFAULT 50,
            UNIQUE(Asset, Fiat)
        )";

    private string CreateCurrenciesTable() => @"
        CREATE TABLE IF NOT EXISTS Currencies (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Code TEXT NOT NULL UNIQUE,
            Name TEXT NOT NULL,
            Symbol TEXT,
            IsActive BOOLEAN NOT NULL,
            DecimalPlaces INTEGER NOT NULL,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NOT NULL,
            PopularityScore REAL,
            DisplayOrder INTEGER DEFAULT 50,
            Notes TEXT
        )";

    public void Dispose()
    {
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}

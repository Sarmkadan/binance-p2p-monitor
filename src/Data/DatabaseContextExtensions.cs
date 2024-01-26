#nullable enable

using System.Data;
using Microsoft.Data.Sqlite;

namespace BinanceP2pMonitor.Data;

/// <summary>
/// Extension methods for DatabaseContext providing common database operations
/// </summary>
public static class DatabaseContextExtensions
{
    /// <summary>
    /// Executes a SQL command with parameters and returns the number of affected rows
    /// </summary>
    public static int ExecuteCommand(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.ExecuteCommand(commandText, paramDict);
    }

    /// <summary>
    /// Executes a parameterized SQL query and returns a sequence of objects
    /// </summary>
    public static IEnumerable<Dictionary<string, object>> ExecuteQuery(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        using var reader = context.ExecuteReader(commandText, parameters.ToDictionary(p => p.Name, p => p.Value));

        var results = new List<Dictionary<string, object>>();
        var fieldCount = reader.FieldCount;
        var fieldNames = new string[fieldCount];

        for (int i = 0; i < fieldCount; i++)
        {
            fieldNames[i] = reader.GetName(i);
        }

        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < fieldCount; i++)
            {
                row[fieldNames[i]] = reader.GetValue(i);
            }
            results.Add(row);
        }

        return results;
    }

    /// <summary>
    /// Executes a scalar query and returns the result as the specified type
    /// </summary>
    public static T? ExecuteScalar<T>(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        var result = context.ExecuteScalar(commandText, parameters);
        return result == DBNull.Value || result == null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Executes a scalar query with parameters and returns the result as the specified type
    /// </summary>
    public static T? ExecuteScalar<T>(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.ExecuteScalar<T>(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns a single value of the specified type
    /// </summary>
    public static T? QuerySingle<T>(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        using var reader = context.ExecuteReader(commandText, parameters);
        if (reader.Read())
        {
            return reader.IsDBNull(0) ? default : (T)Convert.ChangeType(reader.GetValue(0), typeof(T));
        }
        return default;
    }

    /// <summary>
    /// Executes a query and returns a single value of the specified type with parameters
    /// </summary>
    public static T? QuerySingle<T>(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QuerySingle<T>(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a string
    /// </summary>
    public static string? QueryString(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        return context.QuerySingle<string>(commandText, parameters);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a string with parameters
    /// </summary>
    public static string? QueryString(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryString(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as an int
    /// </summary>
    public static int QueryInt(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        return context.QuerySingle<int?>(commandText, parameters) ?? 0;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as an int with parameters
    /// </summary>
    public static int QueryInt(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryInt(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a long
    /// </summary>
    public static long QueryLong(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        return context.QuerySingle<long?>(commandText, parameters) ?? 0L;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a long with parameters
    /// </summary>
    public static long QueryLong(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryLong(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a decimal
    /// </summary>
    public static decimal QueryDecimal(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        return context.QuerySingle<decimal?>(commandText, parameters) ?? 0m;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a decimal with parameters
    /// </summary>
    public static decimal QueryDecimal(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryDecimal(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a bool
    /// </summary>
    public static bool QueryBool(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        var result = context.QuerySingle<bool?>(commandText, parameters);
        return result ?? false;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a bool with parameters
    /// </summary>
    public static bool QueryBool(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryBool(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a DateTime
    /// </summary>
    public static DateTime? QueryDateTime(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        return context.QuerySingle<DateTime?>(commandText, parameters);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a DateTime with parameters
    /// </summary>
    public static DateTime? QueryDateTime(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryDateTime(commandText, paramDict);
    }

    /// <summary>
    /// Executes multiple commands in a single transaction
    /// </summary>
    public static void ExecuteInTransaction(this DatabaseContext context, Action<DatabaseContext> transactionAction)
    {
        using var connection = context.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Temporarily replace the connection with the transaction-bound one
            var originalConnection = context.GetConnection();
            var transactionContext = new DatabaseContext(new SqliteConnection(originalConnection.ConnectionString));

            transactionAction(transactionContext);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Executes multiple commands in a single transaction and returns a result
    /// </summary>
    public static T ExecuteInTransaction<T>(this DatabaseContext context, Func<DatabaseContext, T> transactionAction)
    {
        using var connection = context.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            var originalConnection = context.GetConnection();
            var transactionContext = new DatabaseContext(new SqliteConnection(originalConnection.ConnectionString));

            var result = transactionAction(transactionContext);
            transaction.Commit();
            return result;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Checks if a table exists in the database
    /// </summary>
    public static bool TableExists(this DatabaseContext context, string tableName)
    {
        var sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        return context.QueryInt(sql) > 0;
    }

    /// <summary>
    /// Gets the count of rows in a table
    /// </summary>
    public static int GetTableCount(this DatabaseContext context, string tableName)
    {
        var sql = $"SELECT COUNT(*) FROM {tableName}";
        return context.QueryInt(sql);
    }

    /// <summary>
    /// Gets the last inserted row ID
    /// </summary>
    public static long GetLastInsertRowId(this DatabaseContext context)
    {
        return context.QueryLong("SELECT last_insert_rowid()");
    }
}
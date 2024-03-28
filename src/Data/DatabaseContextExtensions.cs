#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the command</param>
    /// <returns>The number of affected rows</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static int ExecuteCommand(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.ExecuteCommand(commandText, paramDict);
    }

    /// <summary>
    /// Executes a parameterized SQL query and returns a sequence of objects
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>A sequence of dictionaries representing the result rows</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static IEnumerable<Dictionary<string, object>> ExecuteQuery(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

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
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The scalar result or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static T? ExecuteScalar<T>(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var result = context.ExecuteScalar(commandText, parameters);
        return result == DBNull.Value || result == null ? default : (T)Convert.ChangeType(result, typeof(T));
    }

    /// <summary>
    /// Executes a scalar query with parameters and returns the result as the specified type
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The scalar result or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static T? ExecuteScalar<T>(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.ExecuteScalar<T>(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns a single value of the specified type
    /// </summary>
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The single value or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static T? QuerySingle<T>(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

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
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The single value or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static T? QuerySingle<T>(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QuerySingle<T>(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a string
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The string value or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static string? QueryString(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        return context.QuerySingle<string>(commandText, parameters);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a string with parameters
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The string value or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static string? QueryString(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryString(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as an int
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The int value, 0 if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static int QueryInt(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        return context.QuerySingle<int?>(commandText, parameters) ?? 0;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as an int with parameters
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The int value, 0 if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static int QueryInt(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryInt(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a long
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The long value, 0L if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static long QueryLong(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        return context.QuerySingle<long?>(commandText, parameters) ?? 0L;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a long with parameters
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The long value, 0L if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static long QueryLong(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryLong(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a decimal
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The decimal value, 0m if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static decimal QueryDecimal(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        return context.QuerySingle<decimal?>(commandText, parameters) ?? 0m;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a decimal with parameters
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The decimal value, 0m if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static decimal QueryDecimal(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryDecimal(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a bool
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The bool value, false if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static bool QueryBool(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var result = context.QuerySingle<bool?>(commandText, parameters);
        return result ?? false;
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a bool with parameters
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The bool value, false if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static bool QueryBool(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryBool(commandText, paramDict);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a DateTime
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The DateTime value or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static DateTime? QueryDateTime(this DatabaseContext context, string commandText, Dictionary<string, object>? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        return context.QuerySingle<DateTime?>(commandText, parameters);
    }

    /// <summary>
    /// Executes a query and returns the first column of the first row as a DateTime with parameters
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="commandText">The SQL command text</param>
    /// <param name="parameters">The parameters for the query</param>
    /// <returns>The DateTime value or null if no result</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="commandText"/> is null</exception>
    public static DateTime? QueryDateTime(this DatabaseContext context, string commandText, params (string Name, object Value)[] parameters)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(commandText);

        var paramDict = parameters.ToDictionary(p => p.Name, p => p.Value);
        return context.QueryDateTime(commandText, paramDict);
    }

    /// <summary>
    /// Executes multiple commands in a single transaction
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="transactionAction">The action to execute within the transaction</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="transactionAction"/> is null</exception>
    public static void ExecuteInTransaction(this DatabaseContext context, Action<DatabaseContext> transactionAction)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(transactionAction);

        using var connection = context.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Execute the action with the transaction-bound connection
            transactionAction(context);

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
    /// <typeparam name="T">The return type</typeparam>
    /// <param name="context">The database context</param>
    /// <param name="transactionAction">The function to execute within the transaction</param>
    /// <returns>The result of the transaction function</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="transactionAction"/> is null</exception>
    public static T ExecuteInTransaction<T>(this DatabaseContext context, Func<DatabaseContext, T> transactionAction)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(transactionAction);

        using var connection = context.GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Execute the function with the transaction-bound connection
            var result = transactionAction(context);
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
    /// <param name="context">The database context</param>
    /// <param name="tableName">The name of the table to check</param>
    /// <returns>True if the table exists, false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="tableName"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty or whitespace</exception>
    public static bool TableExists(this DatabaseContext context, string tableName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var sql = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        return context.QueryInt(sql) > 0;
    }

    /// <summary>
    /// Gets the count of rows in a table
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tableName">The name of the table</param>
    /// <returns>The number of rows in the table</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> or <paramref name="tableName"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="tableName"/> is empty or whitespace</exception>
    public static int GetTableCount(this DatabaseContext context, string tableName)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var sql = $"SELECT COUNT(*) FROM {tableName}";
        return context.QueryInt(sql);
    }

    /// <summary>
    /// Gets the last inserted row ID
    /// </summary>
    /// <param name="context">The database context</param>
    /// <returns>The last inserted row ID</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> is null</exception>
    public static long GetLastInsertRowId(this DatabaseContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.QueryLong("SELECT last_insert_rowid()");
    }
}
# DatabaseContext

The `DatabaseContext` class serves as the primary data access layer for the `binance-p2p-monitor` application, encapsulating the lifecycle and execution logic for a SQLite database connection. It provides a streamlined interface for initializing the database, managing the underlying `SqliteConnection`, and executing various types of SQL commands, including non-query operations, data retrieval via readers, and scalar value extraction, while ensuring proper resource disposal.

## API

### Constructors

**`public DatabaseContext()`**
Initializes a new instance of the `DatabaseContext` class with default configuration settings. This constructor typically prepares the object for subsequent initialization but does not immediately open a database connection.

**`public DatabaseContext(string connectionString)`**
Initializes a new instance of the `DatabaseContext` class using the specified connection string. This overload allows the immediate configuration of the target SQLite database file or in-memory store upon instantiation.

### Methods

**`public SqliteConnection GetConnection()`**
Retrieves the underlying `SqliteConnection` instance managed by this context.
*   **Return Value:** The active `SqliteConnection` object.
*   **Remarks:** Consumers should use this method only when direct access to the connection object is required for operations not covered by the context's helper methods. The connection state depends on whether `Initialize` has been called.

**`public void Initialize()`**
Establishes the database connection and performs any necessary schema validation or creation steps required by the monitor.
*   **Exceptions:** Throws a database-specific exception if the connection fails, the file is locked, or the schema cannot be created.

**`public int ExecuteCommand(string commandText, params object[] parameters)`**
Executes a SQL command that does not return rows, such as `INSERT`, `UPDATE`, `DELETE`, or DDL statements.
*   **Parameters:**
    *   `commandText`: The SQL statement to execute.
    *   `parameters`: An optional array of objects to bind to parameters in the SQL statement.
*   **Return Value:** The number of rows affected by the command.
*   **Exceptions:** Throws if the command syntax is invalid, the connection is not open, or a constraint violation occurs.

**`public SqliteDataReader ExecuteReader(string commandText, params object[] parameters)`**
Executes a SQL query and returns a `SqliteDataReader` to iterate over the resulting rows.
*   **Parameters:**
    *   `commandText`: The SELECT statement to execute.
    *   `parameters`: An optional array of objects to bind to parameters in the SQL statement.
*   **Return Value:** A `SqliteDataReader` instance containing the result set.
*   **Exceptions:** Throws if the query is invalid or the connection is not open. The caller is responsible for disposing of the reader.

**`public object? ExecuteScalar(string commandText, params object[] parameters)`**
Executes a query and returns the first column of the first row in the result set, ignoring any additional rows or columns.
*   **Parameters:**
    *   `commandText`: The SQL statement to execute.
    *   `parameters`: An optional array of objects to bind to parameters in the SQL statement.
*   **Return Value:** The value of the first cell in the result, or `null` if the result set is empty.
*   **Exceptions:** Throws if the execution fails due to syntax errors or connection issues.

**`public void Dispose()`**
Releases all unmanaged resources used by the `DatabaseContext`, specifically closing and disposing of the underlying `SqliteConnection`.
*   **Remarks:** This method should be called when the context is no longer needed, preferably via a `using` statement. Subsequent calls to other methods after disposal may result in exceptions.

## Usage

### Example 1: Initialization and Scalar Query
This example demonstrates initializing the context and retrieving a single aggregate value, such as a count of monitored pairs.

```csharp
using var context = new DatabaseContext("Data Source=monitor.db");
context.Initialize();

// Retrieve the total count of active P2P advertisements
var countObj = context.ExecuteScalar(
    "SELECT COUNT(*) FROM Advertisements WHERE Status = @status", 
    "active"
);

int activeCount = countObj != null ? Convert.ToInt32(countObj) : 0;
Console.WriteLine($"Active advertisements: {activeCount}");
// Dispose is called automatically by the 'using' statement
```

### Example 2: Command Execution and Data Reading
This example shows how to insert a new record and subsequently read a range of data using a data reader.

```csharp
using var context = new DatabaseContext("Data Source=monitor.db");
context.Initialize();

// Insert a new trade record
int rowsAffected = context.ExecuteCommand(
    "INSERT INTO Trades (Pair, Price, Amount) VALUES (@pair, @price, @amount)",
    "USDT_BUSD", 1.002, 500.00
);

if (rowsAffected > 0)
{
    // Read recent trades
    using var reader = context.ExecuteReader(
        "SELECT Pair, Price FROM Trades ORDER BY Id DESC LIMIT 10"
    );

    while (reader.Read())
    {
        string pair = reader.GetString(0);
        double price = reader.GetDouble(1);
        Console.WriteLine($"{pair}: {price}");
    }
}
```

## Notes

*   **Thread Safety:** The underlying `SqliteConnection` and its associated commands are generally not thread-safe for concurrent write operations. While multiple readers may be supported depending on the SQLite journal mode, simultaneous writes from multiple threads using the same `DatabaseContext` instance can lead to locking exceptions (`SQLiteBusyException`). It is recommended to scope `DatabaseContext` instances per thread or serialize access externally.
*   **Resource Management:** The `ExecuteReader` method returns a `SqliteDataReader` that holds an open lock on the connection. The reader must be explicitly disposed (or wrapped in a `using` block) before executing another command on the same context instance; otherwise, a "Busy" error will occur.
*   **Connection State:** Calling `GetConnection` before `Initialize` may return a connection object that is not yet open. Ensure `Initialize` is invoked prior to any execution methods (`ExecuteCommand`, `ExecuteReader`, `ExecuteScalar`) to guarantee a valid connection state.
*   **Parameter Binding:** The `params object[] parameters` argument in execution methods relies on positional or named binding depending on the underlying SQLite provider implementation. Ensure the order of arguments matches the parameter placeholders in the `commandText` if positional binding is used.

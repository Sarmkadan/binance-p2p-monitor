# DatabaseContextExtensions

Extension methods for `DatabaseContext` that provide simplified SQL command execution, query operations, and transaction handling. These methods abstract common ADO.NET patterns into concise, strongly-typed APIs for database operations.

## API

### `ExecuteCommand`
Executes a non-query SQL command (e.g., `INSERT`, `UPDATE`, `DELETE`) against the database.

- **Parameters**:
  - `commandText`: SQL command text.
  - `parameters`: Optional parameters for the command.
- **Return value**: Number of rows affected.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `SqlException` on database errors.

### `ExecuteQuery`
Executes a SQL query and returns a collection of rows as dictionaries.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: `IEnumerable<Dictionary<string, object>>` where each dictionary represents a row with column names as keys.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `SqlException` on database errors.

### `ExecuteScalar<T>`
Executes a SQL command and returns the first column of the first row in the result set.

- **Parameters**:
  - `commandText`: SQL command text.
  - `parameters`: Optional parameters for the command.
- **Return value**: First column value cast to type `T`, or `default(T)` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidCastException` if the value cannot be cast to `T`.
- **Throws**: `SqlException` on database errors.

### `QuerySingle<T>`
Executes a SQL query and returns the first column of the first row in the result set, throwing if no rows exist.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value cast to type `T`.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidOperationException` if no rows are returned.
- **Throws**: `InvalidCastException` if the value cannot be cast to `T`.
- **Throws**: `SqlException` on database errors.

### `QueryString`
Executes a SQL query and returns the first column of the first row as a string.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value as a string, or `null` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `SqlException` on database errors.

### `QueryInt`
Executes a SQL query and returns the first column of the first row as an integer.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value as an integer, or `0` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidCastException` if the value cannot be cast to an integer.
- **Throws**: `SqlException` on database errors.

### `QueryLong`
Executes a SQL query and returns the first column of the first row as a long integer.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value as a long integer, or `0` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidCastException` if the value cannot be cast to a long integer.
- **Throws**: `SqlException` on database errors.

### `QueryDecimal`
Executes a SQL query and returns the first column of the first row as a decimal.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value as a decimal, or `0` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidCastException` if the value cannot be cast to a decimal.
- **Throws**: `SqlException` on database errors.

### `QueryBool`
Executes a SQL query and returns the first column of the first row as a boolean.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value as a boolean, or `false` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidCastException` if the value cannot be cast to a boolean.
- **Throws**: `SqlException` on database errors.

### `QueryDateTime`
Executes a SQL query and returns the first column of the first row as a nullable `DateTime`.

- **Parameters**:
  - `commandText`: SQL query text.
  - `parameters`: Optional parameters for the query.
- **Return value**: First column value as a `DateTime?`, or `null` if no rows exist.
- **Throws**: `ArgumentNullException` if `commandText` is `null`.
- **Throws**: `InvalidCastException` if the value cannot be cast to a `DateTime`.
- **Throws**: `SqlException` on database errors.

### `ExecuteInTransaction`
Executes an action within a database transaction, committing on success or rolling back on failure.

- **Parameters**:
  - `action`: Action to execute within the transaction.
- **Throws**: `ArgumentNullException` if `action` is `null`.
- **Throws**: `SqlException` on database errors.

### `ExecuteInTransaction<T>`
Executes a function within a database transaction, committing on success or rolling back on failure.

- **Parameters**:
  - `func`: Function to execute within the transaction.
- **Return value**: Result of `func`.
- **Throws**: `ArgumentNullException` if `func` is `null`.
- **Throws**: `SqlException` on database errors.

## Usage

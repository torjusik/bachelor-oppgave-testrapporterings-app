using Npgsql;
using System.Data;


namespace Bachelor_Testing_V1
{
    /// <summary>
    /// A handler class for PostgreSQL database operations in Windows Forms applications
    /// </summary>
    public class PostgresDbHandler
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the PostgresDbHandler class with connection parameters
        /// </summary>
        /// <param name="serverIp">The PostgreSQL server address</param>
        /// <param name="port">The PostgreSQL server port</param>
        /// <param name="database">The database name</param>
        /// <param name="username">The username for authentication</param>
        /// <param name="password">The password for authentication</param>
        public PostgresDbHandler(string serverIp, int port, string database, string username, string password)
        {
            _connectionString = $"Server={serverIp};Port={port};Database={database};User Id={username};Password={password};";
        }

        /// <summary>
        /// Initializes a new instance of the PostgresDbHandler class with a connection string
        /// </summary>
        /// <param name="connectionString">The complete connection string</param>
        public PostgresDbHandler(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if the connection succeeds, false otherwise</returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Executes a non-query SQL command (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="commandText">The SQL command to execute</param>
        /// <param name="parameters">Optional parameters for the SQL command</param>
        /// <returns>The number of rows affected</returns>
        public int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Query execution failed: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        /// <summary>
        /// Executes a SQL query and returns a DataTable
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the SQL query</param>
        /// <returns>A DataTable containing the results</returns>
        public DataTable ExecuteQuery(string query, Dictionary<string, object> parameters = null)
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Query execution failed: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new DataTable();
            }
        }

        /// <summary>
        /// Executes a SQL query asynchronously and returns a DataTable
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the SQL query</param>
        /// <returns>A DataTable containing the results</returns>
        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            dataTable.Load(reader);
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Async query execution failed: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new DataTable();
            }
        }

        /// <summary>
        /// Executes a SQL query and returns a single value
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the SQL query</param>
        /// <returns>The first column of the first row in the result set</returns>
        public object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Query execution failed: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Executes a SQL query within a transaction
        /// </summary>
        /// <param name="queries">List of SQL queries to execute</param>
        /// <param name="parametersList">List of parameter dictionaries for each query</param>
        /// <returns>True if transaction succeeds, false otherwise</returns>
        public bool ExecuteTransaction(List<string> queries, List<Dictionary<string, object>> parametersList = null)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            for (int i = 0; i < queries.Count; i++)
                            {
                                using (var command = new NpgsqlCommand(queries[i], connection, transaction))
                                {
                                    if (parametersList != null && i < parametersList.Count && parametersList[i] != null)
                                    {
                                        foreach (var param in parametersList[i])
                                        {
                                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                                        }
                                    }
                                    command.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Transaction failed and was rolled back: {ex.Message}",
                                "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Transaction setup failed: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Binds a DataTable to a DataGridView control
        /// </summary>
        /// <param name="gridView">The DataGridView to bind data to</param>
        /// <param name="dataTable">The DataTable containing the data</param>
        public void BindDataToGridView(DataGridView gridView, DataTable dataTable)
        {
            try
            {
                gridView.DataSource = null;
                gridView.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to bind data: {ex.Message}", "Data Binding Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads data from a query into a ComboBox control
        /// </summary>
        /// <param name="comboBox">The ComboBox to populate</param>
        /// <param name="query">The SQL query to retrieve data</param>
        /// <param name="displayMember">The column name to display</param>
        /// <param name="valueMember">The column name to use as the value</param>
        /// <param name="parameters">Optional parameters for the query</param>
        public void LoadComboBox(ComboBox comboBox, string query, string displayMember,
            string valueMember, Dictionary<string, object> parameters = null)
        {
            try
            {
                DataTable dataTable = ExecuteQuery(query, parameters);
                comboBox.DataSource = dataTable;
                comboBox.DisplayMember = displayMember;
                comboBox.ValueMember = valueMember;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load combo box: {ex.Message}", "Data Binding Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Checks if a record exists in the database
        /// </summary>
        /// <param name="query">The SQL query to check existence</param>
        /// <param name="parameters">Optional parameters for the query</param>
        /// <returns>True if record exists, false otherwise</returns>
        public bool RecordExists(string query, Dictionary<string, object> parameters = null)
        {
            object result = ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value;
        }
        public string? GetTestProcedureJsonById(int procedureId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = new NpgsqlCommand(
                        "SELECT procedure_json FROM test_procedure WHERE id = @ProcedureId",
                        connection))
                    {
                        // Add parameter to prevent SQL injectionwwww
                        command.Parameters.AddWithValue("@ProcedureId", procedureId);

                        // Execute scalar to get the JSON as a string
                        var result = command.ExecuteScalar();

                        // Return the JSON or null if not found
                        return result?.ToString();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                // Log the exception (in a real application, use proper logging)
                MessageBox.Show($"Database error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, use proper logging)
                MessageBox.Show($"Unexpected error: {ex.Message}");
                throw;
            }
        }
    }
}

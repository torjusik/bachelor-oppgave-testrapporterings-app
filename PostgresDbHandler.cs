using Npgsql;
using NpgsqlTypes;
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
        public int ExecuteNonQuery(string commandText, Dictionary<string, object>? parameters = null)
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
        public DataTable ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
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
        /// Executes a SQL query and returns a single value
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="parameters">Optional parameters for the SQL query</param>
        /// <returns>The first column of the first row in the result set</returns>
        public object? ExecuteScalar(string query, Dictionary<string, object>? parameters = null)
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
        public bool ExecuteTransaction(List<string> queries, List<Dictionary<string, object>>? parametersList = null)
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
                        command.Parameters.AddWithValue("@ProcedureId", procedureId);
                        var result = command.ExecuteScalar();
                        return result?.ToString();
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}");
                throw;
            }
        }
        public void PopulateSwitchboardComboBox(ComboBox comboBox)
        {

            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM get_switchboards_for_combobox()", connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        // Clear existing items
                        comboBox.Items.Clear();

                        // Create a data table to bind to the combobox
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        // Set up the combobox
                        comboBox.DisplayMember = "display_name";
                        comboBox.ValueMember = "id";
                        comboBox.DataSource = dt;
                    }
                }
            }
        }
        public int? GetLatestTestIdForSwitchboard(int switchboardId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (NpgsqlCommand command = new NpgsqlCommand("SELECT get_latest_switchboard_test_id(@switchboardId)", connection))
                {
                    command.Parameters.AddWithValue("switchboardId", NpgsqlDbType.Integer, switchboardId);

                    // Execute the function and get the result
                    object? result = command.ExecuteScalar();

                    // Return null if no test found, otherwise return the ID
                    return result == DBNull.Value ? null : (int?)result;
                }
            }
        }
        public int SaveTestResult(
            int switchboardId,
            int testProcedureId,
            int testerId,
            int stepId,
            string requirement,
            bool passed,
            string? notes = null,
            int? execution_id = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT save_test_result(@switchboard_id, @test_procedure_id, @tester_id, @step_id, @requirement, @passed, @notes, @execution_id);", conn);
            cmd.Parameters.AddWithValue("switchboard_id", switchboardId);
            cmd.Parameters.AddWithValue("test_procedure_id", testProcedureId);
            cmd.Parameters.AddWithValue("tester_id", testerId);
            cmd.Parameters.AddWithValue("step_id", stepId);
            cmd.Parameters.AddWithValue("requirement", requirement);
            cmd.Parameters.AddWithValue("passed", passed);
            cmd.Parameters.AddWithValue("notes", (object?)notes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("execution_id", (object?)execution_id ?? DBNull.Value);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
        }
    }
}

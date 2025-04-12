using Npgsql;
using System;
using System.Data;
using ApartmentManagement.Utility;

namespace ApartmentManagement.Data
{
    public class DbConnection
    {
        private readonly string _connectionString;

        // Constructor that uses the connection string from ConfigManager
        public DbConnection()
        {
            // Get the connection string from ConfigManager
            _connectionString = ConfigManager.GetConnectionString("DefaultConnection");
        }

        // Open a connection to the PostgreSQL database
        public IDbConnection OpenConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(_connectionString);  // Create the connection
                connection.Open();  // Open the connection to the database
                Console.WriteLine("Database connection established successfully.");
                return connection;  // Return the open connection
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while connecting to the database: {ex.Message}");
                return null;  // Return null in case of error
            }
        }

        // Close the database connection
        public void CloseConnection(IDbConnection connection)
        {
            try
            {
                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();  // Close the connection
                    Console.WriteLine("Database connection closed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing the connection: {ex.Message}");
            }
        }
        public string ConnectionString => _connectionString;
    }
}
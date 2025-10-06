using Dapper;
using Microsoft.Data.SqlClient;

namespace Flow.Infra.Migrations
{
    public class Database
    {
        public static void CreateDatabase(string connectionString)
        {
            string databaseName = "FlowDB";

            using var myConnection = new SqlConnection(connectionString);

            var query = @"
                SELECT name 
                FROM sys.databases 
                WHERE name = @name";

            var parameters = new { name = databaseName };

            myConnection.Open();

            var registers = myConnection.Query<string>(query, parameters);

            if (!registers.Any())
            {
                myConnection.Execute($"CREATE DATABASE {databaseName}");
                Console.WriteLine($"Database '{databaseName}' created.");
            }
            else
            {
                Console.WriteLine($"Database '{databaseName}' exists.");
            }
        }
    }
}

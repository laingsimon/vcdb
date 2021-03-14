using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests.Database
{
    internal class Sql : ISql
    {
        private readonly string connectionString;
        private readonly IDatabaseProduct databaseProduct;

        public Sql(IntegrationTestOptions options, IDatabaseProduct databaseProduct)
        {
            this.connectionString = options.ConnectionString;
            this.databaseProduct = databaseProduct;
        }

        public async Task ExecuteBatchedSql(TextReader sqlContent, string database = null)
        {
            using (var connection = databaseProduct.CreateConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!string.IsNullOrEmpty(database))
                    await ExecuteSql($"USE {databaseProduct.EscapeIdentifier(database)}", connection);

                await foreach (var batch in databaseProduct.SplitStatementIntoBatches(sqlContent))
                {
                    await ExecuteSql(batch, connection);
                }
            }
        }

        public async Task ExecuteSql(string sql, DbConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public async Task ExecuteSql(string sql, string database = null)
        {
            using (var connection = databaseProduct.CreateConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!string.IsNullOrEmpty(database))
                    await ExecuteSql($"USE {databaseProduct.EscapeIdentifier(database)}", connection);

                await ExecuteSql(sql, connection);
            }
        }

        public async Task WaitForReady(int attempts)
        {
            Debug.WriteLine("Waiting for database server to be available...");
            var count = 0;
            while (count++ <= attempts)
            {
                var success = await TestConnection(count == attempts);
                if (success)
                {
                    Debug.WriteLine("Database server is available");
                    return;
                }
                await Task.Delay(TimeSpan.FromSeconds(count));
            }

            throw new TimeoutException($"Timeout waiting for database to be ready");
        }

        private async Task<bool> TestConnection(bool reportError)
        {
            try
            {
                using (var connection = databaseProduct.CreateConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = databaseProduct.TestConnectionStatement;
                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception exc)
            {
                if (reportError)
                    throw exc;

                return false;
            }
        }
    }
}

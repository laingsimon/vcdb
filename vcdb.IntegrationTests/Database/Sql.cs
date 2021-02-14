using System;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Database
{
    internal class Sql : ISql
    {
        private readonly string connectionString;
        private readonly ILogger log;
        private readonly ProductName productName;

        public Sql(IntegrationTestOptions options, ILogger log, ProductName productName)
        {
            connectionString = options.ConnectionString;
            this.log = log;
            this.productName = productName;
        }

        public async Task ExecuteBatchedSql(TextReader sqlContent, string database = null)
        {
            using (var connection = productName.CreateConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!string.IsNullOrEmpty(database))
                    await ExecuteSql($"USE [{database}]", connection);

                var sqlBatch = new StringBuilder();
                string line;
                while ((line = await sqlContent.ReadLineAsync()) != null)
                {
                    if (line.Trim().Equals("go", StringComparison.OrdinalIgnoreCase))
                    {
                        await ExecuteSql(sqlBatch.ToString(), connection);
                        sqlBatch.Clear();
                        continue;
                    }

                    sqlBatch.AppendLine(line);
                }

                if (sqlBatch.Length > 0)
                {
                    await ExecuteSql(sqlBatch.ToString(), connection);
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
            using (var connection = productName.CreateConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!string.IsNullOrEmpty(database))
                    await ExecuteSql($"USE [{database}]", connection);

                await ExecuteSql(sql, connection);
            }
        }

        public async Task WaitForReady(int attempts)
        {
            log.LogDebug("Waiting for SQL server to be available...");
            var count = 0;
            while (count++ <= attempts)
            {
                var success = await TestConnection(count == attempts);
                if (success)
                {
                    log.LogDebug("SQL server is available");
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
                using (var connection = productName.CreateConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = "select count(*) from information_schema.tables";
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

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    public class SqlServerProduct : IDatabaseProduct
    {
        public string Name => "SqlServer";
        public string FallbackConnectionString => "server=localhost;user id=sa;password=vcdb_2020";
        public string TestConnectionStatement => "select count(*) from sys.databases";
        
        public DbConnection CreateConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public async Task DropDatabase(string name, ISql sql)
        {
            await sql.ExecuteBatchedSql(new StringReader($@"
DROP DATABASE IF EXISTS {EscapeIdentifier(name)}"), "master");
        }

        public string EscapeIdentifier(string name)
        {
            return $"[{name}]";
        }

        public string GetInstalledServerVersion(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "select @@version";
                return (string)command.ExecuteScalar();
            }
        }

        public string InitialiseDatabase(string name)
        {
            return $@"
DROP DATABASE IF EXISTS {EscapeIdentifier(name)}
GO

CREATE DATABASE {EscapeIdentifier(name)}";
        }

        public bool IsScenarioVersionCompatibleWithDatabaseVersion(string serverVersion, string scenarioMinimumVersion)
        {
            var result = StringComparer.OrdinalIgnoreCase.Compare(serverVersion, scenarioMinimumVersion);
            return result >= 0;
        }

        public async IAsyncEnumerable<string> SplitStatementIntoBatches(TextReader statement)
        {
            var sqlBatch = new StringBuilder();
            string line;
            while ((line = await statement.ReadLineAsync()) != null)
            {
                if (line.Trim().Equals("go", StringComparison.OrdinalIgnoreCase))
                {
                    yield return sqlBatch.ToString();
                    sqlBatch.Clear();
                    continue;
                }

                sqlBatch.AppendLine(line);
            }

            if (sqlBatch.Length > 0)
            {
                yield return sqlBatch.ToString();
            }
        }

        public bool CanConnect(string connectionString)
        {
            try
            {
                GetInstalledServerVersion(connectionString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

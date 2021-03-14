﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    public class MySqlProduct : IDatabaseProduct
    {
        public string Name => "MySql";
        public string FallbackConnectionString => "server=localhost;uid=root;pwd=vcdb_2020";
        public string TestConnectionStatement => "select count(*) from information_schema.tables";
        public string DatabaseVersion { get; set; }

        public DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public async Task DropDatabase(string name, ISql sql)
        {
            await sql.ExecuteBatchedSql(new StringReader($@"
DROP DATABASE IF EXISTS {EscapeIdentifier(name)}"), "information_schema");
        }

        public string EscapeIdentifier(string name)
        {
            return $"`{name}`";
        }

        public string InitialiseDatabase(string name)
        {
            return $@"
DROP DATABASE IF EXISTS {EscapeIdentifier(name)};
CREATE DATABASE {EscapeIdentifier(name)}";
        }

        public async IAsyncEnumerable<string> SplitStatementIntoBatches(TextReader statement)
        {
            var sqlBatch = new StringBuilder();
            string line;
            while ((line = await statement.ReadLineAsync()) != null)
            {
                var batches = line.Trim().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (batches.Length == 0)
                {
                    continue;
                }

                if (batches.Length == 1)
                {
                    sqlBatch.AppendLine(line);
                    continue;
                }

                foreach (var batch in batches.Take(batches.Length - 1))
                {
                    sqlBatch.Append(batch);
                    yield return sqlBatch.ToString();
                    sqlBatch.Clear();
                }

                sqlBatch.AppendLine(batches.Last());
            }

            if (sqlBatch.Length > 0)
            {
                yield return sqlBatch.ToString();
            }
        }
    }
}

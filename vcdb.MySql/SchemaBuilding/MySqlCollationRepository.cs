using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlCollationRepository : ICollationRepository
    {
        private readonly Options options;

        public MySqlCollationRepository(Options options)
        {
            this.options = options;
        }

        public async Task<Dictionary<string, string>> GetColumnCollations(DbConnection connection, ObjectName tableName)
        {
            var columns = await connection.QueryAsync<TableCollation>(@"SELECT 
   column_name,
   character_set_name, 
   collation_name 
FROM information_schema.columns 
WHERE table_name = @table_name
AND table_schema = @databaseName;",
new { table_name = tableName.Name, databaseName = options.Database });

            return columns.ToDictionary(
                column => column.column_name,
                column => column.collation_name);
        }

        public async Task<string> GetDatabaseCollation(DbConnection connection)
        {
            var databaseCollation = await connection.QuerySingleAsync<string>("SHOW VARIABLES LIKE 'collation_database';");
            if (databaseCollation == "collation_database")
            {
                return await GetServerCollation(connection);
            }

            return databaseCollation;
        }

        public async Task<string> GetServerCollation(DbConnection connection)
        {
            return await connection.QuerySingleAsync<string>("SELECT @@collation_server;");
        }
    }
}

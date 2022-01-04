using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using vcdb.CommandLine;
using vcdb.MySql.SchemaBuilding.Models;

namespace vcdb.MySql.SchemaBuilding.Internal
{
    public class MySqlGeneratedColumnRepository : IMySqlGeneratedColumnRepository
    {
        private readonly Options options;

        public MySqlGeneratedColumnRepository(Options options)
        {
            this.options = options;
        }

        public async Task<Dictionary<string, string>> GetComputedColumns(DbConnection connection, ObjectName tableName)
        {
            var computedColumns = await connection.QueryAsync<ComputedColumn>(@"select TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, GENERATION_EXPRESSION
from INFORMATION_SCHEMA.COLUMNS
where TABLE_SCHEMA = @databaseName
and TABLE_NAME = @tableName
and GENERATION_EXPRESSION <> ''", new { tableName = tableName.Name, databaseName = options.Database });

            return computedColumns.ToDictionary(
                col => col.COLUMN_NAME,
                col => col.GENERATION_EXPRESSION.UnwrapDefinition());
        }
    }
}

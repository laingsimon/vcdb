using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.MySql.SchemaBuilding.Models;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlComputedColumnRepository : IMySqlComputedColumnRepository
    {
        public async Task<Dictionary<string, string>> GetComputedColumns(DbConnection connection, ObjectName tableName)
        {
            var computedColums = await connection.QueryAsync<ComputedColumn>(@"select TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, GENERATION_EXPRESSION
from INFORMATION_SCHEMA.COLUMNS
where TABLE_SCHEMA = DATABASE()
and TABLE_NAME = @tableName
and GENERATION_EXPRESSION <> ''", new { tableName = tableName.Name });

            return computedColums.ToDictionary(
                col => col.COLUMN_NAME,
                col => col.GENERATION_EXPRESSION.UnwrapDefinition());
        }
    }
}

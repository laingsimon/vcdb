using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.SqlServer.SchemaBuilding.Models;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerComputedColumnRepository : ISqlServerComputedColumnRepository
    {
        public async Task<Dictionary<string, string>> GetComputedColumns(DbConnection connection, ObjectName tableName)
        {
            var computedColumns = await connection.QueryAsync<ComputedColumn>(@"select  col.name,
        computed.definition
from sys.columns col
inner join sys.tables tab
on tab.object_id = col.object_id
inner join sys.computed_columns computed
on computed.object_id = tab.object_id
and computed.column_id = col.column_id
where tab.name = @table_name
and schema_name(tab.schema_id) = @table_owner", new { table_name = tableName.Name, table_owner = tableName.Schema });

            return computedColumns.ToDictionary(
                col => col.name,
                col => col.definition.UnwrapDefinition());
        }
    }
}

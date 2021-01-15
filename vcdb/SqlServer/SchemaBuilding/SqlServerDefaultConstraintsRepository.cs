using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.SchemaBuilding;
using vcdb;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerDefaultConstraintsRepository : IDefaultConstraintRepository
    {
        public async Task<IDictionary<string, IColumnDefault>> GetColumnDefaults(DbConnection connection, TableName tableName)
        {
            var defaultColumns = connection.QueryAsync<ColumnDefault>($@"
select  def.name as [{nameof(IColumnDefault.Name)}],
        col.name as [{nameof(ColumnDefault.column_name)}],
        def.object_id as [{nameof(IColumnDefault.ObjectId)}],
        def.is_system_named as [{nameof(IColumnDefault.IsSystemNamed)}]
from sys.default_constraints def
inner join sys.columns col
on col.column_id = def.parent_column_id
and col.object_id = def.parent_object_id
inner join sys.tables tab
on tab.object_id = col.object_id
where tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner",
            new
            {
                table_name = tableName.Table,
                table_owner = tableName.Schema
            });

            return await defaultColumns.ToDictionaryAsync(
                defaultConstraint => defaultConstraint.column_name,
                defaultConstraint => (IColumnDefault)defaultConstraint);
        }
    }
}

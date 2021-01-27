using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.SchemaBuilding;
using vcdb;
using System;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerDefaultConstraintsRepository : IDefaultConstraintRepository
    {
        public async Task<IDictionary<string, ColumnDefault>> GetColumnDefaults(DbConnection connection, TableName tableName)
        {
            var defaultColumns = connection.QueryAsync<SqlServerColumnDefault>($@"
select  def.name,
        col.name as [column_name],
        def.object_id,
        def.is_system_named,
        def.definition
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
                defaultConstraint => NormaliseDefaultConstraint(defaultConstraint));
        }

        private ColumnDefault NormaliseDefaultConstraint(SqlServerColumnDefault defaultConstraint)
        {
            return new ColumnDefault
            {
                Definition = UnwrapDefinition(defaultConstraint.definition),
                IsSystemNamed = defaultConstraint.is_system_named,
                Name = defaultConstraint.name,
                ObjectId = defaultConstraint.object_id
            };
        }

        private object UnwrapDefinition(object definition)
        {
            if (definition is string stringDefinition)
            {
                if (string.IsNullOrEmpty(stringDefinition))
                    return definition;

                var withoutBrackets = stringDefinition.Trim('(', ')');
                if (withoutBrackets.StartsWith("'") && withoutBrackets.EndsWith("'"))
                    return withoutBrackets.Trim('\'');

                if (int.TryParse(withoutBrackets, out var intValue))
                    return intValue;

                if (decimal.TryParse(withoutBrackets, out var decimalValue))
                    return decimalValue;
            }

            return definition;
        }
    }
}

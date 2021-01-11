using Dapper;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerCheckConstraintsRepository : ICheckConstraintRepository
    {
        public async Task<CheckConstraintDetails[]> GetCheckConstraints(DbConnection connection, TableName tableName)
        {
            var checkConstraints = await connection.QueryAsync<CheckConstraint>(@"
select chk.name, col.name as column_name, chk.object_id, chk.definition, is_system_named as IS_SYSTEM_NAMED
from sys.check_constraints chk
left outer join sys.columns col
on col.column_id = chk.parent_column_id
and col.object_id = chk.parent_object_id
left outer join sys.tables tab
on tab.object_id = chk.parent_object_id
where tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner",
new
{
    table_name = tableName.Table,
    table_owner = tableName.Schema
});

            return checkConstraints
                .Select(chk => new CheckConstraintDetails
                {
                    Check = UnwrapCheckConstraint(chk.definition),
                    CheckObjectId = chk.object_id,
                    Name = chk.is_system_named
                        ? null
                        : chk.name,
                    ColumnNames = new[] { chk.column_name },
                    SqlName = chk.name
                })
                .ToArray();
        }

        private string UnwrapCheckConstraint(string definition)
        {
            if (string.IsNullOrEmpty(definition))
                return definition;

            var match = Regex.Match(definition, @"^\({0,1}(?<definition>.+?)\){0,1}$");
            return match.Success
                ? match.Groups["definition"].Value
                : definition;
        }
    }
}

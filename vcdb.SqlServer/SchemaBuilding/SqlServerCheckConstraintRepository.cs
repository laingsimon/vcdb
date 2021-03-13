using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;
using vcdb.SqlServer.SchemaBuilding.Models;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerCheckConstraintRepository : ICheckConstraintRepository
    {
        public async Task<IEnumerable<CheckConstraintDetails>> GetCheckConstraints(DbConnection connection, ObjectName tableName)
        {
            var checkConstraints = await connection.QueryAsync<CheckConstraint>(@"
select chk.name, column_usage.COLUMN_NAME, chk.object_id, chk.definition, chk.is_system_named
from sys.check_constraints chk
left outer join sys.tables tab
on tab.object_id = chk.parent_object_id
left outer join INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE column_usage
on column_usage.TABLE_NAME = tab.name
and column_usage.TABLE_SCHEMA = SCHEMA_NAME(tab.schema_id)
and column_usage.CONSTRAINT_NAME = chk.name
and column_usage.CONSTRAINT_SCHEMA = SCHEMA_NAME(chk.schema_id)
where tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner",
new
{
    table_name = tableName.Name,
    table_owner = tableName.Schema
});

            var groupedCheckConstraints = checkConstraints.GroupBy(chk => chk.object_id);

            return groupedCheckConstraints
                .Select(group =>
                {
                    var chk = group.First();

                    return new CheckConstraintDetails
                    {
                        Check = UnwrapCheckConstraint(chk.definition),
                        CheckObjectId = chk.object_id,
                        Name = chk.is_system_named
                            ? null
                            : chk.name,
                        ColumnNames = group.Select(check => check.COLUMN_NAME).ToArray(),
                        SqlName = chk.name
                    };
                })
                .ToArray();
        }

        private static string UnwrapCheckConstraint(string definition)
        {
            if (string.IsNullOrEmpty(definition))
                return definition;

            var match = Regex.Match(definition, @"^\({0,1}(?<definition>.+?)\){0,1}$");
            return match.Success
                ? UnwrapConstants(match.Groups["definition"].Value)
                : definition;
        }

        private static string UnwrapConstants(string definition)
        {
            return Regex.Replace(
                definition,
                @"(\(\d+(?:\.\d+)?\))|(\(\'.+?\'\))",
                match =>
                {
                    return match.Value.UnwrapDefinition();
                });
        }
    }
}

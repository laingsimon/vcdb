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
        private readonly ISqlObjectNameHelper sqlObjectNameHelper;

        public SqlServerCheckConstraintsRepository(ISqlObjectNameHelper sqlObjectNameHelper)
        {
            this.sqlObjectNameHelper = sqlObjectNameHelper;
        }

        public async Task<CheckConstraintDetails[]> GetCheckConstraints(DbConnection connection, TableName tableName)
        {
            var checkConstraints = await connection.QueryAsync<SqlServerCheckConstraintDetails>(@"
select chk.name as CHECK_NAME, col.name as COLUMN_NAME, chk.OBJECT_ID, chk.DEFINITION
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
                    Check = UnwrapCheckConstraint(chk.DEFINITION),
                    CheckObjectId = chk.OBJECT_ID,
                    Name = IsAutomaticName(chk.CHECK_NAME, chk.COLUMN_NAME, chk.OBJECT_ID, tableName)
                        ? null
                        : chk.CHECK_NAME,
                    ColumnNames = new[] { chk.COLUMN_NAME },
                    SqlName = chk.CHECK_NAME
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

        private bool IsAutomaticName(string checkName, string columnName, int objectId, TableName tableName)
        {
            var automaticConstraintName = sqlObjectNameHelper.GetAutomaticConstraintName(
                "CK",
                tableName.Table,
                columnName,
                objectId);

            return checkName == automaticConstraintName;
        }
    }
}

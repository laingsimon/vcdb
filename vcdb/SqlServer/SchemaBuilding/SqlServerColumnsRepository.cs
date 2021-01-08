using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerColumnsRepository : IColumnsRepository
    {
        private readonly ISqlObjectNameHelper sqlObjectNameHelper;
        private readonly IDescriptionRepository descriptionRepository;

        public SqlServerColumnsRepository(ISqlObjectNameHelper sqlObjectNameHelper, IDescriptionRepository descriptionRepository)
        {
            this.sqlObjectNameHelper = sqlObjectNameHelper;
            this.descriptionRepository = descriptionRepository;
        }

        public async Task<Dictionary<string, ColumnDetails>> GetColumns(DbConnection connection, TableName tableName)
        {
            var tableColumns = await connection.QueryAsync<SpColumnsOutput>(@"
sp_columns @table_name = @table_name, @table_owner = @table_owner",
new
{
    table_name = tableName.Table,
    table_owner = tableName.Schema
});

            var columnDefaults = (await connection.QueryAsync<ColumnDefaultDetails>(@"
select def.name as DEFAULT_NAME, col.name as COLUMN_NAME, def.OBJECT_ID
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
})).ToDictionary(defaultConstraint => defaultConstraint.COLUMN_NAME);

            var checkConstraints = (await connection.QueryAsync<CheckConstraintDetails>(@"
select chk.name as CHECK_NAME, col.name as COLUMN_NAME, chk.OBJECT_ID, chk.DEFINITION
from sys.check_constraints chk
inner join sys.columns col
on col.column_id = chk.parent_column_id
and col.object_id = chk.parent_object_id
inner join sys.tables tab
on tab.object_id = col.object_id
where tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner",
new
{
    table_name = tableName.Table,
    table_owner = tableName.Schema
})).ToDictionary(check => check.COLUMN_NAME);

            var columnDescriptions = await descriptionRepository.GetColumnDescriptions(connection, tableName);

            return tableColumns.ToDictionary(
                        column => column.COLUMN_NAME,
                        column =>
                        {
                            var columnDefault = columnDefaults.ItemOrDefault(column.COLUMN_NAME);
                            var checkConstraint = checkConstraints.ItemOrDefault(column.COLUMN_NAME);

                            return new ColumnDetails
                            {
                                Type = GetDataType(column),
                                Nullable = column.NULLABLE,
                                Default = column.COLUMN_DEF?.Trim('(', ')'),
                                DefaultName = columnDefault == null || IsAutomaticName(columnDefault, tableName)
                                    ? null
                                    : columnDefault.DEFAULT_NAME,
                                DefaultObjectId = columnDefault?.OBJECT_ID,
                                CheckObjectId = checkConstraint?.OBJECT_ID,
                                Description = columnDescriptions.ItemOrDefault(column.COLUMN_NAME),
                                Check = UnwrapCheckConstraint(checkConstraint?.DEFINITION),
                                CheckName = checkConstraint == null || IsAutomaticName(checkConstraint, tableName)
                                    ? null
                                    : checkConstraint.CHECK_NAME,
                            };
                        });
        }

        private bool IsAutomaticName(ISqlColumnNamedObject sqlObject, TableName tableName)
        {
            var automaticConstraintName = sqlObjectNameHelper.GetAutomaticConstraintName(
                sqlObject.Prefix,
                tableName.Table,
                sqlObject.ColumnName,
                sqlObject.ObjectId);

            return sqlObject.Name == automaticConstraintName;
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

        private string GetDataType(SpColumnsOutput column)
        {
            switch (column.TYPE_NAME)
            {
                case "char":
                case "varchar":
                    return $"{column.TYPE_NAME}({column.CHAR_OCTET_LENGTH})";
                case "nchar":
                case "nvarchar":
                    return $"{column.TYPE_NAME}({column.CHAR_OCTET_LENGTH / 2})";
                case "decimal":
                    return $"{column.TYPE_NAME}({column.PRECISION}, {column.SCALE})";
                default:
                    return column.TYPE_NAME;
            }
        }
    }
}

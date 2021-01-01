using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer
{
    public class SqlServerColumnsRepository : IColumnsRepository
    {
        public async Task<Dictionary<string, ColumnDetails>> GetColumns(DbConnection connection, TableIdentifier tableIdentifier)
        {
            var tableColumns = await connection.QueryAsync<SpColumnsOutput>(@"
sp_columns @table_name = @table_name, @table_owner = @table_owner",
new
{
table_name = tableIdentifier.TABLE_NAME,
table_owner = tableIdentifier.TABLE_SCHEMA
});

            var columnDefaults = (await connection.QueryAsync<ColumnDefaultDetails>(@"
select def.name as DEFAULT_NAME, col.name as COLUMN_NAME
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
    table_name = tableIdentifier.TABLE_NAME,
    table_owner = tableIdentifier.TABLE_SCHEMA
})).ToDictionary(col => col.COLUMN_NAME, col => col.DEFAULT_NAME);

            return tableColumns.ToDictionary(
                        column => column.COLUMN_NAME,
                        column =>
                        {
                            return new ColumnDetails
                            {
                                Type = GetDataType(column),
                                Nullable = column.NULLABLE,
                                Default = column.COLUMN_DEF?.Trim('(', ')'),
                                DefaultName = CheckForInBuiltDefaultName(columnDefaults.ItemOrDefault(column.COLUMN_NAME))
                            };
                        });
        }

        private string CheckForInBuiltDefaultName(string defaultName)
        {
            //example sql server provided name for a default: DF__Person__Deleted__35BCFE0A

            if (string.IsNullOrEmpty(defaultName))
                return null;

            return Regex.IsMatch(defaultName, $@"^DF__.+?__.+?__[A-F0-9]+$")
                ? null
                : defaultName;
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

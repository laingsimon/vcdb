using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
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

            return tableColumns.ToDictionary(
                        column => column.COLUMN_NAME,
                        column =>
                        {
                            return new ColumnDetails
                            {
                                Type = GetDataType(column),
                                Nullable = column.NULLABLE,
                                Default = column.COLUMN_DEF?.Trim('(', ')')
                            };
                        });
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

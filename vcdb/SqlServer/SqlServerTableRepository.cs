using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace vcdb.SqlServer
{
    public class SqlServerTableRepository : ITableRepository
    {
        private readonly Options options;

        public SqlServerTableRepository(Options options)
        {
            this.options = options;
        }

        public async Task<Dictionary<string, TableDetails>> GetTables(DbConnection connection)
        {
            var tables = await connection.QueryAsync<TableIdentifiers>(@"
select TABLE_NAME, TABLE_SCHEMA
from INFORMATION_SCHEMA.TABLES
where TABLE_TYPE = 'BASE TABLE'");

            return tables.ToDictionary(
                tableIdentifier => $"{tableIdentifier.TABLE_SCHEMA}.{tableIdentifier.TABLE_NAME}",
                tableIdentifier =>
            {
                var tableColumns = connection.Query<SpColumnsOutput>(@"
sp_columns @table_name = @table_name, @table_owner = @table_owner",
new 
{ 
    table_name = tableIdentifier.TABLE_NAME, 
    table_owner = tableIdentifier.TABLE_SCHEMA 
});

                return new TableDetails
                {
                    Columns = tableColumns.ToDictionary(
                        column => column.COLUMN_NAME,
                        column =>
                        {
                            return new ColumnDetails
                            {
                                Type = GetDataType(column),
                                Nullable = column.NULLABLE,
                                Default = column.COLUMN_DEF?.Trim('(', ')')
                            };
                        })
                };
            });
        }

        private string GetDataType(SpColumnsOutput  column)
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

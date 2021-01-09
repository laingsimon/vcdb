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

            var columnDescriptions = await descriptionRepository.GetColumnDescriptions(connection, tableName);

            return tableColumns.ToDictionary(
                        column => column.COLUMN_NAME,
                        column =>
                        {
                            var columnDefault = columnDefaults.ItemOrDefault(column.COLUMN_NAME);

                            return new ColumnDetails
                            {
                                Type = GetDataType(column),
                                Nullable = column.NULLABLE,
                                Default = column.COLUMN_DEF?.Trim('(', ')'),
                                DefaultName = columnDefault == null || IsAutomaticName(columnDefault, tableName)
                                    ? null
                                    : columnDefault.DEFAULT_NAME,
                                DefaultObjectId = columnDefault?.OBJECT_ID,
                                Description = columnDescriptions.ItemOrDefault(column.COLUMN_NAME)
                            };
                        });
        }

        private bool IsAutomaticName(ColumnDefaultDetails columnDefault, TableName tableName)
        {
            var automaticConstraintName = sqlObjectNameHelper.GetAutomaticConstraintName(
                "DF",
                tableName.Table,
                columnDefault.COLUMN_NAME,
                columnDefault.OBJECT_ID);

            return columnDefault.DEFAULT_NAME == automaticConstraintName;
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

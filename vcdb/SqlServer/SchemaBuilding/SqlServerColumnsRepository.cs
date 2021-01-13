using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerColumnsRepository : IColumnsRepository
    {
        private readonly IDescriptionRepository descriptionRepository;
        private readonly ICollationRepository collationRepository;
        private readonly IDefaultConstraintRepository defaultConstraintRepository;

        public SqlServerColumnsRepository(
            IDescriptionRepository descriptionRepository, 
            ICollationRepository collationRepository,
            IDefaultConstraintRepository defaultConstraintRepository)
        {
            this.descriptionRepository = descriptionRepository;
            this.collationRepository = collationRepository;
            this.defaultConstraintRepository = defaultConstraintRepository;
        }

        public async Task<Dictionary<string, ColumnDetails>> GetColumns(DbConnection connection, TableName tableName)
        {
            var tableColumns = await connection.QueryAsync<SpColumnsOutput>(@"
exec sp_columns 
    @table_name = @table_name,
    @table_owner = @table_owner", 
                new
                {
                    table_name = tableName.Table,
                    table_owner = tableName.Schema
                });

            var databaseCollation = await collationRepository.GetDatabaseCollation(connection);
            var columnDefaults = await defaultConstraintRepository.GetColumnDefaults(connection, tableName);
            var columnDescriptions = await descriptionRepository.GetColumnDescriptions(connection, tableName);
            var columnCollations = await collationRepository.GetColumnCollations(connection, tableName);

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
                                DefaultName = columnDefault == null || columnDefault.IsSystemNamed
                                    ? null
                                    : columnDefault.Name,
                                SqlDefaultName = columnDefault?.Name,
                                DefaultObjectId = columnDefault?.ObjectId,
                                Description = columnDescriptions.ItemOrDefault(column.COLUMN_NAME),
                                Collation = columnCollations.ItemOrDefault(column.COLUMN_NAME) == databaseCollation
                                    ? null
                                    : columnCollations.ItemOrDefault(column.COLUMN_NAME)
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

using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;
using vcdb.SqlServer.SchemaBuilding.Models;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerColumnRepository : IColumnRepository
    {
        private readonly IDescriptionRepository descriptionRepository;
        private readonly ICollationRepository collationRepository;
        private readonly ISqlServerDefaultConstraintRepository defaultConstraintRepository;
        private readonly IPrimaryKeyRepository primaryKeyRepository;
        private readonly ISqlServerComputedColumnRepository computedColumnRepository;

        public SqlServerColumnRepository(
            IDescriptionRepository descriptionRepository, 
            ICollationRepository collationRepository,
            ISqlServerDefaultConstraintRepository defaultConstraintRepository,
            IPrimaryKeyRepository primaryKeyRepository,
            ISqlServerComputedColumnRepository computedColumnRepository)
        {
            this.descriptionRepository = descriptionRepository;
            this.collationRepository = collationRepository;
            this.defaultConstraintRepository = defaultConstraintRepository;
            this.primaryKeyRepository = primaryKeyRepository;
            this.computedColumnRepository = computedColumnRepository;
        }

        public async Task<Dictionary<string, ColumnDetails>> GetColumns(
            DbConnection connection,
            ObjectName tableName,
            Permissions tablePermissions)
        {
            var databaseCollation = await collationRepository.GetDatabaseCollation(connection);
            var columnDefaults = await defaultConstraintRepository.GetColumnDefaults(connection, tableName);
            var columnDescriptions = await descriptionRepository.GetColumnDescriptions(connection, tableName);
            var columnCollations = await collationRepository.GetColumnCollations(connection, tableName);
            var columnsInPrimaryKey = await primaryKeyRepository.GetColumnsInPrimaryKey(connection, tableName);
            var computedColumns = await computedColumnRepository.GetComputedColumns(connection, tableName);

            return await connection.QueryAsync<SpColumnsOutput>(@"
exec sp_columns 
    @table_name = @table_name,
    @table_owner = @table_owner",
                new
                {
                    table_name = tableName.Name,
                    table_owner = tableName.Schema
                }).ToDictionaryAsync(
                        column => column.COLUMN_NAME,
                        column =>
                        {
                            var columnDefault = columnDefaults.ItemOrDefault(column.COLUMN_NAME);

                            return new ColumnDetails
                            {
                                Type = GetDataType(column),
                                Nullable = OptOut.From(column.NULLABLE),
                                Default = columnDefault?.Definition,
                                DefaultName = columnDefault == null || columnDefault.IsSystemNamed
                                    ? null
                                    : columnDefault.Name,
                                SqlDefaultName = columnDefault?.Name,
                                DefaultObjectId = columnDefault?.ObjectId,
                                Description = columnDescriptions.ItemOrDefault(column.COLUMN_NAME),
                                Collation = columnCollations.ItemOrDefault(column.COLUMN_NAME) == databaseCollation
                                    ? null
                                    : columnCollations.ItemOrDefault(column.COLUMN_NAME),
                                PrimaryKey = columnsInPrimaryKey.Contains(column.COLUMN_NAME),
                                Permissions = tablePermissions?.SubEntityPermissions?.ItemOrDefault(column.COLUMN_NAME),
                                Expression = computedColumns.ItemOrDefault(column.COLUMN_NAME)
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

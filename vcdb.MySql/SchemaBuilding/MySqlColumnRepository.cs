using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.MySql.SchemaBuilding.Internal;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlColumnRepository : IColumnRepository
    {
        private readonly IDescriptionRepository descriptionRepository;
        private readonly ICollationRepository collationRepository;
        private readonly IPrimaryKeyRepository primaryKeyRepository;
        private readonly IMySqlGeneratedColumnRepository generatedColumnRepository;
        private readonly IDataTypeParser dataTypeParser;
        private readonly IMySqlValueParser valueParser;

        public MySqlColumnRepository(
            IDescriptionRepository descriptionRepository,
            ICollationRepository collationRepository,
            IPrimaryKeyRepository primaryKeyRepository,
            IMySqlGeneratedColumnRepository generatedColumnRepository,
            IDataTypeParser dataTypeParser,
            IMySqlValueParser valueParser)
        {
            this.descriptionRepository = descriptionRepository;
            this.collationRepository = collationRepository;
            this.primaryKeyRepository = primaryKeyRepository;
            this.generatedColumnRepository = generatedColumnRepository;
            this.dataTypeParser = dataTypeParser;
            this.valueParser = valueParser;
        }

        public async Task<Dictionary<string, ColumnDetails>> GetColumns(DbConnection connection, ObjectName tableName, Permissions tablePermissions)
        {
            var databaseCollation = await collationRepository.GetDatabaseCollation(connection);
            var columnDescriptions = await descriptionRepository.GetColumnDescriptions(connection, tableName);
            var columnCollations = await collationRepository.GetColumnCollations(connection, tableName);
            var columnsInPrimaryKey = await primaryKeyRepository.GetColumnsInPrimaryKey(connection, tableName);
            var computedColumns = await generatedColumnRepository.GetComputedColumns(connection, tableName);

            return await connection.QueryAsync<DescribeOutput>($@"
describe {tableName.Name}").ToDictionaryAsync(
                        column => column.Field,
                        column =>
                        {
                            return new ColumnDetails
                            {
                                Type = NormaliseDataType(column, columnCollations),
                                Nullable = OptOut.From(column.Null == "YES"),
                                Default = valueParser.ParseDefault(column.Default),
                                /*DefaultName = columnDefault == null || columnDefault.IsSystemNamed
                                    ? null
                                    : columnDefault.Name,
                                SqlDefaultName = columnDefault?.Name,
                                DefaultObjectId = columnDefault?.ObjectId,*/
                                Description = columnDescriptions.ItemOrDefault(column.Field),
                                Collation = columnCollations.ItemOrDefault(column.Field) == databaseCollation || IsNationalCharacterColumn(column, columnCollations)
                                    ? null
                                    : columnCollations.ItemOrDefault(column.Field),
                                PrimaryKey = columnsInPrimaryKey.Contains(column.Field),
                                Permissions = tablePermissions?.SubEntityPermissions?.ItemOrDefault(column.Field),
                                Expression = computedColumns.ItemOrDefault(column.Field)
                            };
                        });
        }

        private bool IsNationalCharacterColumn(DescribeOutput column, Dictionary<string, string> columnCollations)
        {
            return dataTypeParser.IsNationalCharacterColumn(
                column.Type,
                columnCollations.ItemOrDefault(column.Field));
        }

        private string NormaliseDataType(DescribeOutput column, Dictionary<string, string> columnCollations)
        {
            return dataTypeParser.GetDataType(
                column.Type,
                columnCollations.ItemOrDefault(column.Field));
        }
    }
}

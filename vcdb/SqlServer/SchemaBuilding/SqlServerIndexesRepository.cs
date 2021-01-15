using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerIndexesRepository : IIndexesRepository
    {
        private readonly IDescriptionRepository descriptionRepository;
        private readonly IPrimaryKeyRepository primaryKeyRepository;

        public SqlServerIndexesRepository(
            IDescriptionRepository descriptionRepository,
            IPrimaryKeyRepository primaryKeyRepository)
        {
            this.descriptionRepository = descriptionRepository;
            this.primaryKeyRepository = primaryKeyRepository;
        }

        public async Task<Dictionary<string, IndexDetails>> GetIndexes(DbConnection connection, TableName tableName)
        {
            var sql = @"select ix.name as index_name, ix.type_desc, ix.is_unique, ixc.is_descending_key, ixc.is_included_column, col.name as column_name
from sys.indexes ix
inner join sys.index_columns ixc
on ixc.index_id = ix.index_id
inner join sys.tables t
on t.object_id = ix.object_id
inner join sys.columns col
on (col.object_id = ix.object_id and col.column_id = ixc.column_id and ixc.object_id = t.object_id)
where t.name = @tableName
and schema_name(t.schema_id) = @schemaName";

            var indexesAndColumns = await connection.QueryAsync<SqlIndexDetails>(
                sql,
                new
                {
                    tableName = tableName.Table,
                    schemaName = tableName.Schema
                });

            var indexDescriptions = await descriptionRepository.GetIndexDescriptions(connection, tableName);
            var primaryKey = await primaryKeyRepository.GetPrimaryKeyDetails(connection, tableName);
            var columnsInEachIndex = indexesAndColumns
                .GroupBy(indexColumn => indexColumn.index_name)
                .Where(group => primaryKey == null || group.Key != primaryKey.SqlName);

            return columnsInEachIndex.ToDictionary(
                group => group.Key,
                group =>
                {
                    var indexDetails = group.First();

                    return new IndexDetails
                    {
                        Columns = group.Where(c => !c.is_included_column).ToDictionary(
                            col => col.column_name,
                            col => new IndexColumnDetails
                            {
                                Descending = col.is_descending_key
                            }),
                        Including = group.Where(c => c.is_included_column).Select(c => c.column_name).ToHashSet(),
                        Clustered = indexDetails.type_desc == "CLUSTERED",
                        Unique = indexDetails.is_unique,
                        Description = indexDescriptions.ItemOrDefault(group.Key)
                    };
                });
        }
    }
}

using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerPrimaryKeyRepository : IPrimaryKeyRepository
    {
        private readonly IDescriptionRepository descriptionRepository;

        public SqlServerPrimaryKeyRepository(IDescriptionRepository descriptionRepository)
        {
            this.descriptionRepository = descriptionRepository;
        }

        public async Task<HashSet<string>> GetColumnsInPrimaryKey(DbConnection connection, TableName tableName)
        {
            var columns = await connection.QueryAsync<SqlIndexDetails>(@"select ind.name as index_name, ind.type_desc, ind.is_unique, ic.is_descending_key, ic.is_included_column, col.name as column_name
from sys.key_constraints k
inner join sys.tables tab
on tab.object_id = k.parent_object_id
inner join sys.indexes ind
on ind.object_id = k.parent_object_id and ind.name = k.name
inner join sys.index_columns ic
on ic.index_id = k.unique_index_id
and ic.object_id = tab.object_id
inner join sys.columns col
on col.object_id = tab.object_id
and col.column_id = ic.column_id
where k.type = 'PK'
and ind.type_desc not in ('HEAP')
and tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner
order by ic.key_ordinal",
new { table_name = tableName.Table, table_owner = tableName.Schema });

            return new HashSet<string>(columns.Select(col => col.column_name));
        }

        public async Task<PrimaryKeyDetails> GetPrimaryKeyDetails(
            DbConnection connection,
            TableName tableName)
        {
            var primaryKey = await connection.QuerySingleOrDefaultAsync<PrimaryKey>(@"
select k.name, k.object_id, k.is_system_named, ind.is_unique, ind.type_desc
from sys.key_constraints k
inner join sys.tables tab
on tab.object_id = k.parent_object_id
inner join sys.indexes ind
on ind.object_id = k.parent_object_id and ind.name = k.name
where k.type = 'PK'
and ind.type_desc not in ('HEAP')
and tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner", 
new { table_name = tableName.Table, table_owner = tableName.Schema });

            if (primaryKey == null)
                return null;

            return new PrimaryKeyDetails
            {
                Name = primaryKey.is_system_named 
                    ? null
                    : primaryKey.name,
                SqlName = primaryKey.name,
                ObjectId = primaryKey.object_id,
                Description = await descriptionRepository.GetPrimaryKeyDescription(connection, tableName, primaryKey.name),
                Clustered = OptOut.From(primaryKey.type_desc == "CLUSTERED")
            };
        }
    }
}

using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;
using vcdb.SqlServer.SchemaBuilding.Models;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerForeignKeyRepository : IForeignKeyRepository
    {
        private readonly IDescriptionRepository descriptionRepository;

        public SqlServerForeignKeyRepository(IDescriptionRepository descriptionRepository)
        {
            this.descriptionRepository = descriptionRepository;
        }

        public async Task<Dictionary<string, ForeignKeyDetails>> GetForeignKeys(DbConnection connection, ObjectName tableName)
        {
            var foreignKeys = await connection.QueryAsync<ForeignKey>(@"
select	k.object_id,
        k.name, 
        k.delete_referential_action_desc, 
        k.update_referential_action_desc, 
        k.is_system_named, 
        source_col.name as source_column, 
        schema_name(referenced_table.schema_id) as referenced_table_schema, 
        referenced_table.name as referenced_table, 
        referenced_col.name as referenced_column
from sys.foreign_keys k
inner join sys.tables tab
on tab.object_id = k.parent_object_id
inner join sys.foreign_key_columns fk_col
on fk_col.constraint_object_id = k.object_id
inner join sys.columns source_col
on source_col.object_id = fk_col.parent_object_id
and source_col.column_id = fk_col.parent_column_id
inner join sys.columns referenced_col
on referenced_col.object_id = fk_col.referenced_object_id
and referenced_col.column_id = fk_col.referenced_column_id
inner join sys.tables referenced_table
on referenced_table.object_id = fk_col.referenced_object_id
where tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner",
new { table_name = tableName.Name, table_owner = tableName.Schema });

            var groupedForeignKeys = foreignKeys.GroupBy(key => key.object_id);
            var descriptions = await descriptionRepository.GetForeignKeyDescription(connection, tableName);

            return groupedForeignKeys.ToDictionary(
                group => group.First().name,
                group =>
                {
                    var keyDetails = group.First();

                    return new ForeignKeyDetails
                    {
                        Columns = group.ToDictionary(g => g.source_column, g => g.referenced_column),
                        OnDelete = GetAction(keyDetails.delete_referential_action_desc),
                        OnUpdate = GetAction(keyDetails.update_referential_action_desc),
                        ReferencedTable = new ObjectName
                        {
                            Name = keyDetails.referenced_table,
                            Schema = keyDetails.referenced_table_schema
                        },
                        Description = descriptions.ItemOrDefault(keyDetails.name)
                    };
                });
        }

        private ForeignActionOption? GetAction(string action_desc)
        {
            switch (action_desc)
            {
                case "NO_ACTION":
                    return null;
                case "CASCADE":
                    return ForeignActionOption.Cascade;
                case "SET NULL":
                    return ForeignActionOption.SetNull;
                case "SET DEFAULT":
                    return ForeignActionOption.SetDefault;
                default:
                    throw new NotSupportedException($"Action description: `{action_desc}` isn't supported");
            }
        }
    }
}

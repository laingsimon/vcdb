using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlIndexRepository : IIndexRepository
    {
        private const string PrimaryKeyIndexName = "PRIMARY";

        private readonly IDescriptionRepository descriptionRepository;
        private readonly Options options;
        private readonly Version minimumCompatibilityVersion;

        public MySqlIndexRepository(
            IDescriptionRepository descriptionRepository,
            Options options,
            DatabaseVersion databaseVersion)
        {
            this.descriptionRepository = descriptionRepository;
            this.options = options;
            minimumCompatibilityVersion = databaseVersion.MinimumCompatibilityVersion == null
                ? new Version(0, 0)
                : new Version(databaseVersion.MinimumCompatibilityVersion.Contains(".")
                    ? databaseVersion.MinimumCompatibilityVersion
                    : databaseVersion.MinimumCompatibilityVersion + ".0");
        }

        public async Task<Dictionary<string, IndexDetails>> GetIndexes(DbConnection connection, ObjectName tableName)
        {
            var indexesAndColumns = await connection.QueryAsync<SqlIndexDetails>(
                @"select index_name, index_type as type_desc, case non_unique when 0 then 1 else 0 end as is_unique, column_name
from information_schema.statistics
where table_schema = @databaseName
and table_name = @tableName
order by index_name, seq_in_index",
                new
                {
                    tableName = tableName.Name,
                    databaseName = options.Database
                });

            var indexDescriptions = await descriptionRepository.GetIndexDescriptions(connection, tableName);
            var columnsInEachIndex = indexesAndColumns
                .GroupBy(indexColumn => indexColumn.index_name)
                .Where(g => g.Key != PrimaryKeyIndexName);

            return columnsInEachIndex.ToDictionary(
                group => group.Key,
                group =>
                {
                    var indexDetails = group.First();

                    return new IndexDetails
                    {
                        Columns = group.ToDictionary(
                            col => col.column_name,
                            col => new IndexColumnDetails
                            {
                                Descending = IsIndexColumnDescending(col)
                            }),
                        Clustered = IsClusteredIndex(indexDetails),
                        Unique = indexDetails.is_unique,
                        Description = indexDescriptions.ItemOrDefault(group.Key)
                    };
                });
        }

        private static bool IsClusteredIndex(SqlIndexDetails indexDetails)
        {
            // TODO: Whats the name of a clustered index in MySql??
            return false;
        }

        private bool IsIndexColumnDescending(SqlIndexDetails indexColumn)
        {
            if (minimumCompatibilityVersion <= new Version(8, 0))
            {
                // prior to v8, DESCENDING index columns aren't supported
                // https://stackoverflow.com/questions/10109108/how-do-i-create-a-desc-index-in-mysql
                return false;
            }

            // TODO: Identify how to identify whether the column is descending or not.
            return false;
        }
    }
}

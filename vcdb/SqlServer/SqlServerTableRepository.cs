using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SqlServer
{
    public class SqlServerTableRepository : ITableRepository
    {
        private readonly IColumnsRepository columnsRepository;
        private readonly IIndexesRepository indexesRepository;

        public SqlServerTableRepository(IColumnsRepository columnsRepository, IIndexesRepository indexesRepository)
        {
            this.columnsRepository = columnsRepository;
            this.indexesRepository = indexesRepository;
        }

        public async Task<Dictionary<string, TableDetails>> GetTables(DbConnection connection)
        {
            var tables = await connection.QueryAsync<TableIdentifier>(@"
select TABLE_NAME, TABLE_SCHEMA
from INFORMATION_SCHEMA.TABLES
where TABLE_TYPE = 'BASE TABLE'");

            return await tables.ToDictionaryAsync(
                tableIdentifier => $"{tableIdentifier.TABLE_SCHEMA}.{tableIdentifier.TABLE_NAME}",
                async tableIdentifier =>
            {
                return new TableDetails
                {
                    Columns = await columnsRepository.GetColumns(connection, tableIdentifier),
                    Indexes = await indexesRepository.GetIndexes(connection, tableIdentifier)
                };
            });
        }
    }
}

using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerDatabaseRepository : IDatabaseRepository
    {
        private readonly ITableRepository tableRepository;
        private readonly ISchemaRepository schemaRepository;
        private readonly IDescriptionRepository descriptionRepository;

        public SqlServerDatabaseRepository(
            ITableRepository tableRepository,
            ISchemaRepository schemaRepository,
            IDescriptionRepository descriptionRepository)
        {
            this.tableRepository = tableRepository;
            this.schemaRepository = schemaRepository;
            this.descriptionRepository = descriptionRepository;
        }

        public async Task<DatabaseDetails> GetDatabaseDetails(DbConnection connection)
        {
            return new DatabaseDetails
            {
                Tables = await tableRepository.GetTables(connection),
                Schemas = await schemaRepository.GetSchemas(connection),
                Description = await descriptionRepository.GetDatabaseDescription(connection)
            };
        }
    }
}

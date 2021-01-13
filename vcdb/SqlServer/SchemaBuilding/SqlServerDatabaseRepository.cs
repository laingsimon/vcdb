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
        private readonly ICollationRepository collationRepository;

        public SqlServerDatabaseRepository(
            ITableRepository tableRepository,
            ISchemaRepository schemaRepository,
            IDescriptionRepository descriptionRepository,
            ICollationRepository collationRepository)
        {
            this.tableRepository = tableRepository;
            this.schemaRepository = schemaRepository;
            this.descriptionRepository = descriptionRepository;
            this.collationRepository = collationRepository;
        }

        public async Task<DatabaseDetails> GetDatabaseDetails(DbConnection connection)
        {
            var serverCollation = await collationRepository.GetServerCollation(connection);
            var databaseCollation = await collationRepository.GetDatabaseCollation(connection);

            return new DatabaseDetails
            {
                Tables = await tableRepository.GetTables(connection),
                Schemas = await schemaRepository.GetSchemas(connection),
                Description = await descriptionRepository.GetDatabaseDescription(connection),
                Collation = databaseCollation == serverCollation
                    ? null
                    : databaseCollation,
                ServerCollation = serverCollation
            };
        }
    }
}

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
        private readonly IUserRepository userRepository;
        private readonly IPermissionRepository permissionRepository;
        private readonly IProcedureRepository procedureRepository;

        public SqlServerDatabaseRepository(
            ITableRepository tableRepository,
            ISchemaRepository schemaRepository,
            IDescriptionRepository descriptionRepository,
            ICollationRepository collationRepository,
            IUserRepository userRepository,
            IPermissionRepository permissionRepository,
            IProcedureRepository procedureRepository)
        {
            this.tableRepository = tableRepository;
            this.schemaRepository = schemaRepository;
            this.descriptionRepository = descriptionRepository;
            this.collationRepository = collationRepository;
            this.userRepository = userRepository;
            this.permissionRepository = permissionRepository;
            this.procedureRepository = procedureRepository;
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
                ServerCollation = serverCollation,
                Users = await userRepository.GetUsers(connection),
                Permissions = await permissionRepository.GetDatabasePermissions(connection),
                Procedures = await procedureRepository.GetProcedures(connection)
            };
        }
    }
}

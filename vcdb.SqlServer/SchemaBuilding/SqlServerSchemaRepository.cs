using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerSchemaRepository : ISchemaRepository
    {
        private static readonly HashSet<string> BuiltInSchemas = new HashSet<string>()
        {
            "dbo",
            "guest",
            "INFORMATION_SCHEMA",
            "sys",
            "db_owner",
            "db_accessadmin",
            "db_securityadmin",
            "db_ddladmin",
            "db_backupoperator",
            "db_datareader",
            "db_datawriter",
            "db_denydatareader",
            "db_denydatawriter"
        };
        private readonly IDescriptionRepository descriptionRepository;
        private readonly IPermissionRepository permissionRepository;

        public SqlServerSchemaRepository(
            IDescriptionRepository descriptionRepository,
            IPermissionRepository permissionRepository)
        {
            this.descriptionRepository = descriptionRepository;
            this.permissionRepository = permissionRepository;
        }

        public async Task<Dictionary<string, SchemaDetails>> GetSchemas(DbConnection connection)
        {
            var schemas = await connection.QueryAsync<SqlServerSchemata>(@"
select *from INFORMATION_SCHEMA.SCHEMATA");

            var permissions = await permissionRepository.GetSchemaPermissions(connection);

            return await schemas.Where(schema => !BuiltInSchemas.Contains(schema.SCHEMA_NAME)).ToDictionaryAsync(
                schema => schema.SCHEMA_NAME,
                async schema => new SchemaDetails
                {
                    Description = await descriptionRepository.GetSchemaDescription(connection, schema.SCHEMA_NAME),
                    Permissions = permissions.ItemOrDefault(schema.SCHEMA_NAME)
                });
        }
    }
}

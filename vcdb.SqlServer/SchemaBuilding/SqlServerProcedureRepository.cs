using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerProcedureRepository : IProcedureRepository
    {
        private readonly IPermissionRepository permissionRepository;
        private readonly IDescriptionRepository descriptionRepository;

        public SqlServerProcedureRepository(
            IPermissionRepository permissionRepository,
            IDescriptionRepository descriptionRepository)
        {
            this.permissionRepository = permissionRepository;
            this.descriptionRepository = descriptionRepository;
        }

        public async Task<Dictionary<ObjectName, ProcedureDetails>> GetProcedures(DbConnection connection)
        {
            var procedures = await connection.QueryAsync<StoredProcedure>(@"
select p.name, p.object_id, schema_name(p.schema_id) as schema_name, m.definition, m.is_schema_bound
from sys.procedures p
inner join sys.sql_modules m
on m.object_id = p.object_id
");

            var permissions = await permissionRepository.GetProcedurePermissions(connection);

            return await procedures.ToDictionaryAsync(
                procedure => procedure.GetName(),
                async procedure =>
                {
                    var procedurePermissions = permissions.ItemOrDefault(procedure.GetName());

                    return new ProcedureDetails
                    {
                        Definition = procedure.definition,
                        Encrypted = procedure.definition == null,
                        SchemaBound = procedure.is_schema_bound,
                        Permissions = procedurePermissions,
                        Description = await descriptionRepository.GetProcedureDescription(connection, procedure.GetName())
                    };
                });
        }
    }
}

using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerTableRepository : ITableRepository
    {
        private readonly IColumnsRepository columnsRepository;
        private readonly IIndexesRepository indexesRepository;
        private readonly IDescriptionRepository descriptionRepository;
        private readonly ICheckConstraintRepository checkConstraintRepository;
        private readonly IPrimaryKeyRepository primaryKeyRepository;
        private readonly IPermissionRepository permissionRepository;

        public SqlServerTableRepository(
            IColumnsRepository columnsRepository,
            IIndexesRepository indexesRepository,
            IDescriptionRepository descriptionRepository,
            ICheckConstraintRepository checkConstraintRepository,
            IPrimaryKeyRepository primaryKeyRepository,
            IPermissionRepository permissionRepository)
        {
            this.columnsRepository = columnsRepository;
            this.indexesRepository = indexesRepository;
            this.descriptionRepository = descriptionRepository;
            this.checkConstraintRepository = checkConstraintRepository;
            this.primaryKeyRepository = primaryKeyRepository;
            this.permissionRepository = permissionRepository;
        }

        public async Task<Dictionary<TableName, TableDetails>> GetTables(DbConnection connection)
        {
            var tables = await connection.QueryAsync<TableName>(@"
select TABLE_NAME as [Table], TABLE_SCHEMA as [Schema]
from INFORMATION_SCHEMA.TABLES
where TABLE_TYPE = 'BASE TABLE'");

            var permissions = await permissionRepository.GetTablePermissions(connection);

            return await tables.ToDictionaryAsync(
                tableName => tableName,
                async tableName =>
                {
                    var tablePermissions = permissions.ItemOrDefault(tableName);

                    return new TableDetails
                    {
                        Columns = await columnsRepository.GetColumns(connection, tableName, tablePermissions),
                        Indexes = await indexesRepository.GetIndexes(connection, tableName),
                        Description = await descriptionRepository.GetTableDescription(connection, tableName),
                        Checks = await checkConstraintRepository.GetCheckConstraints(connection, tableName),
                        PrimaryKey = await primaryKeyRepository.GetPrimaryKeyDetails(connection, tableName),
                        Permissions = tablePermissions
                    };
                });
        }
    }
}

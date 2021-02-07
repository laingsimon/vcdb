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
    public class SqlServerPermissionRepository : IPermissionRepository
    {
        internal const string GrantWithGrantOption = "GRANT_WITH_GRANT_OPTION";
        internal const string Grant = "GRANT";
        internal const string Deny = "DENY";
        internal const string Revoke = "REVOKE";

        public async Task<Permissions> GetDatabasePermissions(DbConnection connection)
        {
            var permissionRecords = await connection.QueryAsync<PermissionRecord>(@"
select  class_desc,
        major_id,
        schema_name(major_id) as object_name,
        minor_id,
        null as minor_name,
        USER_NAME(grantee_principal_id) as grantee_principal,
        USER_NAME(grantor_principal_id) as grantor_principal,
        permission_name,
        state_desc
from sys.database_permissions
where class_desc = 'DATABASE'
and permission_name not in ('VIEW ANY COLUMN MASTER KEY DEFINITION', 'VIEW ANY COLUMN ENCRYPTION KEY DEFINITION')");

            return new Permissions
            {
                Grant = permissionRecords
                    .Where(permission => permission.IsGrant())
                    .ToPermissionNameThenGranteeMapping(record => new PermissionDetails { WithGrant = record.state_desc == GrantWithGrantOption }),
                Deny = permissionRecords
                    .Where(permission => permission.IsDeny())
                    .ToPermissionNameThenGranteeHashSet(),
                Revoke = permissionRecords
                    .Where(permission => permission.IsRevoke())
                    .ToPermissionNameThenGranteeHashSet()
            };
        }

        public async Task<Dictionary<string, Permissions>> GetSchemaPermissions(DbConnection connection)
        {
            var permissionRecords = await connection.QueryAsync<PermissionRecord>(@"
select  class_desc,
        major_id,
        schema_name(major_id) as object_name,
        minor_id,
        null as minor_name,
        USER_NAME(grantee_principal_id) as grantee_principal,
        USER_NAME(grantor_principal_id) as grantor_principal,
        permission_name,
        state_desc
from sys.database_permissions
where major_id > 0
and class_desc = 'SCHEMA'");

            return BuildPermissions(
                permissionRecords.ToArray(),
                r => r.object_name,
                GetEntityPermissions);
        }

        public async Task<Dictionary<ObjectName, Permissions>> GetTablePermissions(DbConnection connection)
        {
            var permissionRecords = await connection.QueryAsync<PermissionRecord>(@"
select  perm.class_desc,
        perm.major_id,
        schema_name(tab.schema_id) + '.' + tab.name as object_name,
        col.name as minor_name,
        USER_NAME(perm.grantee_principal_id) as grantee_principal,
        USER_NAME(perm.grantor_principal_id) as grantor_principal,
        perm.permission_name,
        perm.state_desc
from sys.database_permissions perm
inner join sys.tables tab
on tab.object_id = perm.major_id
left join sys.columns col
on col.object_id = tab.object_id
and col.column_id = perm.minor_id
where perm.major_id > 0
and perm.class_desc = 'OBJECT_OR_COLUMN'");

            return BuildPermissions(
                permissionRecords.ToArray(),
                r => ObjectName.Parse(r.object_name),
                GetEntityAndSubEntityPermissions);
        }

        private Dictionary<TKey, TEntityPermissions> BuildPermissions<TKey, TEntityPermissions>(
            IReadOnlyCollection<PermissionRecord> records,
            Func<PermissionRecord, TKey> entityNameSelector,
            Func<IGrouping<TKey, PermissionRecord>, TEntityPermissions> entityPermissionFactory)
            where TEntityPermissions: Permissions, new ()
        {
            var perEntityPermissions = records.GroupBy(entityNameSelector);

            return perEntityPermissions.ToDictionary(
                group => group.Key, //entity name, e.g. schema name, table name, etc.
                entityPermissionFactory);
        }

        private Permissions GetEntityPermissions(IEnumerable<PermissionRecord> permissionsForEntity)
        {
            return GetEntityPermissions(permissionsForEntity, false, null);
        }

        private Permissions GetEntityAndSubEntityPermissions(IEnumerable<PermissionRecord> permissionsForEntity)
        {
            return GetEntityPermissions(permissionsForEntity, true, null);
        }

        private Permissions GetEntityPermissions(
            IEnumerable<PermissionRecord> permissionsForEntity,
            bool processSubEntityPermissions,
            string thisLevelMinorEntityName)
        {
            var subEntityPermissions = processSubEntityPermissions
                ? permissionsForEntity.Where(permission => permission.minor_name != thisLevelMinorEntityName)
                    .GroupBy(subEntityPermission => subEntityPermission.minor_name)
                    .ToDictionary(
                        subEntityPermissionGroup => subEntityPermissionGroup.Key,
                        subEntityPermissionGroup => GetEntityPermissions(subEntityPermissionGroup, false, subEntityPermissionGroup.Key))
                : null;

            return new Permissions
            {
                Grant = permissionsForEntity
                    .Where(permission => permission.IsGrant() && permission.minor_name == thisLevelMinorEntityName)
                    .ToPermissionNameThenGranteeMapping(record => new PermissionDetails { WithGrant = record.state_desc == GrantWithGrantOption }),
                Deny = permissionsForEntity
                    .Where(permission => permission.IsDeny() && permission.minor_name == thisLevelMinorEntityName)
                    .ToPermissionNameThenGranteeHashSet(),
                Revoke = permissionsForEntity
                    .Where(permission => permission.IsRevoke() && permission.minor_name == thisLevelMinorEntityName)
                    .ToPermissionNameThenGranteeHashSet(),
                SubEntityPermissions = subEntityPermissions
            };
        }

        public async Task<Dictionary<ObjectName, Permissions>> GetProcedurePermissions(DbConnection connection)
        {
            var permissionRecords = await connection.QueryAsync<PermissionRecord>(@"
select  perm.class_desc,
        perm.major_id,
        schema_name(p.schema_id) + '.' + p.name as object_name,
        null as minor_name,
        USER_NAME(perm.grantee_principal_id) as grantee_principal,
        USER_NAME(perm.grantor_principal_id) as grantor_principal,
        perm.permission_name,
        perm.state_desc
from sys.database_permissions perm
inner join sys.procedures p
on p.object_id = perm.major_id
where perm.major_id > 0
and perm.class_desc = 'OBJECT_OR_COLUMN'");

            return BuildPermissions(
                permissionRecords.ToArray(),
                r => ObjectName.Parse(r.object_name),
                GetEntityPermissions);
        }
    }
}

using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerPermissionRepository : IPermissionRepository
    {
        private const string GrantWithGrantOption = "GRANT_WITH_GRANT_OPTION";
        private const string Grant = "GRANT";
        private const string Deny = "DENY";
        private const string Revoke = "REVOKE";

        public async Task<Dictionary<string, PermissionStates>> GetSchemaPermissions(DbConnection connection)
        {
            var permissionRecords = await connection.QueryAsync<PermissionRecord>(@"
select  class_desc,
        major_id,
        major_name = case class_desc
            when 'SCHEMA' then schema_name(major_id)
            else null
        end,
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
                r => r.major_name);
        }

        private Dictionary<TKey, PermissionStates> BuildPermissions<TKey>(IReadOnlyCollection<PermissionRecord> records, Func<PermissionRecord, TKey> entityNameSelector)
        {
            var perEntityPermissions = records.GroupBy(entityNameSelector);

            return perEntityPermissions.ToDictionary(
                group => group.Key, //entity name, e.g. schema name, table name, etc.
                permissionsForSchema =>
                {
                    return new PermissionStates
                    {
                        Grant = permissionsForSchema
                            .Where(permission => permission.state_desc == Grant || permission.state_desc == GrantWithGrantOption)
                            .GroupBy(grants => grants.permission_name)
                            .ToDictionary(
                                grantByPermissionName => grantByPermissionName.Key, //permission name, e.g. CONTROL
                                grantByPermissionName =>
                                {
                                    return grantByPermissionName.ToDictionary(
                                        record => record.grantee_principal, //the grantee, i.e. the username
                                        record => new PermissionDetails
                                        {
                                            WithGrant = record.state_desc == GrantWithGrantOption
                                        });
                                }),
                        Deny = permissionsForSchema
                            .Where(permission => permission.state_desc == Deny)
                            .GroupBy(denys => denys.permission_name)
                            .ToDictionary(
                                denyByPermissionName => denyByPermissionName.Key, //permissionName, e.g. CONTROL
                                denyByPermissionName =>
                                {
                                    return denyByPermissionName.ToHashSet(record => record.grantee_principal);
                                }),
                        Revoke = permissionsForSchema
                            .Where(permission => permission.state_desc == Revoke)
                            .GroupBy(revokes => revokes.permission_name)
                            .ToDictionary(
                                denyByPermissionName => denyByPermissionName.Key, //permissionName, e.g. CONTROL
                                denyByPermissionName =>
                                {
                                    return denyByPermissionName.ToHashSet(record => record.grantee_principal); //TODO: Consider the inclusion of the column name?
                                })
                    };
                });
        }
    }
}

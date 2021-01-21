using System;
using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.SqlServer.SchemaBuilding
{
    internal static class SqlServerPermissionsExtensions
    {
        public static Dictionary<PermissionName, Dictionary<UserPrincipal, PermissionDetails>> ToPermissionNameThenGranteeMapping(
            this IEnumerable<PermissionRecord> relevantPermissionRecords,
            Func<PermissionRecord, PermissionDetails> permissionDetailsFactory = null)
        {
            return relevantPermissionRecords
                .GroupBy(denys => denys.PermissionName)
                .ToDictionary(
                    denyByPermissionName => denyByPermissionName.Key,
                    denyByPermissionName =>
                    {
                        return denyByPermissionName.ToDictionary(
                            record => record.GranteePrincipal, //the grantee, i.e. the username
                            record => permissionDetailsFactory != null 
                                ? permissionDetailsFactory(record) 
                                : new PermissionDetails());
                    });
        }

        public static Dictionary<PermissionName, HashSet<UserPrincipal>> ToPermissionNameThenGranteeHashSet(
            this IEnumerable<PermissionRecord> relevantPermissionRecords)
        {
            return relevantPermissionRecords
                .GroupBy(denys => denys.PermissionName)
                .ToDictionary(
                    denyByPermissionName => denyByPermissionName.Key,
                    denyByPermissionName => denyByPermissionName.ToHashSet(record => record.GranteePrincipal));
        }

        public static bool IsGrant(this PermissionRecord permission)
        {
            return permission.state_desc == SqlServerPermissionRepository.Grant || permission.state_desc == SqlServerPermissionRepository.GrantWithGrantOption;
        }

        public static bool IsDeny(this PermissionRecord permission)
        {
            return permission.state_desc == SqlServerPermissionRepository.Deny;
        }

        public static bool IsRevoke(this PermissionRecord permission)
        {
            return permission.state_desc == SqlServerPermissionRepository.Revoke;
        }
    }
}

using System;
using System.Collections.Generic;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting.Permission;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerPermissionScriptBuilder : IPermissionScriptBuilder
    {
        public IEnumerable<SqlScript> CreateDatabasePermissionScripts(PermissionDifferences permissionDifferences)
        {
            if (permissionDifferences == null)
            {
                yield break;
            }

            foreach (var denyPermission in permissionDifferences.DenyChanges.OrEmptyCollection())
            {
                foreach (var script in CreateDatabaseDenyScripts(denyPermission))
                {
                    yield return script;
                }
            }

            foreach (var grantPermission in permissionDifferences.GrantChanges.OrEmptyCollection())
            {
                foreach (var script in CreateDatabaseGrantScripts(grantPermission))
                {
                    yield return script;
                }
            }

            foreach (var revokePermission in permissionDifferences.RevokeChanges.OrEmptyCollection())
            {
                foreach (var script in CreateDatabaseRevokeScripts(revokePermission))
                {
                    yield return script;
                }
            }
        }

        public IEnumerable<SqlScript> CreateTablePermissionScripts(
            ObjectName tableName,
            PermissionDifferences permissionDifferences)
        {
            if (permissionDifferences == null)
            {
                yield break;
            }

            foreach (var denyPermission in permissionDifferences.DenyChanges.OrEmptyCollection())
            {
                foreach (var script in CreateTableDenyScripts(tableName, denyPermission))
                {
                    yield return script;
                }
            }

            foreach (var grantPermission in permissionDifferences.GrantChanges.OrEmptyCollection())
            {
                foreach (var script in CreateTableGrantScripts(tableName, grantPermission))
                {
                    yield return script;
                }
            }
        }

        public IEnumerable<SqlScript> CreateSchemaPermissionScripts(string schemaName, PermissionDifferences permissionDifferences)
        {
            if (permissionDifferences == null)
            {
                yield break;
            }

            foreach (var denyPermission in permissionDifferences.DenyChanges.OrEmptyCollection())
            {
                foreach (var script in CreateSchemaDenyScripts(schemaName, denyPermission))
                {
                    yield return script;
                }
            }

            foreach (var grantPermission in permissionDifferences.GrantChanges.OrEmptyCollection())
            {
                foreach (var script in CreateSchemaGrantScripts(schemaName, grantPermission))
                {
                    yield return script;
                }
            }
        }

        public IEnumerable<SqlScript> CreateColumnPermissionScripts(ObjectName tableName, string columnName, PermissionDifferences permissionDifferences)
        {
            if (permissionDifferences == null)
            {
                yield break;
            }

            foreach (var denyPermission in permissionDifferences.DenyChanges.OrEmptyCollection())
            {
                foreach (var script in CreateColumnDenyScripts(tableName, columnName, denyPermission))
                {
                    yield return script;
                }
            }

            foreach (var grantPermission in permissionDifferences.GrantChanges.OrEmptyCollection())
            {
                foreach (var script in CreateColumnGrantScripts(tableName, columnName, grantPermission))
                {
                    yield return script;
                }
            }

            foreach (var revokePermission in permissionDifferences.RevokeChanges.OrEmptyCollection())
            {
                foreach (var script in CreateColumnRevokeScripts(tableName, columnName, revokePermission))
                {
                    yield return script;
                }
            }
        }

        public IEnumerable<SqlScript> CreateProcedurePermissionScripts(ObjectName procedureName, PermissionDifferences permissionDifferences)
        {
            if (permissionDifferences == null)
            {
                yield break;
            }

            foreach (var denyPermission in permissionDifferences.DenyChanges.OrEmptyCollection())
            {
                foreach (var script in CreateProcedureDenyScripts(procedureName, denyPermission))
                {
                    yield return script;
                }
            }

            foreach (var grantPermission in permissionDifferences.GrantChanges.OrEmptyCollection())
            {
                foreach (var script in CreateProcedureGrantScripts(procedureName, grantPermission))
                {
                    yield return script;
                }
            }

            foreach (var revokePermission in permissionDifferences.RevokeChanges.OrEmptyCollection())
            {
                foreach (var script in CreateProcedureRevokeScripts(procedureName, revokePermission))
                {
                    yield return script;
                }
            }
        }
        
        private IEnumerable<SqlScript> CreateDatabaseDenyScripts(PermissionNameDifference<HashSet<UserPrincipal>> deny)
        {
            return DenyOrRevoke(
                deny,
                (permission, user) => ApplyDatabasePermission("DENY", permission, user, cascade: true),
                (permission, user) => ApplyDatabasePermission("REVOKE", permission, user));
        }

        private IEnumerable<SqlScript> CreateDatabaseGrantScripts(PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>> grant)
        {
            return GrantOrRevoke(
                grant,
                (permission, user, withGrantOption) => ApplyDatabasePermission("GRANT", permission, user, withGrantOption),
                (permission, user) => ApplyDatabasePermission("REVOKE", permission, user, cascade: true));
        }

        private IEnumerable<SqlScript> CreateDatabaseRevokeScripts(PermissionNameDifference<HashSet<UserPrincipal>> deny)
        {
            return DenyOrRevoke(
                deny,
                (permission, user) => ApplyDatabasePermission("REVOKE", permission, user, cascade: true),
                (permission, user) => throw new NotImplementedException("How do you revoke a revoke!"));
        }

        private IEnumerable<SqlScript> CreateTableDenyScripts(ObjectName tableName, PermissionNameDifference<HashSet<UserPrincipal>> deny)
        {
            return DenyOrRevoke(
                deny,
                (permission, user) => ApplyObjectPermission("DENY", permission, tableName.SqlSafeName(), user, cascade: true),
                (permission, user) => ApplyObjectPermission("REVOKE", permission, tableName.SqlSafeName(), user));
        }

        private IEnumerable<SqlScript> CreateTableGrantScripts(ObjectName tableName, PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>> grant)
        {
            return GrantOrRevoke(
                grant,
                (permission, user, withGrantOption) => ApplyObjectPermission("GRANT", permission, tableName.SqlSafeName(), user, withGrantOption),
                (permission, user) => ApplyObjectPermission("REVOKE", permission, tableName.SqlSafeName(), user, cascade: true));
        }

        private IEnumerable<SqlScript> CreateSchemaGrantScripts(string schemaName, PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>> grant)
        {
            return GrantOrRevoke(
                grant,
                (permission, user, withGrantOption) => ApplySchemaPermission("GRANT", permission, schemaName, user, withGrantOption),
                (permission, user) => ApplySchemaPermission("REVOKE", permission, schemaName, user, cascade: true));
        }

        private IEnumerable<SqlScript> CreateSchemaDenyScripts(string schemaName, PermissionNameDifference<HashSet<UserPrincipal>> deny)
        {
            return DenyOrRevoke(
                deny,
                (permission, user) => ApplySchemaPermission("DENY", permission, schemaName, user, cascade: true),
                (permission, user) => ApplySchemaPermission("REVOKE", permission, schemaName, user));
        }

        private IEnumerable<SqlScript> CreateColumnGrantScripts(ObjectName tableName, string columnName, PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>> grant)
        {
            return GrantOrRevoke(
                grant,
                (permission, user, withGrantOption) => ApplyColumnPermission("GRANT", permission, tableName, columnName, user),
                (permission, user) => ApplyColumnPermission("REVOKE", permission, tableName, columnName, user));
        }

        private IEnumerable<SqlScript> CreateColumnDenyScripts(ObjectName tableName, string columnName, PermissionNameDifference<HashSet<UserPrincipal>> deny)
        {
            return DenyOrRevoke(
                deny,
                (permission, user) => ApplyColumnPermission("DENY", permission, tableName, columnName, user),
                (permission, user) => ApplyColumnPermission("REVOKE", permission, tableName, columnName, user));
        }

        private IEnumerable<SqlScript> CreateColumnRevokeScripts(ObjectName tableName, string columnName, PermissionNameDifference<HashSet<UserPrincipal>> revoke)
        {
            return DenyOrRevoke(
                revoke,
                (permission, user) => ApplyColumnPermission("REVOKE", permission, tableName, columnName, user),
                (permission, user) => throw new NotImplementedException($"Dont know how to revoke a revoke!"));
        }

        private IEnumerable<SqlScript> CreateProcedureDenyScripts(ObjectName objectName, PermissionNameDifference<HashSet<UserPrincipal>> deny)
        {
            return DenyOrRevoke(
                deny,
                (permission, user) => ApplyObjectPermission("DENY", permission, objectName.SqlSafeName(), user, cascade: true),
                (permission, user) => ApplyObjectPermission("REVOKE", permission, objectName.SqlSafeName(), user));
        }

        private IEnumerable<SqlScript> CreateProcedureGrantScripts(ObjectName objectName, PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>> grant)
        {
            return GrantOrRevoke(
                grant,
                (permission, user, withGrantOption) => ApplyObjectPermission("GRANT", permission, objectName.SqlSafeName(), user, withGrantOption),
                (permission, user) => ApplyObjectPermission("REVOKE", permission, objectName.SqlSafeName(), user, cascade: true));
        }

        private IEnumerable<SqlScript> CreateProcedureRevokeScripts(ObjectName objectName, PermissionNameDifference<HashSet<UserPrincipal>> revoke)
        {
            return DenyOrRevoke(
                revoke,
                (permission, user) => ApplyObjectPermission("REVOKE", permission, objectName.SqlSafeName(), user),
                (permission, user) => throw new NotSupportedException("How to revoke a revoke"));
        }


        private static IEnumerable<SqlScript> DenyOrRevoke(
            PermissionNameDifference<HashSet<UserPrincipal>> permissions,
            Func<PermissionName, UserPrincipal, SqlScript> apply,
            Func<PermissionName, UserPrincipal, SqlScript> revert)
        {
            var permissionName = (permissions.RequiredPermission ?? permissions.CurrentPermission).Key;

            if (permissions.PermissionAdded)
            {
                foreach (var userPrincipal in permissions.RequiredPermission.Value)
                {
                    yield return apply(permissionName, userPrincipal);
                }
            }
            else if (permissions.PermissionDeleted)
            {
                foreach (var userPrincipal in permissions.CurrentPermission.Value)
                {
                    yield return revert(permissionName, userPrincipal);
                }
            }
            else
            {
                foreach (var userPermissionDifference in permissions.UserPermissionDifferences.OrEmptyCollection())
                {
                    var userPrincipal = userPermissionDifference.RequiredPermission.Key;
                    yield return apply(permissionName, userPrincipal);
                }
            }
        }

        private static IEnumerable<SqlScript> GrantOrRevoke(
            PermissionNameDifference<Dictionary<UserPrincipal, PermissionDetails>> permissions,
            Func<PermissionName, UserPrincipal, bool, SqlScript> apply,
            Func<PermissionName, UserPrincipal, SqlScript> revert)
        {
            var permissionName = (permissions.RequiredPermission ?? permissions.CurrentPermission).Key;

            if (permissions.PermissionAdded)
            {
                foreach (var userPrincipal in permissions.RequiredPermission.Value)
                {
                    yield return apply(permissionName, userPrincipal.Key, userPrincipal.Value.WithGrant);
                }
            }
            else if (permissions.PermissionDeleted)
            {
                foreach (var userPrincipal in permissions.CurrentPermission.Value)
                {
                    yield return revert(permissionName, userPrincipal.Key);
                }
            }
            else
            {
                foreach (var userPermissionDifference in permissions.UserPermissionDifferences.OrEmptyCollection())
                {
                    var userPrincipal = userPermissionDifference.RequiredPermission.Key;
                    var withGrantOption = userPermissionDifference.RequiredPermission.Value.WithGrant;

                    if (userPermissionDifference.WithGrantChangedTo?.Value == false)
                    {
                        yield return revert(permissionName, userPrincipal);
                    }

                    yield return apply(permissionName, userPrincipal, withGrantOption);
                }
            }
        }

        private static SqlScript ApplyObjectPermission(
            string permissionCategory,
            PermissionName permissionName,
            string sqlSafeObjectName,
            UserPrincipal userPrincipal,
            bool withGrantOption = false,
            bool cascade = false)
        {
            var withGrantOptionClause = withGrantOption
                ? " WITH GRANT OPTION"
                : "";
            var cascadeClause = cascade
                ? " CASCADE"
                : "";
            return new SqlScript($@"{permissionCategory} {permissionName.SqlSafeName()} ON {sqlSafeObjectName} TO {userPrincipal.SqlSafeName()}{withGrantOptionClause}{cascadeClause}
GO");
        }

        private static SqlScript ApplyColumnPermission(string permissionCategory, PermissionName permissionName, ObjectName tableName, string columnName, UserPrincipal userPrincipal)
        {
            return new SqlScript($@"{permissionCategory} {permissionName.SqlSafeName()} ON {tableName.SqlSafeName()} ({columnName.SqlSafeName()}) TO {userPrincipal.SqlSafeName()}
GO");
        }

        private static SqlScript ApplySchemaPermission(
            string permissionCategory,
            PermissionName permissionName,
            string schemaName,
            UserPrincipal userPrincipal,
            bool withGrantOption = false,
            bool cascade = false)
        {
            var withGrantOptionClause = withGrantOption
                ? " WITH GRANT OPTION"
                : "";
            var cascadeClause = cascade
                ? " CASCADE"
                : "";
            return new SqlScript($@"{permissionCategory} {permissionName.SqlSafeName()} ON SCHEMA :: {schemaName.SqlSafeName()} TO {userPrincipal.SqlSafeName()}{withGrantOptionClause}{cascadeClause}
GO");
        }

        private static SqlScript ApplyDatabasePermission(
            string permissionCategory,
            PermissionName permissionName,
            UserPrincipal userPrincipal,
            bool withGrantOption = false,
            bool cascade = false)
        {
            var withGrantOptionClause = withGrantOption
                ? " WITH GRANT OPTION"
                : "";
            var cascadeClause = cascade
                ? " CASCADE"
                : "";
            return new SqlScript($@"{permissionCategory} {permissionName.SqlSafeName()} TO {userPrincipal.SqlSafeName()}{withGrantOptionClause}{cascadeClause}
GO");
        }
    }
}

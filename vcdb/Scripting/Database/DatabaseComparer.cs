using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Scripting.Collation;
using vcdb.Scripting.Permission;
using vcdb.Scripting.Schema;
using vcdb.Scripting.Table;
using vcdb.Scripting.User;

namespace vcdb.Scripting.Database
{
    public class DatabaseComparer : IDatabaseComparer
    {
        private readonly ITableComparer tableComparer;
        private readonly ISchemaComparer schemaComparer;
        private readonly ICollationComparer collationComparer;
        private readonly IUserComparer userComparer;
        private readonly IPermissionComparer permissionComparer;
        private readonly Options options;

        public DatabaseComparer(
            ITableComparer tableComparer,
            ISchemaComparer schemaComparer,
            ICollationComparer collationComparer,
            IUserComparer userComparer,
            IPermissionComparer permissionComparer,
            Options options)
        {
            this.tableComparer = tableComparer;
            this.schemaComparer = schemaComparer;
            this.collationComparer = collationComparer;
            this.userComparer = userComparer;
            this.permissionComparer = permissionComparer;
            this.options = options;
        }

        public DatabaseDifference GetDatabaseDifferences(
            ComparerContext context,
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase)
        {
            return new DatabaseDifference
            {
                TableDifferences = tableComparer.GetDifferentTables(
                    context.ForDatabase(currentDatabase, requiredDatabase),
                    currentDatabase.Tables.OrEmpty(),
                    requiredDatabase.Tables.OrEmpty()).ToArray(),
                SchemaDifferences = schemaComparer.GetSchemaDifferences(
                    context.ForDatabase(currentDatabase, requiredDatabase),
                    currentDatabase.Schemas.OrEmpty(),
                    requiredDatabase.Schemas.OrEmpty()).ToArray(),
                DescriptionChangedTo = currentDatabase.Description != requiredDatabase.Description
                    ? requiredDatabase.Description.AsChange()
                    : null,
                CollationChangedTo = collationComparer.GetDatabaseCollationChange(
                    currentDatabase,
                    requiredDatabase),
                UserDifferences = userComparer.GetUserDifferences(
                    currentDatabase.Users.OrEmpty(),
                    requiredDatabase.Users.OrEmpty()).ToArray(),
                PermissionDifferences = GetPermissionDifferences(context, currentDatabase, requiredDatabase)
            };
        }

        private PermissionDifferences GetPermissionDifferences(
            ComparerContext context,
            DatabaseDetails currentDatabase,
            DatabaseDetails requiredDatabase)
        {
            if (requiredDatabase.Permissions == null && !options.ExplicitDatabasePermissions)
            {
                return null;
            }

            return permissionComparer.GetPermissionDifferences(
                    context,
                    currentDatabase.Permissions,
                    requiredDatabase.Permissions);
        }
    }
}

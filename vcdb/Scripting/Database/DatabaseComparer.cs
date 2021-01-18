using System.Linq;
using vcdb.Models;
using vcdb.Scripting.Collation;
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

        public DatabaseComparer(
            ITableComparer tableComparer,
            ISchemaComparer schemaComparer,
            ICollationComparer collationComparer,
            IUserComparer userComparer)
        {
            this.tableComparer = tableComparer;
            this.schemaComparer = schemaComparer;
            this.collationComparer = collationComparer;
            this.userComparer = userComparer;
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
                    requiredDatabase.Users.OrEmpty()).ToArray()
            };
        }
    }
}

using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class DatabaseComparer : IDatabaseComparer
    {
        private readonly ITableComparer tableComparer;
        private readonly ISchemaComparer schemaComparer;

        public DatabaseComparer(ITableComparer tableComparer, ISchemaComparer schemaComparer)
        {
            this.tableComparer = tableComparer;
            this.schemaComparer = schemaComparer;
        }

        public DatabaseDifference GetDatabaseDifferences(DatabaseDetails currentDatabase, DatabaseDetails requiredDatabase)
        {
            return new DatabaseDifference
            {
                TableDifferences = tableComparer.GetDifferentTables(currentDatabase.Tables, requiredDatabase.Tables).ToArray(),
                SchemaDifferences = schemaComparer.GetSchemaDifferences(currentDatabase.Schemas, requiredDatabase.Schemas).ToArray(),
                DescriptionChangedTo = currentDatabase.Description != requiredDatabase.Description
                    ? requiredDatabase.Description
                    : DatabaseDifference.UnchangedDescription
            };
        }
    }
}

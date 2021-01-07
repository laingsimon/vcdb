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
                TableDifferences = tableComparer.GetDifferentTables(currentDatabase.Tables.OrEmpty(), requiredDatabase.Tables.OrEmpty()).ToArray(),
                SchemaDifferences = schemaComparer.GetSchemaDifferences(currentDatabase.Schemas.OrEmpty(), requiredDatabase.Schemas.OrEmpty()).ToArray(),
                DescriptionChangedTo = currentDatabase.Description != requiredDatabase.Description
                    ? requiredDatabase.Description.AsChange()
                    : null
            };
        }
    }
}

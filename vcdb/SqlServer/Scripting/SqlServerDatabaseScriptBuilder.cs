using System.Collections.Generic;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public class SqlServerDatabaseScriptBuilder : IDatabaseScriptBuilder
    {
        private readonly Options options;
        private readonly ITableScriptBuilder tableScriptBuilder;
        private readonly ISchemaScriptBuilder schemaScriptBuilder;
        private readonly IDatabaseComparer databaseComparer;
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;

        public SqlServerDatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder,
            ISchemaScriptBuilder schemaScriptBuilder,
            IDatabaseComparer databaseComparer,
            IDescriptionScriptBuilder descriptionScriptBuilder)
        {
            this.options = options;
            this.tableScriptBuilder = tableScriptBuilder;
            this.schemaScriptBuilder = schemaScriptBuilder;
            this.databaseComparer = databaseComparer;
            this.descriptionScriptBuilder = descriptionScriptBuilder;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            var databaseDifferences = databaseComparer.GetDatabaseDifferences(current, required);
            if (databaseDifferences.DescriptionChangedTo != null)
                yield return descriptionScriptBuilder.ChangeDatabaseDescription(current.Description, databaseDifferences.DescriptionChangedTo.Value);

            foreach (var schemaScript in schemaScriptBuilder.CreateUpgradeScripts(databaseDifferences.SchemaDifferences, databaseDifferences.TableDifferences))
            {
                yield return schemaScript;
            }

            foreach (var tableScript in tableScriptBuilder.CreateUpgradeScripts(databaseDifferences.TableDifferences))
            {
                yield return tableScript;
            }
        }
    }
}

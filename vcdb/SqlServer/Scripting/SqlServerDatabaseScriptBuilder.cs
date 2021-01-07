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
        private readonly ISqlServerSchemaScriptBuilder schemaScriptBuilder;
        private readonly IDatabaseComparer databaseComparer;
        private readonly IDescriptionScriptBuilder descriptionScriptBuilder;

        public SqlServerDatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder,
            ISqlServerSchemaScriptBuilder schemaScriptBuilder,
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
            if (databaseDifferences.DescriptionHasChanged)
                yield return descriptionScriptBuilder.ChangeDatabaseDescription(current.Description, databaseDifferences.DescriptionChangedTo);

            foreach (var creationScript in schemaScriptBuilder.CreateUpgradeScripts(databaseDifferences.SchemaDifferences, true))
            {
                yield return creationScript;
            }

            foreach (var upgradeScript in tableScriptBuilder.CreateUpgradeScripts(databaseDifferences.TableDifferences))
            {
                yield return upgradeScript;
            }

            foreach (var upgradeScript in schemaScriptBuilder.CreateUpgradeScripts(databaseDifferences.SchemaDifferences, false))
            {
                yield return upgradeScript;
            }
        }
    }
}

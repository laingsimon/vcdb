using System.Collections.Generic;
using System.Linq;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class DatabaseScriptBuilder : IDatabaseScriptBuilder
    {
        private readonly Options options;
        private readonly ITableScriptBuilder tableScriptBuilder;
        private readonly ISchemaScriptBuilder schemaScriptBuilder;
        private readonly IDatabaseComparer databaseComparer;

        public DatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder,
            ISchemaScriptBuilder schemaScriptBuilder,
            IDatabaseComparer databaseComparer)
        {
            this.options = options;
            this.tableScriptBuilder = tableScriptBuilder;
            this.schemaScriptBuilder = schemaScriptBuilder;
            this.databaseComparer = databaseComparer;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            var databaseDifferences = databaseComparer.GetDatabaseDifferences(current, required);
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

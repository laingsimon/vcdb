using System.Collections.Generic;
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
            foreach (var script in tableScriptBuilder.CreateUpgradeScripts(databaseDifferences.TableDifferences))
            {
                yield return script;
            }

            foreach (var script in schemaScriptBuilder.CreateUpgradeScripts(databaseDifferences.SchemaDifferences))
            {
                yield return script;
            }
        }
    }
}

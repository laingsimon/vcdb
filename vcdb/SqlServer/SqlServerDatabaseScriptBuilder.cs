using System.Collections.Generic;
using vcdb.CommandLine;
using vcdb.Models;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer
{
    public class SqlServerDatabaseScriptBuilder : IDatabaseScriptBuilder
    {
        private readonly Options options;
        private readonly ITableScriptBuilder tableScriptBuilder;

        public SqlServerDatabaseScriptBuilder(
            Options options,
            ITableScriptBuilder tableScriptBuilder)
        {
            this.options = options;
            this.tableScriptBuilder = tableScriptBuilder;
        }

        public IEnumerable<SqlScript> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            var tableScripts = tableScriptBuilder.CreateUpgradeScripts(current.Tables, required.Tables);
            foreach (var script in tableScripts)
            {
                yield return script;
            }
        }
    }
}

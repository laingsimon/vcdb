using System.Collections.Generic;
using vcdb.Models;
using vcdb.Scripting.Database;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.MySql.Scripting
{
    public class MySqlDatabaseScriptBuilder : IDatabaseScriptBuilder
    {
        public IEnumerable<IScriptTask> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required)
        {
            yield break;
        }
    }
}

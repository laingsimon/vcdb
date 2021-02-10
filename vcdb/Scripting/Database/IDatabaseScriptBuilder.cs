using System.Collections.Generic;
using vcdb.Models;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.Database
{
    public interface IDatabaseScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(DatabaseDetails current, DatabaseDetails required);
    }
}

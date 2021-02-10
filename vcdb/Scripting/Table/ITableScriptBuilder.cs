using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.Table
{
    public interface ITableScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences);
    }
}

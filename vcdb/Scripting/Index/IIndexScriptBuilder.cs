using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.Index
{
    public interface IIndexScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(ObjectName requiredTableName, IReadOnlyCollection<IndexDifference> indexDifferences);
    }
}
using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.Programmability
{
    public interface IProcedureScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(IReadOnlyCollection<ProcedureDifference> procedureDifferences);
    }
}

using System.Collections.Generic;

namespace vcdb.Scripting.ExecutionPlan
{
    public interface IScriptExecutionPlanManager
    {
        ScriptExecutionPlan CreateExecutionPlan(IEnumerable<IScriptTask> tasks);
    }
}

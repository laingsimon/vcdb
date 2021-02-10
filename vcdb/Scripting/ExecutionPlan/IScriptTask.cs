using vcdb.Output;

namespace vcdb.Scripting.ExecutionPlan
{
    public interface IScriptTask : IOutputable
    {
        ScriptTaskDependency[] Requires { get; }
        ScriptTaskDependency[] ResultsIn { get; }

        IScriptTask ResultingIn(ScriptTaskDependency dependency);
        IScriptTask Requiring(ScriptTaskDependency dependency);
    }
}

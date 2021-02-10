using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Output;

namespace vcdb.Scripting.ExecutionPlan
{
    [DebuggerDisplay("{task,nq}")]
    public class ScriptTask : IScriptTask
    {
        public ScriptTaskDependency[] Requires { get; }
        public ScriptTaskDependency[] ResultsIn { get; }
        private readonly IOutputable task;

        public ScriptTask(
           IOutputable task,
           IEnumerable<ScriptTaskDependency> requires = null,
           IEnumerable<ScriptTaskDependency> resultsIn = null)
        {
            this.task = task;
            Requires = requires?.ToArray() ?? new ScriptTaskDependency[0];
            ResultsIn = resultsIn?.ToArray() ?? new ScriptTaskDependency[0];
        }

        public IScriptTask Requiring(ScriptTaskDependency dependency)
        {
            return new ScriptTask(
                task,
                Requires.Concat(new[] { dependency }),
                ResultsIn);
        }

        public IScriptTask ResultingIn(ScriptTaskDependency dependency)
        {
            return new ScriptTask(
                task,
                Requires,
                ResultsIn.Concat(new[] { dependency }));
        }

        public async Task WriteToOutput(IOutput output)
        {
            await task.WriteToOutput(output);
        }
    }
}

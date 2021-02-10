using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Output;

namespace vcdb.Scripting.ExecutionPlan
{
    /// <summary>
    /// A collection of scripts that must be executed in the given order
    /// </summary>
    [DebuggerDisplay("Block of {tasks.Count} task/s")]
    public class ScriptBlock : IScriptTask
    {
        protected readonly IReadOnlyCollection<IScriptTask> tasks;
        protected readonly IEnumerable<ScriptTaskDependency> requires;
        protected readonly IEnumerable<ScriptTaskDependency> resultsIn;

        public ScriptBlock(
            IEnumerable<IScriptTask> tasks,
            IEnumerable<ScriptTaskDependency> requires = null,
            IEnumerable<ScriptTaskDependency> resultsIn = null)
        {
            this.tasks = tasks.SelectMany(task => task is IEnumerable<IScriptTask> enumerable ? enumerable : new[] { task }).ToArray();
            this.requires = requires?.ToArray() ?? new ScriptTaskDependency[0];
            this.resultsIn = resultsIn?.ToArray() ?? new ScriptTaskDependency[0];
        }

        public ScriptTaskDependency[] Requires => requires.Concat(tasks.SelectMany(task => task.Requires).ToArray()).ToArray();
        public ScriptTaskDependency[] ResultsIn => resultsIn.Concat(tasks.SelectMany(task => task.ResultsIn).ToArray()).ToArray();

        public virtual IScriptTask Requiring(ScriptTaskDependency dependency)
        {
            return new ScriptBlock(
                tasks,
                requires.Concat(new[] { dependency }),
                resultsIn);
        }

        public virtual IScriptTask ResultingIn(ScriptTaskDependency dependency)
        {
            return new ScriptBlock(
                tasks,
                requires,
                resultsIn.Concat(new[] { dependency }));
        }

        public async Task WriteToOutput(IOutput output)
        {
            foreach (var task in tasks)
            {
                await task.WriteToOutput(output);
            }
        }
    }
}

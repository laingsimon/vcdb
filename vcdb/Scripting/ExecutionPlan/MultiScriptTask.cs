using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace vcdb.Scripting.ExecutionPlan
{
    [DebuggerDisplay("{tasks.Count} task/s")]
    public class MultiScriptTask : ScriptBlock, IEnumerable<IScriptTask>
    {
        public MultiScriptTask(
            IEnumerable<IScriptTask> tasks,
            IEnumerable<ScriptTaskDependency> requires = null,
            IEnumerable<ScriptTaskDependency> resultsIn = null)
           :base(tasks, requires, resultsIn)
        { }

        public override IScriptTask Requiring(ScriptTaskDependency dependency)
        {
            return new MultiScriptTask(
                tasks,
                requires.Concat(new[] { dependency }),
                resultsIn);
        }

        public override IScriptTask ResultingIn(ScriptTaskDependency dependency)
        {
            return new MultiScriptTask(
                tasks,
                requires,
                resultsIn.Concat(new[] { dependency }));
        }

        public IEnumerator<IScriptTask> GetEnumerator()
        {
            return base.tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

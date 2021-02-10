using System.Collections.Generic;
using System.Linq;

namespace vcdb.Scripting.ExecutionPlan
{
    public class ScriptExecutionPlanManager : IScriptExecutionPlanManager
    {
        public ScriptExecutionPlan CreateExecutionPlan(IEnumerable<IScriptTask> tasks)
        {
            return new ScriptExecutionPlan(UnwrapTasks(tasks).Aggregate(
                new IScriptTask[0],
                (currentTasks, newTask) => GetNewPlan(currentTasks, newTask).ToArray()));
        }

        private IEnumerable<IScriptTask> UnwrapTasks(IEnumerable<IScriptTask> tasks)
        {
            foreach (var task in tasks)
            {
                if (task is IEnumerable<IScriptTask> wrapper)
                {
                    foreach (var subTask in UnwrapTasks(wrapper))
                    {
                        yield return subTask;
                    }
                    continue;
                }

                yield return task;
            }
        }

        private IEnumerable<IScriptTask> GetNewPlan(IReadOnlyCollection<IScriptTask> tasks, IScriptTask task)
        {
            if (tasks.Count == 0)
            {
                yield return task;
                yield break;
            }

            //..work through the items add the new item at the right place... Or at the end...

            var taskEmitted = false;
            foreach (var existingTask in tasks)
            {
                if (!taskEmitted && existingTask.RequiresTask(task))
                {
                    taskEmitted = true;
                    yield return task;
                }

                yield return existingTask;
            }

            if (!taskEmitted)
            {
                yield return task;
            }
        }
    }
}

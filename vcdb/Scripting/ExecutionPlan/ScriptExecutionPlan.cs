using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Output;

namespace vcdb.Scripting.ExecutionPlan
{
    public class ScriptExecutionPlan : IOutputable
    {
        internal readonly IScriptTask[] tasks;

        public ScriptExecutionPlan(IEnumerable<IScriptTask> tasks)
        {
            this.tasks = tasks.ToArray();
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

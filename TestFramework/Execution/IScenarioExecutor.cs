using System.IO;
using System.Threading.Tasks;

namespace TestFramework.Execution
{
    internal interface IScenarioExecutor
    {
        Task<ExecutionResultStatus> Execute(DirectoryInfo scenarioDirectory);
    }
}
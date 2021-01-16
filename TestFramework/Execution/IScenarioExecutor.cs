using System.IO;
using System.Threading.Tasks;

namespace TestFramework.Execution
{
    internal interface IScenarioExecutor
    {
        Task<bool> Execute(DirectoryInfo scenarioDirectory);
    }
}
using System.IO;
using System.Threading.Tasks;

namespace TestFramework
{
    internal interface IScenarioExecutor
    {
        Task Execute(DirectoryInfo scenarioDirectory);
    }
}
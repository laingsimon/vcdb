using System.IO;
using System.Threading.Tasks;
using TestFramework.Input;

namespace TestFramework.Execution
{
    public interface IVcdbProcess
    {
        Task<ExecutionResult> Execute(ScenarioSettings settings, DirectoryInfo scenario);
    }
}
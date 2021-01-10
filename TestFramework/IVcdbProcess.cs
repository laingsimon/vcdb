using System.IO;
using System.Threading.Tasks;

namespace TestFramework
{
    public interface IVcdbProcess
    {
        Task<ExecutionResult> Execute(ScenarioSettings settings, DirectoryInfo scenario);
    }
}
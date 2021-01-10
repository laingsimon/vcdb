using System.Threading.Tasks;

namespace vcdb.IntegrationTests
{
    internal interface IExecutor
    {
        Task<ExecutorResult> ExecuteProcess(string scenarioName = null);
    }
}
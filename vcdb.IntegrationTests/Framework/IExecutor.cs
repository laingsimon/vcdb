using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Framework
{
    internal interface IExecutor
    {
        Task<ExecutorResult> ExecuteProcess(string productName, string scenarioName = null);
    }
}
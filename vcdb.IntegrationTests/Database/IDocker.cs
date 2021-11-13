using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests.Database
{
    internal interface IDocker
    {
        Task<bool> IsContainerRunning(IntegrationTestOptions options);
        bool IsInstalled();
        Task<StartResult> IsDockerHostRunning();
        Task<bool> StartDockerHost(CancellationToken cancellationToken = default);
        Task<bool> StartDockerCompose(IntegrationTestOptions options, CancellationToken cancellationToken = default);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal interface IDocker
    {
        Task<bool> IsContainerRunning();
        bool IsInstalled();
        Task<StartResult> IsDockerHostRunning();
        Task<bool> StartDockerHost(CancellationToken cancellationToken = default);
        Task<bool> StartDockerCompose(CancellationToken cancellationToken = default);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal interface IDocker
    {
        Task<bool> IsContainerRunning(CancellationToken cancellationToken = default);
        bool IsInstalled();
        Task<StartResult> IsDockerHostRunning(CancellationToken cancellationToken = default);
        Task<bool> StartDockerHost(CancellationToken cancellationToken = default);
        Task<bool> StartDockerCompose(CancellationToken cancellationToken = default);
    }
}
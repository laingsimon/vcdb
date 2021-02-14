using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal interface IDocker
    {
        Task<bool> IsContainerRunning(string containerName);
        bool IsInstalled();
        Task<bool> IsDockerHostRunning();
        Task<bool> StartDockerHost(CancellationToken cancellationToken = default);
        Task<bool> StartDockerCompose(string workingDirectory, ProductName productName, CancellationToken cancellationToken = default);
    }
}
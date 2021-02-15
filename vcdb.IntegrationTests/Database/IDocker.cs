using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal interface IDocker
    {
        Task<bool> IsContainerRunning(ProductName productName);
        bool IsInstalled();
        Task<bool> IsDockerHostRunning();
        Task<bool> StartDockerHost(CancellationToken cancellationToken = default);
        Task<bool> StartDockerCompose(ProductName productName, CancellationToken cancellationToken = default);
    }
}
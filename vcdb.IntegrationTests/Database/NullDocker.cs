using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal class NullDocker : IDocker
    {
        public Task<bool> IsContainerRunning(ProductName productName)
        {
            return Task.FromResult(true);
        }

        public Task<bool> IsDockerHostRunning()
        {
            return Task.FromResult(true);
        }

        public bool IsInstalled()
        {
            return true;
        }

        public Task<bool> StartDockerCompose(ProductName productName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> StartDockerHost(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}

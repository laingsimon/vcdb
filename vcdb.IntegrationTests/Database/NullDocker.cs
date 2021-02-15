using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    internal class NullDocker : IDocker
    {
        public Task<bool> IsContainerRunning()
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

        public Task<bool> StartDockerCompose(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> StartDockerHost(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}

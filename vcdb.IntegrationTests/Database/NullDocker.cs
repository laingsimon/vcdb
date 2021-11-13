using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests.Database
{
    internal class NullDocker : IDocker
    {
        public Task<bool> IsContainerRunning(IntegrationTestOptions options)
        {
            return Task.FromResult(true);
        }

        public Task<StartResult> IsDockerHostRunning()
        {
            return Task.FromResult(StartResult.Started);
        }

        public bool IsInstalled()
        {
            return true;
        }

        public Task<bool> StartDockerCompose(IntegrationTestOptions options, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> StartDockerHost(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}

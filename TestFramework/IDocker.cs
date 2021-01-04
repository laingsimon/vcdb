using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TestFramework
{
    public interface IDocker
    {
        Task<bool> IsContainerRunning(string containerName);
        bool IsInstalled();
        Task<bool> IsDockerHostRunning();
        Task<bool> StartDockerHost(CancellationToken cancellationToken = default);
        Task<bool> StartDockerCompose(string workingDirectory, CancellationToken cancellationToken = default);
    }
}
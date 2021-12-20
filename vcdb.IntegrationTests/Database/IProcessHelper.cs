using System.Threading;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests.Database
{
    public interface IProcessHelper
    {
        Task<string> GetFullPathToCommand(string command, CancellationToken cancellationToken);
    }
}
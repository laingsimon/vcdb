using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb
{
    public interface IConnectionFactory
    {
        Task<DbConnection> CreateConnection();
    }
}

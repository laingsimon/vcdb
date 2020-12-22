using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb
{
    public interface IDatabaseRepository
    {
        Task<DatabaseDetails> GetDatabaseDetails(DbConnection connection);
    }
}
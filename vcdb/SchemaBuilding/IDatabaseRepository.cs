using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IDatabaseRepository
    {
        Task<DatabaseDetails> GetDatabaseDetails(DbConnection connection);
    }
}
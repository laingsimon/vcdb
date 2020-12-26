using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb
{
    public interface IIndexesRepository
    {
        Task<Dictionary<string, IndexDetails>> GetIndexes(DbConnection connection, TableIdentifier tableIdentifier);
    }
}

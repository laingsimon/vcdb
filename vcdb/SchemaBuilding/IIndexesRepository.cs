using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IIndexesRepository
    {
        Task<Dictionary<string, IndexDetails>> GetIndexes(DbConnection connection, TableIdentifier tableIdentifier);
    }
}

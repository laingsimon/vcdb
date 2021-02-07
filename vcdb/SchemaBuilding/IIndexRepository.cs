using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IIndexRepository
    {
        Task<Dictionary<string, IndexDetails>> GetIndexes(DbConnection connection, ObjectName tableName);
    }
}

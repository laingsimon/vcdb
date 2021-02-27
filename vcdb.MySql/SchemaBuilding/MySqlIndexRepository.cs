using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlIndexRepository : IIndexRepository
    {
        public Task<Dictionary<string, IndexDetails>> GetIndexes(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult(new Dictionary<string, IndexDetails>());
        }
    }
}

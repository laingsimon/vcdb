using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlForeignKeyRepository : IForeignKeyRepository
    {
        public Task<Dictionary<string, ForeignKeyDetails>> GetForeignKeys(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult(new Dictionary<string, ForeignKeyDetails>());
        }
    }
}

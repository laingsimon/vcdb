using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IForeignKeyRepository
    {
        Task<Dictionary<string, ForeignKeyDetails>> GetForeignKeys(DbConnection connection, ObjectName tableName);
    }
}

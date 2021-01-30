using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IPrimaryKeyRepository
    {
        Task<PrimaryKeyDetails> GetPrimaryKeyDetails(
            DbConnection connection,
            ObjectName tableName);

        Task<HashSet<string>> GetColumnsInPrimaryKey(
            DbConnection connection,
            ObjectName tableName);
    }
}

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
            TableName tableName);

        Task<HashSet<string>> GetColumnsInPrimaryKey(
            DbConnection connection,
            TableName tableName);
    }
}

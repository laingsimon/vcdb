using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.MySql.SchemaBuilding.Internal
{
    public interface IMySqlGeneratedColumnRepository
    {
        Task<Dictionary<string, string>> GetComputedColumns(DbConnection connection, ObjectName tableName);
    }
}

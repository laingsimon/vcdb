using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SqlServer.SchemaBuilding
{
    public interface ISqlServerComputedColumnRepository
    {
        Task<Dictionary<string, string>> GetComputedColumns(DbConnection connection, ObjectName tableName);
    }
}
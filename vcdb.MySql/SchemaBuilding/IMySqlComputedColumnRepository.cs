using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.MySql.SchemaBuilding
{
    public interface IMySqlComputedColumnRepository
    {
        Task<Dictionary<string, string>> GetComputedColumns(DbConnection connection, ObjectName tableName);
    }
}

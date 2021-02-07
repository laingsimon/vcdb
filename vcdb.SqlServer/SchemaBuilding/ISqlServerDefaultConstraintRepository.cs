using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public interface ISqlServerDefaultConstraintRepository
    {
        Task<IDictionary<string, ColumnDefault>> GetColumnDefaults(DbConnection connection, ObjectName tableName);
    }
}

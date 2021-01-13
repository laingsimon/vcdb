using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace vcdb.SchemaBuilding
{
    public interface IDefaultConstraintRepository
    {
        Task<IDictionary<string, IColumnDefault>> GetColumnDefaults(DbConnection connection, TableName tableName);
    }
}

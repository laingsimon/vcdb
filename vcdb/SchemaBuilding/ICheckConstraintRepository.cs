using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface ICheckConstraintRepository
    {
        Task<IEnumerable<CheckConstraintDetails>> GetCheckConstraints(DbConnection connection, ObjectName tableName);
    }
}
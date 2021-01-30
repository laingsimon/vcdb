using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IProcedureRepository
    {
        Task<Dictionary<ObjectName, ProcedureDetails>> GetProcedures(DbConnection connection);
    }
}

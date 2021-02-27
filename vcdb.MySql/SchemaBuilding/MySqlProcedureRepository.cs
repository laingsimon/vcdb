using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlProcedureRepository : IProcedureRepository
    {
        public Task<Dictionary<ObjectName, ProcedureDetails>> GetProcedures(DbConnection connection)
        {
            return Task.FromResult(new Dictionary<ObjectName, ProcedureDetails>());
        }
    }
}

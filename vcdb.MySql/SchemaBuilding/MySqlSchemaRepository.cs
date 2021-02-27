using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlSchemaRepository : ISchemaRepository
    {
        public Task<Dictionary<string, SchemaDetails>> GetSchemas(DbConnection connection)
        {
            return Task.FromResult(new Dictionary<string, SchemaDetails>());
        }
    }
}

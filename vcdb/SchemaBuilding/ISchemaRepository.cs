using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface ISchemaRepository
    {
        Task<Dictionary<string, SchemaDetails>> GetSchemas(DbConnection connection);
    }
}

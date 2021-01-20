using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IPermissionRepository
    {
        Task<Dictionary<string, PermissionStates>> GetSchemaPermissions(DbConnection connection);
    }
}

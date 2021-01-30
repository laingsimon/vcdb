using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IPermissionRepository
    {
        Task<Permissions> GetDatabasePermissions(DbConnection connection);
        Task<Dictionary<string, Permissions>> GetSchemaPermissions(DbConnection connection);
        Task<Dictionary<ObjectName, Permissions>> GetTablePermissions(DbConnection connection);
        Task<Dictionary<ObjectName, Permissions>> GetProcedurePermissions(DbConnection connection);
    }
}

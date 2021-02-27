using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlPermissionRepository : IPermissionRepository
    {
        public Task<Permissions> GetDatabasePermissions(DbConnection connection)
        {
            return Task.FromResult(new Permissions());
        }

        public Task<Dictionary<ObjectName, Permissions>> GetProcedurePermissions(DbConnection connection)
        {
            return Task.FromResult(new Dictionary<ObjectName, Permissions>());
        }

        public Task<Dictionary<string, Permissions>> GetSchemaPermissions(DbConnection connection)
        {
            return Task.FromResult(new Dictionary<string, Permissions>());
        }

        public Task<Dictionary<ObjectName, Permissions>> GetTablePermissions(DbConnection connection)
        {
            return Task.FromResult(new Dictionary<ObjectName, Permissions>());
        }
    }
}

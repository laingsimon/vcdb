using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IPermissionRepository
    {
        Task<Dictionary<string, Permissions>> GetSchemaPermissions(DbConnection connection);
        Task<Dictionary<TableName, Permissions>> GetTablePermissions(DbConnection connection);
    }
}

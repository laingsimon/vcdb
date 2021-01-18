using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;

namespace vcdb.SchemaBuilding
{
    public interface IUserRepository
    {
        Task<Dictionary<string, UserDetails>> GetUsers(DbConnection connection);
    }
}

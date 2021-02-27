using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlUserRepository : IUserRepository
    {
        public Task<Dictionary<string, UserDetails>> GetUsers(DbConnection connection)
        {
            return Task.FromResult(new Dictionary<string, UserDetails>());
        }
    }
}

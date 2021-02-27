using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.MySql.SchemaBuilding.Models;
using vcdb.SchemaBuilding;

namespace vcdb.MySql.SchemaBuilding
{
    public class MySqlPrimaryKeyRepository : IPrimaryKeyRepository
    {
        public async Task<IEnumerable<string>> GetColumnsInPrimaryKey(DbConnection connection, ObjectName tableName)
        {
            var columns = await connection.QueryAsync<DescribeOutput>($@"
describe {tableName.Name}");

            return columns.Where(col => col.Key == "PRI").Select(col => col.Field).ToArray();
        }

        public Task<PrimaryKeyDetails> GetPrimaryKeyDetails(DbConnection connection, ObjectName tableName)
        {
            return Task.FromResult<PrimaryKeyDetails>(null);
        }
    }
}

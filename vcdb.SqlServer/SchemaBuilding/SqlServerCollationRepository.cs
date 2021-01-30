using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer.SchemaBuilding
{
    public class SqlServerCollationRepository : ICollationRepository
    {
        public async Task<string> GetDatabaseCollation(DbConnection connection)
        {
            return await connection.QuerySingleAsync<string>(@"select collation_name
from sys.databases
where name =  DB_NAME()");
        }

        public async Task<string> GetServerCollation(DbConnection connection)
        {
            return await connection.QuerySingleAsync<string>(@"select convert(varchar(256), SERVERPROPERTY('collation'))");
        }

        public async Task<IDictionary<string, string>> GetColumnCollations(DbConnection connection, ObjectName tableName)
        {
            return await connection.QueryAsync<ColumnCollation>(@"select col.name as ColumnName, col.collation_name
from sys.columns col
inner join sys.tables tab
on tab.object_id = col.object_id
where tab.name = @table_name
and SCHEMA_NAME(tab.schema_id) = @table_owner", 
                new { table_name = tableName.Name, table_owner = tableName.Schema })
                .ToDictionaryAsync(
                    details => details.ColumnName,
                    details => details.collation_name);
        }
    }
}

using Dapper;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using vcdb.Models;
using vcdb.SchemaBuilding;

namespace vcdb.SqlServer
{
    public class SqlServerSchemaRepository : ISchemaRepository
    {
        private static readonly HashSet<string> BuiltInSchemas = new HashSet<string>()
        {
            "dbo",
            "guest",
            "INFORMATION_SCHEMA",
            "sys",
            "db_owner",
            "db_accessadmin",
            "db_securityadmin",
            "db_ddladmin",
            "db_backupoperator",
            "db_datareader",
            "db_datawriter",
            "db_denydatareader",
            "db_denydatawriter"
        };

        public async Task<Dictionary<string, SchemaDetails>> GetSchemas(DbConnection connection)
        {
            var schemas = await connection.QueryAsync<SqlServerSchemata>(@"
select *from INFORMATION_SCHEMA.SCHEMATA");

            return schemas.Where(schema => !BuiltInSchemas.Contains(schema.SCHEMA_NAME)).ToDictionary(
                schema => schema.SCHEMA_NAME,
                schema => new SchemaDetails 
                {
                    
                });
        }
    }
}

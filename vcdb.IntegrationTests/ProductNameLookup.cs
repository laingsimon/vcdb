using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    internal static class ProductNameLookup
    {
        public static readonly HashSet<ProductName> Lookup = new HashSet<ProductName>
        {
            new ProductName("SqlServer", InitialiseDatabase.SqlServer, DropDatabase.SqlServer, DockerConnectionStrings.SqlServer)
        };

        private static class InitialiseDatabase
        {
            public static string SqlServer(DirectoryInfo scenario)
            {
                return $@"
DROP DATABASE IF EXISTS [{scenario.Name}]
GO

CREATE DATABASE [{scenario.Name}]";
            }
        }

        private static class DropDatabase
        {
            public static async Task SqlServer(DirectoryInfo scenario, ISql sql)
            {
                await sql.ExecuteBatchedSql(new StringReader($@"
DROP DATABASE IF EXISTS [{scenario.Name}]
GO"), "master");
            }
        }
    }
}

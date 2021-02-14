using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    [DebuggerDisplay("{name,nq}")]
    public class ProductName
    {
        private readonly string name;
        private readonly Func<DirectoryInfo, string> initialiseDatabase;
        private readonly Func<DirectoryInfo, ISql, Task> dropDatabase;
        private readonly Func<string, DbConnection> createConnection;

        internal string FallbackConnectionString { get; }

        internal ProductName(
            string name,
            Func<DirectoryInfo, string> initialiseDatabase,
            Func<DirectoryInfo, ISql, Task> dropDatabase,
            string fallbackConnectionString,
            Func<string, DbConnection> createConnection)
        {
            this.name = name;
            this.initialiseDatabase = initialiseDatabase;
            this.dropDatabase = dropDatabase;
            FallbackConnectionString = fallbackConnectionString;
            this.createConnection = createConnection;
        }

        public override string ToString()
        {
            return name;
        }

        internal string InitialiseDatabase(DirectoryInfo scenario)
        {
            return initialiseDatabase(scenario);
        }

        internal Task DropDatabase(DirectoryInfo scenario, ISql sql)
        {
            return dropDatabase(scenario, sql);
        }

        public override bool Equals(object obj)
        {
            return (obj is ProductName productName)
                && productName.name.Equals(name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        internal DbConnection CreateConnection(string connectionString)
        {
            return createConnection(connectionString);
        }
    }
}

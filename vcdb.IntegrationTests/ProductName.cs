using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    [DebuggerDisplay("{Name,nq}")]
    internal class ProductName
    {
        public ProductName(
            string name,
            Func<DirectoryInfo, string> initialiseDatabase,
            Func<DirectoryInfo, ISql, Task> dropDatabase)
        {
            Name = name;
            InitialiseDatabase = initialiseDatabase;
            DropDatabase = dropDatabase;
        }

        public string Name { get; }
        public Func<DirectoryInfo, string> InitialiseDatabase { get; }
        public Func<DirectoryInfo, ISql, Task> DropDatabase { get; }
    }
}

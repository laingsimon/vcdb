using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    [DebuggerDisplay("{Name,nq}")]
    public class ProductName
    {
        private readonly string name;

        internal ProductName(
            string name,
            Func<DirectoryInfo, string> initialiseDatabase,
            Func<DirectoryInfo, ISql, Task> dropDatabase,
            string fallbackConnectionString)
        {
            this.name = name;
            InitialiseDatabase = initialiseDatabase;
            DropDatabase = dropDatabase;
            FallbackConnectionString = fallbackConnectionString;
        }

        public override string ToString()
        {
            return name;
        }

        internal Func<DirectoryInfo, string> InitialiseDatabase { get; }
        internal Func<DirectoryInfo, ISql, Task> DropDatabase { get; }
        public string FallbackConnectionString { get; }

        public override bool Equals(object obj)
        {
            return (obj is ProductName productName)
                && productName.name.Equals(name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}

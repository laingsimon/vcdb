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
        internal ProductName(
            string name,
            Func<DirectoryInfo, string> initialiseDatabase,
            Func<DirectoryInfo, ISql, Task> dropDatabase)
        {
            Name = name;
            InitialiseDatabase = initialiseDatabase;
            DropDatabase = dropDatabase;
        }

        public override string ToString()
        {
            return Name;
        }

        public string Name { get; }
        internal Func<DirectoryInfo, string> InitialiseDatabase { get; }
        internal Func<DirectoryInfo, ISql, Task> DropDatabase { get; }

        public override bool Equals(object obj)
        {
            return (obj is ProductName productName)
                && productName.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}

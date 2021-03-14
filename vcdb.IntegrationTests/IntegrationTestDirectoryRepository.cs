using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace vcdb.IntegrationTests
{
    internal class IntegrationTestDirectoryRepository
    {
        private readonly DirectoryInfo rootDirectory;

        public IntegrationTestDirectoryRepository(DirectoryInfo rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        public IEnumerable<IntegrationTestDirectory> GetDirectories()
        {
            var directories = rootDirectory.EnumerateDirectories();
            return directories.Select(d => new IntegrationTestDirectory(d));
        }
    }
}

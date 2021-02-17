using System.Diagnostics;
using System.IO;
using System.Linq;

namespace vcdb.IntegrationTests
{
    [DebuggerDisplay("{directory.Name,nq}")]
    internal class Scenario
    {
        private readonly DirectoryInfo directory;

        public Scenario(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public string File(string relativePath)
        {
            return Path.Combine(directory.FullName, relativePath);
        }

        public string Name => directory.Name;
        public string FullName => directory.FullName;

        public TextReader OpenText(string relativePath)
        {
            var filePath = File(relativePath);

            return System.IO.File.Exists(filePath)
                ? new StreamReader(filePath)
                : null;
        }

        public string FirstFile(params string[] relativePaths)
        {
            var firstFile = relativePaths.Select(File).FirstOrDefault(
                path => System.IO.File.Exists(path));

            return firstFile;
        }
    }
}

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

        public string Name => directory.Name;
        public string FullName => directory.FullName;

        public TextReader Read(string relativePath)
        {
            var filePath = FullPath(relativePath);

            return File.Exists(filePath)
                ? new StreamReader(filePath)
                : null;
        }

        public TextWriter Write(string relativePath)
        {
            var filePath = FullPath(relativePath);

            return new StreamWriter(filePath);
        }

        public string FindFile(params string[] relativePaths)
        {
            var firstFile = relativePaths.Select(FullPath).FirstOrDefault(
                path => File.Exists(path));

            if (firstFile == null)
                throw new FileNotFoundException($"Unable to find file, searched for: {string.Join(", ", relativePaths)} in {directory.FullName}");

            return Path.GetFileName(firstFile);
        }

        private string FullPath(string relativePath)
        {
            return Path.Combine(directory.FullName, relativePath);
        }
    }
}

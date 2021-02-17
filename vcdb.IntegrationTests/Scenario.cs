using System;
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
        
        public TextReader Read(params string[] relativePaths)
        {
            var filePath = FindFile(relativePaths);
            return new StreamReader(Path.Combine(directory.FullName, filePath));
        }

        public TextWriter Write(string relativePath)
        {
            var filePath = FullPath(relativePath);

            return new StreamWriter(filePath);
        }

        public string FindFile(params string[] relativePaths)
        {
            var firstFile = relativePaths.Select(FullPath).FirstOrDefault(File.Exists);

            if (firstFile == null)
                throw new FileNotFoundException($"Unable to find file, searched for: {string.Join(", ", relativePaths)} in {directory.FullName}");

            return Path.GetFileName(firstFile);
        }

        public void Delete(string relativePath)
        {
            var fullPath = FullPath(relativePath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private string FullPath(string relativePath)
        {
            if (relativePath.Contains(".."))
            {
                throw new ArgumentException("Relative path cannot contain directory navigation characters (..)");
            }

            return Path.Combine(directory.FullName, relativePath);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using vcdb.CommandLine;
using vcdb.IntegrationTests.Execution;

namespace vcdb.IntegrationTests
{
    [DebuggerDisplay("{directory.Name,nq}")]
    internal class Scenario
    {
        private readonly IntegrationTestOptions options;
        private readonly IntegrationTestFileGroup scenario;
        
        public Scenario(IntegrationTestOptions options)
        {
            this.options = options;
            scenario = options.FileGroup;
        }

        public DatabaseVersion DatabaseVersion => scenario.DatabaseVersion;
        public DirectoryInfo Directory => scenario.DirectoryPath;
        public string DatabaseName => scenario.DirectoryPath.Name;

        public TextReader Read(string relativePath)
        {
            var filePath = FindFile(relativePath);
            return new StreamReader(Path.Combine(Directory.FullName, filePath));
        }

        public TextWriter Write(string relativePath)
        {
            var filePath = GetFullPaths(relativePath).FirstOrDefault(File.Exists) ?? FullPath(relativePath, FileFormat.NameAndDatabaseVersion);

            return new StreamWriter(filePath);
        }

        public string FindFile(string relativePath)
        {
            var relativePaths = GetFullPaths(relativePath);
            var firstFile = relativePaths.FirstOrDefault(File.Exists);

            if (firstFile == null)
                throw new FileNotFoundException($"Unable to find file, searched for: {string.Join(", ", relativePaths)} in {Directory}");

            return Path.GetFileName(firstFile);
        }

        public void Delete(string relativePath)
        {
            foreach (var path in GetFullPaths(relativePath))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return;
                }
            }
        }

        private string FullPath(string relativePath, FileFormat fileFormat)
        {
            if (DatabaseVersion == null && fileFormat == FileFormat.NameAndDatabaseVersion)
            {
                fileFormat = FileFormat.NameAndProductName;
            }

            switch (fileFormat)
            {
                case FileFormat.NameAndDatabaseVersion:
                    relativePath = Path.ChangeExtension(relativePath, $".{DatabaseVersion}{Path.GetExtension(relativePath)}");
                    break;
                case FileFormat.NameAndProductName:
                    relativePath = Path.ChangeExtension(relativePath, $".{options.DatabaseProduct.Name}{Path.GetExtension(relativePath)}");
                    break;
            }

            if (relativePath.Contains(".."))
            {
                throw new ArgumentException("Relative path cannot contain directory navigation characters (..)");
            }

            return Path.Combine(Directory.FullName, relativePath);
        }

        private IEnumerable<string> GetFullPaths(string relativePath)
        {
            var potentialProductName = Path.GetExtension(Path.ChangeExtension(relativePath, "").TrimEnd('.'));
            if (potentialProductName == "." + options.DatabaseProduct.Name)
            {
                return new[] { FullPath(relativePath, FileFormat.NameOnly) };
            }

            return new[]
            {
                FullPath(relativePath, FileFormat.NameAndDatabaseVersion),
                FullPath(relativePath, FileFormat.NameAndProductName),
                FullPath(relativePath, FileFormat.NameOnly)
            }.Distinct();
        }

        private enum FileFormat
        {
            NameOnly,
            NameAndDatabaseVersion,
            NameAndProductName
        }
    }
}

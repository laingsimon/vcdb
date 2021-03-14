using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using vcdb.CommandLine;

namespace vcdb.IntegrationTests
{
    internal class IntegrationTestDirectory
    {
        private readonly DirectoryInfo directory;

        public IntegrationTestDirectory(DirectoryInfo directory)
        {
            this.directory = directory;
        }

        public ExecutionMode? Mode
        {
            get
            {
                var modeMatch = Regex.Match(directory.Name, @"^(?<mode>.+?)_");

                if (modeMatch.Success && Enum.TryParse<CommandLine.ExecutionMode>(modeMatch.Groups["mode"].Value, out var mode))
                {
                    return mode;
                }

                return null;
            }
        }

        public IEnumerable<IntegrationTestFileGroup> GetFileGroups()
        {
            if (Mode == CommandLine.ExecutionMode.Deploy)
            {
                return GetDeployFileGroups().Distinct();
            }

            if (Mode == CommandLine.ExecutionMode.Read)
            {
                return GetReadFileGroups().Distinct();
            }

            return new IntegrationTestFileGroup[0];
        }

        private IEnumerable<IntegrationTestFileGroup> GetDeployFileGroups()
        {
            var inputFiles = directory.EnumerateFiles("Input*.json").ToArray();
            var setupFiles = directory.EnumerateFiles("Database*.sql").ToArray();
            var expectedOutputFiles = directory.EnumerateFiles("ExpectedOutput*.sql").ToArray();
            var scenarioFiles = directory.EnumerateFiles("Scenario*.json").ToArray();

            foreach (var setupFile in setupFiles)
            {
                var productNameAndVersionMatch = Regex.Match(setupFile.Name, @"Database.(?<productNameAndVersion>.+?).sql$");
                if (productNameAndVersionMatch.Success)
                {
                    var productNameAndVersion = productNameAndVersionMatch.Groups["productNameAndVersion"].Value;
                    var productNameOnly = productNameAndVersion.Contains("^")
                        ? Regex.Match(productNameAndVersion, @"(.+?)^").Groups[1].Value
                        : null;

                    var inputFile = inputFiles.FirstOrDefault(file => file.Name == $"Input.{productNameAndVersion}.json")
                        ?? inputFiles.FirstOrDefault(file => productNameOnly != null && file.Name == $"Input.{productNameOnly}.json")
                        ?? inputFiles.FirstOrDefault(file => file.Name == "Input.json");

                    var expectedOutputFile = expectedOutputFiles.FirstOrDefault(file => file.Name == $"ExpectedOutput.{productNameAndVersion}.sql")
                        ?? expectedOutputFiles.FirstOrDefault(file => productNameOnly != null && file.Name == $"ExpectedOutput.{productNameOnly}.sql")
                        ?? expectedOutputFiles.FirstOrDefault(file => file.Name == "ExpectedOutput.sql");

                    yield return new IntegrationTestFileGroup
                    {
                        Mode = ExecutionMode.Deploy,
                        InputFile = inputFile,
                        ExpectedOutputFile = expectedOutputFile,
                        SetupFile = setupFile,
                        ScenarioFile = new FileInfo(Path.Combine(directory.FullName, "Scenario.json")),
                        DatabaseVersion = DatabaseVersion.Parse(productNameAndVersion),
                        DirectoryPath = directory
                    };
                }
            }

            foreach (var expectedOutputFile in expectedOutputFiles)
            {
                var productNameAndVersionMatch = Regex.Match(expectedOutputFile.Name, @"Database.(?<productNameAndVersion>.+?).sql$");
                if (productNameAndVersionMatch.Success)
                {
                    var productNameAndVersion = productNameAndVersionMatch.Groups["productNameAndVersion"].Value;
                    var productNameOnly = productNameAndVersion.Contains("^")
                        ? Regex.Match(productNameAndVersion, @"(.+?)^").Groups[1].Value
                        : null;

                    var inputFile = inputFiles.FirstOrDefault(file => file.Name == $"Input.{productNameAndVersion}.json")
                        ?? inputFiles.FirstOrDefault(file => productNameOnly != null && file.Name == $"Input.{productNameOnly}.json")
                        ?? inputFiles.FirstOrDefault(file => file.Name == "Input.json");

                    var setupFile = setupFiles.FirstOrDefault(file => file.Name == $"Database.{productNameAndVersion}.sql")
                        ?? expectedOutputFiles.FirstOrDefault(file => productNameOnly != null && file.Name == $"Database.{productNameOnly}.sql")
                        ?? expectedOutputFiles.FirstOrDefault(file => file.Name == "Database.sql");

                    yield return new IntegrationTestFileGroup
                    {
                        Mode = ExecutionMode.Deploy,
                        InputFile = inputFile,
                        ExpectedOutputFile = expectedOutputFile,
                        SetupFile = setupFile,
                        ScenarioFile = new FileInfo(Path.Combine(directory.FullName, "Scenario.json")),
                        DatabaseVersion = DatabaseVersion.Parse(productNameAndVersion),
                        DirectoryPath = directory
                    };
                }
            }
        }

        private IEnumerable<IntegrationTestFileGroup> GetReadFileGroups()
        {
            var setupFiles = directory.EnumerateFiles("Database*.sql");
            var expectedOutputFiles = directory.EnumerateFiles("ExpectedOutput*.json");
            var scenarioFiles = directory.EnumerateFiles("Scenario*.json");

            foreach (var setupFile in setupFiles)
            {
                var productNameAndVersionMatch = Regex.Match(setupFile.Name, @"Database.(?<productNameAndVersion>.+?).sql$");
                if (productNameAndVersionMatch.Success)
                {
                    var productNameAndVersion = productNameAndVersionMatch.Groups["productNameAndVersion"].Value;
                    var productNameOnly = productNameAndVersion.Contains("^")
                        ? Regex.Match(productNameAndVersion, @"(.+?)^").Groups[1].Value
                        : null;

                    var expectedOutputFile = expectedOutputFiles.FirstOrDefault(file => file.Name == $"ExpectedOutput.{productNameAndVersion}.json")
                        ?? expectedOutputFiles.FirstOrDefault(file => productNameOnly != null && file.Name == $"ExpectedOutput.{productNameOnly}.json")
                        ?? expectedOutputFiles.FirstOrDefault(file => file.Name == "ExpectedOutput.json");

                    yield return new IntegrationTestFileGroup
                    {
                        Mode = ExecutionMode.Read,
                        ExpectedOutputFile = expectedOutputFile,
                        SetupFile = setupFile,
                        ScenarioFile = new FileInfo(Path.Combine(directory.FullName, "Scenario.json")),
                        DatabaseVersion = DatabaseVersion.Parse(productNameAndVersion),
                        DirectoryPath = directory
                    };
                }
            }

            foreach (var expectedOutputFile in expectedOutputFiles)
            {
                var productNameAndVersionMatch = Regex.Match(expectedOutputFile.Name, @"Database.(?<productNameAndVersion>.+?).sql$");
                if (productNameAndVersionMatch.Success)
                {
                    var productNameAndVersion = productNameAndVersionMatch.Groups["productNameAndVersion"].Value;
                    var productNameOnly = productNameAndVersion.Contains("^")
                        ? Regex.Match(productNameAndVersion, @"(.+?)^").Groups[1].Value
                        : null;

                    var setupFile = setupFiles.FirstOrDefault(file => file.Name == $"Database.{productNameAndVersion}.sql")
                        ?? expectedOutputFiles.FirstOrDefault(file => productNameOnly != null && file.Name == $"Database.{productNameOnly}.sql")
                        ?? expectedOutputFiles.FirstOrDefault(file => file.Name == "Database.sql");

                    yield return new IntegrationTestFileGroup
                    {
                        Mode = ExecutionMode.Read,
                        ExpectedOutputFile = expectedOutputFile,
                        SetupFile = setupFile,
                        ScenarioFile = new FileInfo(Path.Combine(directory.FullName, "Scenario.json")),
                        DatabaseVersion = DatabaseVersion.Parse(productNameAndVersion),
                        DirectoryPath = directory
                    };
                }
            }
        }
    }
}

using System;
using System.IO;
using vcdb.CommandLine;

namespace vcdb.IntegrationTests
{
    public class IntegrationTestFileGroup : IEquatable<IntegrationTestFileGroup>
    {
        public ExecutionMode Mode { get; set; }
        public FileInfo SetupFile { get; set; }
        public FileInfo InputFile { get; set; }
        public FileInfo ExpectedOutputFile { get; set; }
        public FileInfo ScenarioFile { get; set; }
        public DatabaseVersion DatabaseVersion { get; set; }
        public DirectoryInfo DirectoryPath { get; set; }

        public bool Equals(IntegrationTestFileGroup other)
        {
            if (other == null)
                return false;

            return Mode == other.Mode
                && SetupFile == other.SetupFile
                && InputFile == other.InputFile
                && ExpectedOutputFile == other.ExpectedOutputFile
                && ScenarioFile == other.ScenarioFile
                && DatabaseVersion?.ProductName == other.DatabaseVersion?.ProductName
                && DatabaseVersion?.MinimumCompatibilityVersion == other.DatabaseVersion?.MinimumCompatibilityVersion;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as IntegrationTestFileGroup);
        }

        public override int GetHashCode()
        {
            return Mode.GetHashCode() 
                ^ (SetupFile?.GetHashCode() ?? 0)
                ^ (InputFile?.GetHashCode() ?? 0)
                ^ (ExpectedOutputFile?.GetHashCode() ?? 0)
                ^ (ScenarioFile?.GetHashCode() ?? 0)
                ^ (DatabaseVersion?.ProductName?.GetHashCode() ?? 0)
                ^ (DatabaseVersion?.MinimumCompatibilityVersion?.GetHashCode() ?? 0);
        }
    }
}

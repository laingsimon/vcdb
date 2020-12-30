using JsonEqualityComparer;
using System;
using vcdb.CommandLine;

namespace TestFramework
{
    public class ScenarioSettings
    {
        public string CommandLine { get; set; }
        public ExecutionMode? Mode { get; set; }
        public ComparisonOptions JsonComparison { get; set; }
        public int? ExpectedExitCode { get; set; }
        public string VcDbBuildConfiguraton { get; set; }
        public string VcDbPath { get; set; }

        public static readonly ScenarioSettings Default = new ScenarioSettings
        {
            JsonComparison = new ComparisonOptions
            {
                PropertyNameComparer = StringComparison.OrdinalIgnoreCase
            }
        };
    }
}

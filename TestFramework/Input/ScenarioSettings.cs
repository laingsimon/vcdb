using JsonEqualityComparer;
using System;

namespace TestFramework.Input
{
    public class ScenarioSettings
    {
        public string CommandLine { get; set; }
        public string Mode { get; set; }
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

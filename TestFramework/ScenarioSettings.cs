using JsonEqualityComparer;

namespace TestFramework
{
    public class ScenarioSettings
    {
        public string CommandLine { get; set; }
        public Comparison? Comparison { get; set; }
        public bool Strict { get; set; }
        public bool StdOut { get; set; }
        public ComparisonOptions JsonComparison { get; set; }
        public int? ExpectedExitCode { get; set; }

        public static readonly ScenarioSettings Default = new ScenarioSettings
        {
            Strict = false,
            JsonComparison = new ComparisonOptions()
        };
    }
}

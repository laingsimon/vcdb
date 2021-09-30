using JsonEqualityComparer;
using System;
using System.Collections.Generic;
using vcdb.CommandLine;

namespace vcdb.IntegrationTests.Content
{
    public class ScenarioSettings
    {
        public string CommandLine { get; set; }
        public ExecutionMode? Mode { get; set; }
        public SerialisableComparisonOptions JsonComparison { get; set; }
        public int? ExpectedExitCode { get; set; }
        public string VcDbPath { get; set; }

        public static ScenarioSettings Default = new ScenarioSettings
        {
            JsonComparison = new SerialisableComparisonOptions
            {
                PropertyNameComparer = StringComparison.OrdinalIgnoreCase,
                IgnoreLineEndings = true,
            }
        }.ResolveComparisonOptions();

        public ScenarioSettings ResolveComparisonOptions()
        {
            var stringComparer = JsonComparison?.IgnoreCase == true
                ? (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase
                : null;

            var comparer = JsonComparison?.IgnoreLineEndings == true
                ? new IgnoreLineEndingsComparer(stringComparer)
                : stringComparer;

            if (comparer != null)
            {
                JsonComparison.StringValueComparer = comparer;
            }
            return this;
        }
    }
}

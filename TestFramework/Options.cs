using CommandLine;

namespace TestFramework
{
    public class Options
    {
        [Option("connectionString", Required = true)]
        public string ConnectionString { get; set; }

        [Option("scenarios", Required = false, Default = null)]
        public string ScenariosPath { get; set; }

        [Option("exclude", Required = false)]
        public string ExcludeScenarioFilter { get; set; }

        [Option("include", Required = false)]
        public string IncludeScenarioFilter { get; set; }
    }
}

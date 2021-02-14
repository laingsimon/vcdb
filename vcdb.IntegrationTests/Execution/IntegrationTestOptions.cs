using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestOptions
    {
        public string ConnectionString { get; set; }
        public string ScenariosPath { get; set; }
        public string ExcludeScenarioFilter { get; set; }
        public string IncludeScenarioFilter { get; set; }
        public LogLevel MinLogLevel { get; set; }
        public bool ShowVcdbProgress { get; set; }
        public string DockerDesktopPath { get; set; }
        public bool KeepDatabases { get; set; }
        public int? MaxConcurrency { get; set; }
        public int ProcessTimeout { get; set; } = 60;
        public bool UseLocalDatabase { get; set; }
        public ProductName ProductName { get; set; }
    }
}

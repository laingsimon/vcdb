using System;
using System.IO;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestOptions
    {
        public string ConnectionString { get; set; }
        public string ScenariosPath { get; set; }
        public LogLevel MinLogLevel { get; set; }
        public bool ShowVcdbProgress { get; set; }
        public string DockerDesktopPath { get; set; }
        public bool KeepDatabases { get; set; }
        public int? MaxConcurrency { get; set; }
        public int ProcessTimeout { get; set; } = 60;
        public bool UseLocalDatabase { get; set; }
        public IDatabaseProduct DatabaseProduct { get; set; }
        public string ScenarioName { get; set; }
        public TextWriter StandardOutput { get; set; } = Console.Out;
        public TextWriter ErrorOutput { get; set; } = Console.Error;
    }
}

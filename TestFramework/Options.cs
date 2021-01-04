﻿using CommandLine;
using Microsoft.Extensions.Logging;

namespace TestFramework
{
    public class Options
    {
        [Option("connectionString", Required = true, HelpText = "A connection string to the database server")]
        public string ConnectionString { get; set; }

        [Option("scenarios", Required = false, Default = null, HelpText = "The path of the scenario directories, if not supplied the current directory will be used")]
        public string ScenariosPath { get; set; }

        [Option("exclude", Required = false, HelpText = "A regular expression that defines the scenarios that should be ignored")]
        public string ExcludeScenarioFilter { get; set; }

        [Option("include", Required = false, HelpText = "A regular expression that defines the scenario (names) that can be run")]
        public string IncludeScenarioFilter { get; set; }

        [Option("logLevel", Default = LogLevel.Information)]
        public LogLevel MinLogLevel { get; set; }

        [Option("showVcdbProgress", Default = false, HelpText = "Show STDERR output from vcdb")]
        public bool ShowVcdbProgress { get; set; }

        [Option("dockerDesktopPath", Required = false, HelpText = "A path override to the location of the docker desktop executable")]
        public string DockerDesktopPath { get; set; }
    }
}

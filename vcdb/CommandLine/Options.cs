﻿using CommandLine;

namespace vcdb.CommandLine
{
    public class Options
    {
        [Option('c', "connectionString", Required = true, HelpText = "A connection string to the database server")]
        public string ConnectionString { get; set; }

        [Option('d', "database", HelpText = "Use the specified database")]
        public string Database { get; set; }

        [Option('m', "mode", Default = ExecutionMode.Construct, HelpText = "The mode for execution")]
        public ExecutionMode Mode { get; set; }

        [Option('i', "input", Required = false, HelpText = "The path to the required database representation, if appropriate, otherwise pipe into the command")]
        public string InputFile { get; set; }

        [Option('t', "type", Required = false, HelpText = "The type of database server to connect to", Default = DatabaseType.SqlServer)]
        public DatabaseType DatabaseType { get; set; }
    }
}
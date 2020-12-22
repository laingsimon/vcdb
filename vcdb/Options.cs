using CommandLine;

namespace vcdb
{
    public class Options
    {
        [Option('c', "connectionString", Required = true, HelpText = "A connection string to the database server")]
        public string ConnectionString { get; set; }

        [Option('d', "database", HelpText = "Use the specified database")]
        public string Database { get; set; }

        [Option('m', "mode", Default = ExecutionMode.Construct, HelpText = "The mode for execution")]
        public ExecutionMode Mode { get; set; }
    }
}

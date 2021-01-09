using CommandLine;

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

        [Option("ignoreUnnamedConstraints", Default = false, HelpText = "Don't create rename scripts for unnamed default/check constraints when its table or column changes name")]
        public bool IgnoreUnnamedConstraints { get; set; }

        [Option("hashSize", Default = 8, HelpText = "For unnamed objects, where a hash is required, how long should the hash be, only (increase) this if you are getting hash collisions with objects - this will be extremely unlikely")]
        public int HashSize { get; internal set; }
    }
}

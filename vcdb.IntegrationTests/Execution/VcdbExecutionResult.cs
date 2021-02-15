namespace vcdb.IntegrationTests.Execution
{
    internal class VcdbExecutionResult
    {
        public string Output { get; set; }
        public string ErrorOutput { get; set; }
        public int ExitCode { get; set; }
        public string CommandLine { get; set; }
        public bool Timeout { get; internal set; }
    }
}
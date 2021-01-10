namespace TestFramework
{
    public class ExecutionResult
    {
        public string Output { get; set; }
        public string ErrorOutput { get; set; }
        public int ExitCode { get; set; }
        public string CommandLine { get; set; }
    }
}
namespace vcdb.IntegrationTests
{
    internal static class ExecutorFactory
    {
        public static IExecutor GetExecutor()
        {
            return new AssemblyReferenceExecutor();
        }
    }
}

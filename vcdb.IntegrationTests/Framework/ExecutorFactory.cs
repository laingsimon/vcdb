namespace vcdb.IntegrationTests.Framework
{
    internal static class ExecutorFactory
    {
        public static IExecutor GetExecutor()
        {
            return new AssemblyReferenceExecutor();
        }
    }
}

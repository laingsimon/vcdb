namespace vcdb.IntegrationTests
{
    public static class BuildConfiguration
    {
        public static readonly string Current = "Release";

        static BuildConfiguration()
        {
#if DEBUG
            Current = "Debug";
#endif
        }
    }
}

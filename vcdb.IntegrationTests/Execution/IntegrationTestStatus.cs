namespace vcdb.IntegrationTests.Execution
{
    internal enum IntegrationTestStatus
    {
        Pass,
        Timeout,
        Different,
        InvalidSql,
        UnexpectedExitCode,
        Exception,
        InitialiseDatabaseError
    }
}

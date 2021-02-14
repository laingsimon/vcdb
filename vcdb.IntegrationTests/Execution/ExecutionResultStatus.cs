namespace vcdb.IntegrationTests.Execution
{
    internal enum ExecutionResultStatus
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

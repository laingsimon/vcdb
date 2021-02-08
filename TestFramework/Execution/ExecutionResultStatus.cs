namespace TestFramework.Execution
{
    public enum ExecutionResultStatus
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

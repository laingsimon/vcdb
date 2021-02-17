namespace vcdb.IntegrationTests.Execution
{
    internal enum IntegrationTestStatus
    {
        /// <summary>
        /// An attempt to execute vcdb has failed as required dependencies (from the database interface product) were not registered
        /// </summary>
        DependenciesNotRegistered = -3,

        /// <summary>
        /// An exception occurred during processing of the given request to vcdb
        /// </summary>
        GeneralException = -2,

        /// <summary>
        /// An error occurred during the starup or close down of the vcdb process
        /// </summary>
        UnhandledVcdbException = -1,

        /// <summary>
        /// The scenario passed
        /// </summary>
        Pass = 0,

        /// <summary>
        /// The scenario did not complete in the allotted time
        /// </summary>
        Timeout = 1,

        /// <summary>
        /// The scenario produced output that was different to expected.
        /// Review the ActualOutput file in the scenario directory and compare that to the ExpectedOutput file
        /// </summary>
        Different = 2,

        /// <summary>
        /// The scenario produced the expected output, but when executed against the database server it failed
        /// This indicates - normally - that the produced SQL is invalid
        /// </summary>
        InvalidSql = 3,

        /// <summary>
        /// Vcdb exited with an unexpected error code
        /// </summary>
        UnexpectedExitCode = 4,

        /// <summary>
        /// An error occurred during the execution of this scenario, this typically means there is an error in the integration testing framework
        /// </summary>
        Exception = 5,

        /// <summary>
        /// There was an error initialising the database for this scenario, check the Database.&lt;product&gt;.sql contains valid SQL and the database server is accessible
        /// </summary>
        InitialiseDatabaseError = 6
    }
}

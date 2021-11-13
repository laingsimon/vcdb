using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Database;

namespace vcdb.IntegrationTests
{
    public interface IDatabaseProduct
    {
        /// <summary>
        /// The name of this product, e.g. SqlServer or MySql
        /// The name of the product must not contain any file-system reserved characters, e.g. \, / or : as it is used in the name of expected and input files
        /// </summary>
        string Name { get; }

        /// <summary>
        /// If a connection string has not been provided as an environment variable, use this connection string to connect to a database for executing the tests
        /// </summary>
        string FallbackConnectionString { get; }

        /// <summary>
        /// Execute this statement to prove the database server is ready for connections
        /// </summary>
        string TestConnectionStatement { get; }

        /// <summary>
        /// Create a connection to the database with the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        DbConnection CreateConnection(string connectionString);

        /// <summary>
        /// Retrieve the version of the database server that is installed and able to be connected with the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        string GetInstalledServerVersion(string connectionString);

        /// <summary>
        /// Resolve the version numbers and indicate whether the scenario can be executed, based on a comparison of versions
        /// </summary>
        /// <param name="serverVersion">The version of the accessible database server</param>
        /// <param name="scenarioMinimumVersion">The minimum server version required to execute the scenario</param>
        /// <returns></returns>
        bool IsScenarioVersionCompatibleWithDatabaseVersion(string serverVersion, string scenarioMinimumVersion);

        /// <summary>
        /// Drop the given database, perform any actions that are required before or afterwards.
        /// For example in SQL server the connection must move to using a different database before the database can be dropped
        /// </summary>
        /// <param name="name">The name of the database that needs to be dropped</param>
        /// <param name="sql"></param>
        /// <returns></returns>
        Task DropDatabase(string name, ISql sql);

        /// <summary>
        /// For a given database name, return a statement that can be used to create the database
        /// </summary>
        /// <param name="name">The (unescaped) name of the database to create</param>
        /// <returns></returns>
        string InitialiseDatabase(string name);

        /// <summary>
        /// Given a particular identifier, return an escaped version that is always considered safe for use in a statement.
        /// For example in SQL sever square brackets identify a name, so this method would convert <pre>'MyDatabaseName'</pre> into <pre>'[MyDatabaseName]</pre>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string EscapeIdentifier(string name);

        /// <summary>
        /// For a given SQL statement, split it into batches as appropriate.
        /// For SQL Server this might split the given string on GO, likewise for MySql it might split on a ;
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        IAsyncEnumerable<string> SplitStatementIntoBatches(TextReader statement);

        /// <summary>
        /// Test to see if the product can be connected to
        /// </summary>
        /// <returns></returns>
        bool CanConnect(string connectionString);
    }
}
using NUnit.Framework;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        private readonly IntegrationTestExecutor processExecutor;

        protected IntegrationTestBase()
        {
            processExecutor = new IntegrationTestExecutor();
        }

        [TestCaseSource(typeof(IntegrationTestScenarios.Read))]
        public async Task ExecuteRead(IntegrationTestScenario scenario)
        {
            var options = GetOptions(scenario);

            await processExecutor.ExecuteScenario(options);
        }

        [TestCaseSource(typeof(IntegrationTestScenarios.Deploy))]
        public async Task ExecuteDeploy(IntegrationTestScenario scenario)
        {
            var options = GetOptions(scenario);

            await processExecutor.ExecuteScenario(options);
        }

        private IntegrationTestOptions GetOptions(IntegrationTestScenario scenario)
        {
            var databaseProduct = scenario.DatabaseProduct;
            var connectionString = EnvironmentVariable.Get<string>($"Vcdb_{databaseProduct.Name}_ConnectionString") ?? databaseProduct.FallbackConnectionString;
            var serverVersion = EnvironmentVariable.Get<string>($"Vcdb_{databaseProduct.Name}_Version") ?? databaseProduct.GetInstalledServerVersion(connectionString);
            var scenarioMinimumVersion = scenario.FileGroup.DatabaseVersion.MinimumCompatibilityVersion;

            if (!string.IsNullOrEmpty(serverVersion) && !string.IsNullOrEmpty(scenarioMinimumVersion))
            {
                if (!databaseProduct.IsScenarioVersionCompatibleWithDatabaseVersion(serverVersion, scenarioMinimumVersion))
                {
                    Assert.Inconclusive($"Scenario requires a later ({scenarioMinimumVersion}) version of the database server, currently it is version {serverVersion}");
                    return null;
                }
            }

            return new IntegrationTestOptions
            {
                ConnectionString = connectionString,
                FileGroup = scenario.FileGroup,
                MinLogLevel = LogLevel.Information,
                UseLocalDatabase = EnvironmentVariable.Get<bool?>($"Vcdb_{databaseProduct.Name}_UseLocalDatabase") ?? EnvironmentVariable.Get<bool?>($"Vcdb_UseLocalDatabase") ?? false,
                DatabaseProduct = databaseProduct
            };
        }
    }
}

using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        private readonly IntegrationTestExecutor processExecutor = new IntegrationTestExecutor();
        private readonly IDatabaseProduct databaseProduct;

        protected IntegrationTestBase(IDatabaseProduct databaseProduct = null)
        {
            this.databaseProduct = databaseProduct ?? GetDatabaseProduct();
        }

        private static IDatabaseProduct GetDatabaseProduct()
        {
            var testScenarios = new IntegrationTestScenarios();
            return testScenarios.DatabaseProduct;
        }

#if DEBUG
        [TestCaseSource(typeof(IntegrationTestScenarios)), Explicit]
#endif
        public async Task ExecuteScenario(string scenario)
        {
            var options = GetOptions(scenario);

            await processExecutor.ExecuteScenario(options);
        }

        [Test]
        public async Task ExecuteAllAtOnce()
        {
            var options = GetOptions(null);
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                Assert.Fail("Connection string not found");
            }

            var result = await processExecutor.ExecuteScenarios(options);

            Assert.That(result.Fail, Is.EqualTo(0), "Some scenarios failed");
            if (result.Pass == 0)
            {
                Assert.Inconclusive("No scenarios found");
            }
        }

        private IntegrationTestOptions GetOptions(string scenarioName)
        {
            return new IntegrationTestOptions
            {
                ConnectionString = EnvironmentVariable.Get<string>($"Vcdb_{databaseProduct.Name}_ConnectionString") ?? databaseProduct.FallbackConnectionString,
                ScenarioName = scenarioName,
                MaxConcurrency = 10,
                ScenariosPath = Path.GetFullPath("..\\..\\..\\..\\TestScenarios"),
                MinLogLevel = LogLevel.Information,
                UseLocalDatabase = EnvironmentVariable.Get<bool?>($"Vcdb_{databaseProduct.Name}_UseLocalDatabase") ?? EnvironmentVariable.Get<bool?>($"Vcdb_UseLocalDatabase") ?? false,
                DatabaseProduct = databaseProduct
            };
        }
    }
}

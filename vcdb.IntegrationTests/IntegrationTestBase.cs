using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        private static readonly Dictionary<IDatabaseProduct, Dictionary<string, string>> ProductVersionCache = new Dictionary<IDatabaseProduct, Dictionary<string, string>>();

        private readonly IntegrationTestExecutor processExecutor;
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        protected IntegrationTestBase()
        {
            processExecutor = new IntegrationTestExecutor();
        }

        [OneTimeTearDown]
        public void TearDownOnce()
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        [TestCaseSource(typeof(IntegrationTestScenarios.Read))]
        public async Task ExecuteRead(IntegrationTestScenario scenario)
        {
            var options = GetOptions(scenario);

            var disposable = await processExecutor.ExecuteScenario(options, CancellationToken.None);
            if (disposable != null)
            {
                disposables.Add(disposable);
            }
        }

        [TestCaseSource(typeof(IntegrationTestScenarios.Deploy))]
        public async Task ExecuteDeploy(IntegrationTestScenario scenario)
        {
            var options = GetOptions(scenario);

            var disposable = await processExecutor.ExecuteScenario(options, CancellationToken.None);
            if (disposable != null)
            {
                disposables.Add(disposable);
            }
        }

        private IntegrationTestOptions GetOptions(IntegrationTestScenario scenario)
        {
            var databaseProduct = scenario.DatabaseProduct;
            var connectionString = EnvironmentVariable.Get<string>($"Vcdb_{databaseProduct.Name}_ConnectionString") ?? databaseProduct.FallbackConnectionString;
            var serverVersion = EnvironmentVariable.Get<string>($"Vcdb_{databaseProduct.Name}_Version") ?? TryGetInstalledServerVersion(databaseProduct, connectionString);
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

        private static string TryGetInstalledServerVersion(IDatabaseProduct databaseProduct, string connectionString)
        {
            if (!ProductVersionCache.TryGetValue(databaseProduct, out var versionCache))
            {
                versionCache = new Dictionary<string, string>();
                ProductVersionCache.Add(databaseProduct, versionCache);
            }

            if (versionCache.TryGetValue(connectionString, out var version))
            {
                return version;
            }

            try
            {
                var installedVersion = databaseProduct.GetInstalledServerVersion(connectionString);
                versionCache.TryAdd(connectionString, installedVersion);
                return installedVersion;
            }
            catch (Exception)
            {
                versionCache.TryAdd(connectionString, null);
                return null;
            }
        }
    }
}

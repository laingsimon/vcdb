using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Execution;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests
{
    [TestFixture]
    public class TestScenarios
    {
        internal static bool IncludeProcessOutputInFailureMessage { get; set; } = EnvironmentVariable.Get<bool?>("Vcdb_IncludeProcessOutputInFailureMessage") ?? true;

        private readonly IntegrationTestExecutor processExecutor = new IntegrationTestExecutor();

#if DEBUG
        [TestCaseSource(nameof(ScenarioNames))]
#endif
        public async Task ExecuteScenarios(ProductNameScenario scenario)
        {
            var options = GetOptions(scenario.ProductName, $"^{scenario.ScenarioName}$");

            var result = await processExecutor.ExecuteScenarios(options);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{string.Join("\r\n", result.StdOut)}\r\n{string.Join("\r\n", result.StdErr)}\r\nProcess exited with non-success code");
        }

        [TestCaseSource(nameof(ProductNames))]
        public async Task ExecuteAllAtOnce(ProductName productName)
        {
            var options = GetOptions(productName, null);
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                Assert.Fail("Connection string not found");
            }

            var result = await processExecutor.ExecuteScenarios(options);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);

            var prefix = IncludeProcessOutputInFailureMessage
                ? $"{string.Join("\r\n", result.StdOut)}\r\n{string.Join("\r\n", result.StdErr)}\r\n"
                : "";
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{prefix}\r\nProcess exited with non-success code");
        }

        private static IntegrationTestOptions GetOptions(ProductName productName, string scenarioName)
        {
            return new IntegrationTestOptions
            {
                ConnectionString = EnvironmentVariable.Get<string>($"Vcdb_{productName}_ConnectionString") ?? productName.FallbackConnectionString,
                ScenarioName = scenarioName,
                MaxConcurrency = 10,
                ScenariosPath = Path.GetFullPath("..\\..\\..\\..\\TestScenarios"),
                MinLogLevel = LogLevel.Information,
                UseLocalDatabase = EnvironmentVariable.Get<bool?>($"Vcdb_{productName}_UseLocalDatabase") ?? EnvironmentVariable.Get<bool?>($"Vcdb_UseLocalDatabase") ?? false,
                ProductName = productName
            };
        }

        public static IEnumerable<ProductNameScenario> ScenarioNames
        {
            get
            {
                foreach (var productName in ProductNames)
                {
                    var testScenarios = Path.GetFullPath("..\\..\\..\\..\\TestScenarios");
                    var filter = new ScenarioFilter(productName);

                    foreach (var directory in Directory.GetDirectories(testScenarios))
                    {
                        if (filter.IsValidScenario(directory))
                        {
                            yield return new ProductNameScenario(productName, Path.GetFileName(directory));
                        }
                    }
                }
            }
        }

        public static IEnumerable<ProductName> ProductNames
        {
            get
            {
                return ProductNameLookup.Lookup;
            }
        }
    }
}

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
        private const string DockerSqlServerConnectionString = "server=localhost;user id=sa;password=vcdb_2020";

        internal static string ConnectionString { get; set; } = EnvironmentVariable.Get<string>("Vcdb_ConnectionString") ?? DockerSqlServerConnectionString;
        internal static bool UseLocalDatabase { get; set; } = EnvironmentVariable.Get<bool?>("Vcdb_UseLocalDatabase") ?? false;
        internal static bool IncludeProcessOutputInFailureMessage { get; set; } = EnvironmentVariable.Get<bool?>("Vcdb_IncludeProcessOutputInFailureMessage") ?? true;

        private readonly IntegrationTestExecutor processExecutor = new IntegrationTestExecutor();

#if DEBUG
        [TestCaseSource(nameof(ScenarioNames))]
#endif
        public async Task ExecuteScenarios(ProductNameScenario scenario)
        {
            var options = new IntegrationTestOptions
            {
                ConnectionString = ConnectionString,
                IncludeScenarioFilter = $"^{scenario.ScenarioName}$",
                MaxConcurrency = 10,
                ScenariosPath = Path.GetFullPath("..\\..\\..\\..\\TestScenarios"),
                MinLogLevel = LogLevel.Information,
                UseLocalDatabase = UseLocalDatabase,
                ProductName = scenario.ProductName
            };

            var result = await processExecutor.ExecuteScenarios(options);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{string.Join("\r\n", result.StdOut)}\r\n{string.Join("\r\n", result.StdErr)}\r\nProcess exited with non-success code");
        }

        [TestCaseSource(nameof(ProductNames))]
        public async Task ExecuteAllAtOnce(ProductName productName)
        {
            var options = new IntegrationTestOptions
            {
                ConnectionString = ConnectionString,
                IncludeScenarioFilter = null,
                MaxConcurrency = 10,
                ScenariosPath = Path.GetFullPath("..\\..\\..\\..\\TestScenarios"),
                MinLogLevel = LogLevel.Information,
                UseLocalDatabase = UseLocalDatabase,
                ProductName = productName
            };

            var result = await processExecutor.ExecuteScenarios(options);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);

            var prefix = IncludeProcessOutputInFailureMessage
                ? $"{string.Join("\r\n", result.StdOut)}\r\n{string.Join("\r\n", result.StdErr)}\r\n"
                : "";
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{prefix}\r\nProcess exited with non-success code");
        }

        public static IEnumerable<ProductNameScenario> ScenarioNames
        {
            get
            {
                foreach (var productName in ProductNames)
                {
                    var testScenarios = Path.GetFullPath("..\\..\\..\\..\\TestScenarios");
                    foreach (var directory in Directory.GetDirectories(testScenarios))
                    {
                        if (File.Exists(Path.Combine(directory, $"ExpectedOutput.{productName}.sql")) || File.Exists(Path.Combine(directory, $"Database.{productName}.sql")))
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

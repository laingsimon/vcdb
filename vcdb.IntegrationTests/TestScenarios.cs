using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vcdb.IntegrationTests.Framework;

namespace vcdb.IntegrationTests
{
    [TestFixture]
    public class TestScenarios
    {
        private const string DockerSqlServerConnectionString = "server=localhost;user id=sa;password=vcdb_2020";

        internal static string ConnectionString { get; set; } = EnvironmentVariable.Get<string>("Vcdb_ConnectionString") ?? DockerSqlServerConnectionString;
        internal static bool UseLocalDatabase { get; set; } = EnvironmentVariable.Get<bool?>("Vcdb_UseLocalDatabase") ?? false;

        private readonly Framework.IExecutor processExecutor = ExecutorFactory.GetExecutor();

        [TestCaseSource(nameof(ScenarioNames))]
        public async Task ExecuteSqlServerScenarios(string scenarioName)
        {
            var result = await processExecutor.ExecuteProcess("SqlServer", scenarioName);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{string.Join("\r\n", result.StdOut)}\r\n{string.Join("\r\n", result.StdErr)}\r\nProcess exited with non-success code");
        }

        [Explicit]
        [TestCase("SqlServer")]
        public async Task ExecuteAllAtOnce(string productName)
        {
            var result = await processExecutor.ExecuteProcess(productName);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{string.Join("\r\n", result.StdOut)}\r\nProcess exited with non-success code");
        }

        public static IEnumerable<string> ScenarioNames
        {
            get
            {
                var testScenarios = Path.GetFullPath("..\\..\\..\\..\\TestScenarios");
                return Directory.GetDirectories(testScenarios).Select(dir => Path.GetFileName(dir));
            }
        }
    }
}

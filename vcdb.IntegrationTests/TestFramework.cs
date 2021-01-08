using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace vcdb.IntegrationTests
{
    [TestFixture]
    public class TestFramework
    {
        private const string ConnectionString = "server=localhost;user id=sa;password=vcdb_2020";
        private ProcessExecutor processExecutor;

        public TestFramework()
        {
            processExecutor = new ProcessExecutor();
        }

        [TestCaseSource(nameof(ScenarioNames))]
        public async Task ExecuteScenarios(string scenarioName)
        {
            var commandLineArguments = $"--connectionString \"{ConnectionString}\" --include \"^{scenarioName}$\"";
            var result = await processExecutor.ExecuteProcess(commandLineArguments);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{string.Join("\r\n", result.StdOut)}\r\nProcess exited with non-success code");
        }

        [Explicit]
        [Test]
        public async Task ExecuteAllAtOnce()
        {
            var commandLineArguments = $"--connectionString \"{ConnectionString}\"  --maxConcurrency 10";
            var result = await processExecutor.ExecuteProcess(commandLineArguments);

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

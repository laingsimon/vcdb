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
        internal const string ConnectionString = "server=localhost;user id=sa;password=vcdb_2020";
        private readonly Framework.IExecutor processExecutor = ExecutorFactory.GetExecutor();

        [TestCaseSource(nameof(ScenarioNames))]
        public async Task ExecuteScenarios(string scenarioName)
        {
            var result = await processExecutor.ExecuteProcess(scenarioName);

            result.WriteStdOutTo(Console.Out);
            result.WriteStdErrTo(Console.Error);
            Assert.That(result.ExitCode, Is.EqualTo(0), $"{string.Join("\r\n", result.StdOut)}\r\n{string.Join("\r\n", result.StdErr)}\r\nProcess exited with non-success code");
        }

        [Explicit]
        [Test]
        public async Task ExecuteAllAtOnce()
        {
            var result = await processExecutor.ExecuteProcess();

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

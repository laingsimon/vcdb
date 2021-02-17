using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace vcdb.IntegrationTests.Execution
{
    internal class SingleIntegrationTestExecutionContext : IIntegrationTestExecutionContext
    {
        private readonly StringWriter allOutput;

        public SingleIntegrationTestExecutionContext(StringWriter allOutput)
        {
            this.allOutput = allOutput;
        }

        public void Finished()
        { }

        public IntegrationTestStatus ScenarioComplete(Scenario scenario, IntegrationTestStatus result, IEnumerable<string> differences)
        {
            var output = $"{scenario.Name}: {allOutput.GetStringBuilder()}{string.Join("\r\n", differences)}";

            Assert.That(result, Is.EqualTo(IntegrationTestStatus.Pass), output);
            Assert.That(differences, Is.Empty, output);

            return result;
        }
    }
}

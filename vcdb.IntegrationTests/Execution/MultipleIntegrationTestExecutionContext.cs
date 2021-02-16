using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace vcdb.IntegrationTests.Execution
{
    internal class MultipleIntegrationTestExecutionContext : IIntegrationTestExecutionContext
    {
        private readonly TextWriter output;
        private readonly TextWriter error;
        private readonly Dictionary<IntegrationTestStatus, int> results = new Dictionary<IntegrationTestStatus, int>();

        public MultipleIntegrationTestExecutionContext(TextWriter output, TextWriter error)
        {
            this.output = output;
            this.error = error;
        }

        public int Pass => results.ContainsKey(IntegrationTestStatus.Pass)
            ? results[IntegrationTestStatus.Pass]
            : 0;
        public int Fail => results.Where(pair => pair.Key != IntegrationTestStatus.Pass).Sum(pair => pair.Value);

        public IntegrationTestStatus ScenarioComplete(DirectoryInfo scenario, IntegrationTestStatus result, IEnumerable<string> differences)
        {
            if (!results.ContainsKey(result))
            {
                results.Add(result, 0);
            }
            results[result] = results[result] + 1;

            if (result == IntegrationTestStatus.Pass)
            {
                Debug.WriteLine($"Scenario {scenario.Name} pass");
                return result;
            }

            error.WriteLine($"Scenario {scenario.Name} unsuccessful: {result}");
            foreach (var difference in differences)
                error.WriteLine(difference);
            return result;
        }

        public void Finished()
        {
            var total = (double)Pass + Fail;
            if (total == 0)
            {
                return;
            }

            var passPercentage = Pass / total * 100;
            output.WriteLine($"Finished: Pass: {Pass} ({passPercentage:n0}%), Fail: {Fail} ({100 - passPercentage:n0}%)");
        }
    }
}

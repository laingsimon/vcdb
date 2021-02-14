using System.Collections.Generic;
using System.IO;
using System.Linq;
using vcdb.IntegrationTests.Output;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutionContext
    {
        private readonly ILogger log;

        public Dictionary<ExecutionResultStatus, int> Results { get; } = new Dictionary<ExecutionResultStatus, int>();

        public IntegrationTestExecutionContext(ILogger log)
        {
            this.log = log;
        }

        public ExecutionResultStatus ScenarioComplete(DirectoryInfo scenario, ExecutionResultStatus result, IEnumerable<string> differences)
        {
            if (!Results.ContainsKey(result))
            {
                Results.Add(result, 0);
            }
            Results[result] = Results[result] + 1;

            if (result == ExecutionResultStatus.Pass)
            {
                log.LogDebug($"Scenario {scenario.Name} pass");
                return result;
            }

            log.LogWarning($"Scenario {scenario.Name} unsuccessful: {result}");
            foreach (var difference in differences)
                log.LogWarning(difference);
            return result;
        }

        public int Pass => Results.ContainsKey(ExecutionResultStatus.Pass)
            ? Results[ExecutionResultStatus.Pass]
            : 0;

        public int Fail => Results.Where(pair => pair.Key != ExecutionResultStatus.Pass).Sum(pair => pair.Value);

        public void Finished()
        {
            var total = (double)Pass + Fail;
            if (total == 0)
            {
                log.LogError($"Finished: No scenarios found.");
                return;
            }

            var passPercentage = Pass / total * 100;
            log.LogInformation($"Finished: Pass: {Pass} ({passPercentage:n0}%), Fail: {Fail} ({100 - passPercentage:n0}%)");
        }
    }
}

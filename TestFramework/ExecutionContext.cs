using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;

namespace TestFramework
{
    public class ExecutionContext
    {
        private readonly ILogger<ExecutionContext> log;
        public int Pass { get; private set; }
        public int Fail { get; private set; }

        public ExecutionContext(ILogger<ExecutionContext> log)
        {
            this.log = log;
        }

        public void ScenarioComplete(DirectoryInfo scenario, bool pass, IEnumerable<string> differences)
        {
            if (pass)
            {
                log.LogDebug($"Scenario {scenario.Name} pass");
                Pass++;
                return;
            }

            Fail++;
            log.LogWarning($"Scenario {scenario.Name} failed");
            foreach (var difference in differences)
                log.LogWarning(difference);
            return;
        }

        public void Finished()
        {
            var total = (double)Pass + Fail;
            var passPercentage = (Pass / total) * 100;
            log.LogInformation($"Finished: Pass: {Pass} ({passPercentage:n0}%), Fail: {Fail}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestFramework.Input;
using TestFramework.Output;

namespace TestFramework.Execution
{
    public class ExecutionContext
    {
        private readonly ILogger log;
        private readonly Options options;

        public Dictionary<ExecutionResultStatus, int> Results { get; } = new Dictionary<ExecutionResultStatus, int>();

        public ExecutionContext(ILogger log, Options options)
        {
            this.log = log;
            this.options = options;
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
            var passPercentage = Pass / total * 100;
            if (Console.IsOutputRedirected || options.Porcelain)
            {
                log.LogInformation($"Finished: Pass: {Pass} ({passPercentage:n0}%), Fail: {Fail} ({100 - passPercentage:n0}%)");
            }
            else
            {
                using (log.GetWriteLock())
                {
                    Console.Write("Finished: ");
                    using (new ResetConsoleColor(foreground: ConsoleColor.DarkGreen))
                    {
                        Console.Write($"Pass: {Pass} ({passPercentage:n0}%)");
                    }

                    Console.Write(", ");

                    using (new ResetConsoleColor(foreground: ConsoleColor.Red))
                    {
                        Console.Write($"Fail: {Fail} ({100 - passPercentage:n0}%)");
                    }
                }
            }
        }
    }
}

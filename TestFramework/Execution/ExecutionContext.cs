using System;
using System.Collections.Generic;
using System.IO;
using TestFramework.Input;
using TestFramework.Output;

namespace TestFramework.Execution
{
    public class ExecutionContext
    {
        private readonly ILogger log;
        private readonly Options options;

        public int Pass { get; private set; }
        public int Fail { get; private set; }

        public ExecutionContext(ILogger log, Options options)
        {
            this.log = log;
            this.options = options;
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

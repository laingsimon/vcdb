﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace vcdb.IntegrationTests.Execution
{
    internal class IntegrationTestExecutionContext
    {
        public Dictionary<IntegrationTestStatus, int> Results { get; } = new Dictionary<IntegrationTestStatus, int>();

        public IntegrationTestStatus ScenarioComplete(DirectoryInfo scenario, IntegrationTestStatus result, IEnumerable<string> differences)
        {
            if (!Results.ContainsKey(result))
            {
                Results.Add(result, 0);
            }
            Results[result] = Results[result] + 1;

            if (result == IntegrationTestStatus.Pass)
            {
                Debug.WriteLine($"Scenario {scenario.Name} pass");
                return result;
            }

            Console.Error.WriteLine($"Scenario {scenario.Name} unsuccessful: {result}");
            foreach (var difference in differences)
                Console.Error.WriteLine(difference);
            return result;
        }

        public int Pass => Results.ContainsKey(IntegrationTestStatus.Pass)
            ? Results[IntegrationTestStatus.Pass]
            : 0;

        public int Fail => Results.Where(pair => pair.Key != IntegrationTestStatus.Pass).Sum(pair => pair.Value);

        public void Finished()
        {
            var total = (double)Pass + Fail;
            if (total == 0)
            {
                return;
            }

            var passPercentage = Pass / total * 100;
            Console.WriteLine($"Finished: Pass: {Pass} ({passPercentage:n0}%), Fail: {Fail} ({100 - passPercentage:n0}%)");
        }
    }
}

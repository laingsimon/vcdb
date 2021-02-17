using System.Collections.Generic;
using System.IO;

namespace vcdb.IntegrationTests.Execution
{
    internal interface IIntegrationTestExecutionContext
    {
        void Finished();
        IntegrationTestStatus ScenarioComplete(Scenario scenario, IntegrationTestStatus result, IEnumerable<string> differences);
    }
}
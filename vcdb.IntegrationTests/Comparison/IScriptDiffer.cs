using System.Collections.Generic;

namespace vcdb.IntegrationTests.Comparison
{
    internal interface IScriptDiffer
    {
        IEnumerable<Difference> CompareScripts(string expected, string actual);
    }
}
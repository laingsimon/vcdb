using System.Collections.Generic;
using System.IO;

namespace TestFramework.Comparison
{
    internal interface IScriptDiffer
    {
        IEnumerable<Difference> CompareScripts(TextReader expected, string actual);
    }
}
using System.Collections.Generic;
using System.IO;

namespace TestFramework.Comparison
{
    internal interface IScriptDiffer
    {
        IEnumerable<string> CompareScripts(TextReader expected, string actual);
    }
}
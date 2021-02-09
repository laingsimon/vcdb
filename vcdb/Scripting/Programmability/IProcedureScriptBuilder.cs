using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Programmability
{
    public interface IProcedureScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(IReadOnlyCollection<ProcedureDifference> procedureDifferences);
    }
}

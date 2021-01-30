using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Programmability
{
    public interface IProcedureScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<ProcedureDifference> procedureDifferences);
    }
}

using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.Programmability
{
    public interface IProcedureComparer
    {
        IEnumerable<ProcedureDifference> GetProcedureDifferences(
            ComparerContext context,
            IDictionary<ObjectName, ProcedureDetails> currentProcedures,
            IDictionary<ObjectName, ProcedureDetails> requiredProcedures);
    }
}
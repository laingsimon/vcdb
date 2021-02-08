using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.ForeignKey
{
    public interface IForeignKeyScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(
            ObjectName requiredTableName,
            IReadOnlyCollection<ForeignKeyDifference> foreignKeyDifferences,
            ScriptingPhase phase);
    }
}

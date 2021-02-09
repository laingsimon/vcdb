using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.ForeignKey
{
    public interface IForeignKeyScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(
            ObjectName requiredTableName,
            IReadOnlyCollection<ForeignKeyDifference> foreignKeyDifferences,
            ScriptingPhase phase);
    }
}

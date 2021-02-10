using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.ForeignKey
{
    public interface IForeignKeyScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(
            ObjectName requiredTableName,
            IReadOnlyCollection<ForeignKeyDifference> foreignKeyDifferences);
    }
}

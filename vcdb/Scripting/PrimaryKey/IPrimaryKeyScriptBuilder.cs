using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;

namespace vcdb.Scripting.PrimaryKey
{
    public interface IPrimaryKeyScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(
            ObjectName tableName,
            PrimaryKeyDifference primaryKeyDifference);
    }
}

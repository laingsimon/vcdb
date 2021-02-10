using System.Collections.Generic;
using vcdb.Scripting.Column;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.Table;

namespace vcdb.Scripting.CheckConstraint
{
    public interface ICheckConstraintScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(TableDifference tableDifference);
    }
}

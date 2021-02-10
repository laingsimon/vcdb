using System.Collections.Generic;
using vcdb.Scripting.ExecutionPlan;
using vcdb.Scripting.Table;

namespace vcdb.Scripting.Column
{
    public interface IDefaultConstraintScriptBuilder
    {
        IEnumerable<IScriptTask> CreateUpgradeScripts(TableDifference tableDifference);
        IEnumerable<IScriptTask> CreateUpgradeScripts(ObjectName requiredTableName, ColumnDifference columnDifference);
    }
}
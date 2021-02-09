using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting.Table;

namespace vcdb.Scripting.Column
{
    public interface IDefaultConstraintScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(TableDifference tableDifference);
        IEnumerable<IOutputable> CreateUpgradeScripts(ObjectName requiredTableName, ColumnDifference columnDifference);
    }
}
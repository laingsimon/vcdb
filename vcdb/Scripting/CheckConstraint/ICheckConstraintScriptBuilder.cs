using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting.Column;
using vcdb.Scripting.Table;

namespace vcdb.Scripting.CheckConstraint
{
    public interface ICheckConstraintScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScriptsBeforeColumnChanges(TableDifference tableDifference, ColumnDifference alteration);
        IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference);
    }
}

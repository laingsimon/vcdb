using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface ICheckConstraintScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScriptsBeforeColumnChanges(TableDifference tableDifference, ColumnDifference alteration);
        IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference);
    }
}

using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface ICheckConstraintScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference);
        IEnumerable<SqlScript> CreateUpgradeScripts(TableName requiredTableName, ColumnDifference columnDifference);
    }
}

using System.Collections.Generic;
using vcdb.Output;
using vcdb.Scripting;

namespace vcdb.SqlServer.Scripting
{
    public interface IDefaultConstraintScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(TableDifference tableDifference);
        IEnumerable<SqlScript> CreateUpgradeScripts(TableName requiredTableName, ColumnDifference columnDifference);
    }
}
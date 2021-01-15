using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Table
{
    public interface ITableScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences);
    }
}

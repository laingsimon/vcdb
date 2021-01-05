using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting
{
    public interface ITableScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences);
    }
}

using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Table
{
    public interface ITableScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(IReadOnlyCollection<TableDifference> tableDifferences);
    }
}

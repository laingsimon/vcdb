using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Index
{
    public interface IIndexScriptBuilder
    {
        IEnumerable<SqlScript> CreateUpgradeScripts(TableName requiredTableName, IReadOnlyCollection<IndexDifference> indexDifferences);
    }
}
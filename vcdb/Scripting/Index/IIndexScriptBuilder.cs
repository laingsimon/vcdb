using System.Collections.Generic;
using vcdb.Output;

namespace vcdb.Scripting.Index
{
    public interface IIndexScriptBuilder
    {
        IEnumerable<IOutputable> CreateUpgradeScripts(ObjectName requiredTableName, IReadOnlyCollection<IndexDifference> indexDifferences);
    }
}
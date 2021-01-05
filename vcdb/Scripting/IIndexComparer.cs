using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface IIndexComparer
    {
        IEnumerable<IndexDifference> GetIndexDifferences(
            IDictionary<string, IndexDetails> currentIndexes,
            IDictionary<string, IndexDetails> requiredIndexes,
            IDictionary<string, ColumnDetails> requiredTableColumns);
    }
}

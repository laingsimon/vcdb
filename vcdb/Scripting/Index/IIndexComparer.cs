using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.Index
{
    public interface IIndexComparer
    {
        IEnumerable<IndexDifference> GetIndexDifferences(
            ComparerContext context,
            IDictionary<string, IndexDetails> currentIndexes,
            IDictionary<string, IndexDetails> requiredIndexes);
    }
}

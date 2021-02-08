using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.ForeignKey
{
    public interface IForeignKeyComparer
    {
        IEnumerable<ForeignKeyDifference> GetForeignKeyDifferences(
            ComparerContext context,
            IDictionary<string, ForeignKeyDetails> currentForeignKeys,
            IDictionary<string, ForeignKeyDetails> requiredForeignKeys);
    }
}

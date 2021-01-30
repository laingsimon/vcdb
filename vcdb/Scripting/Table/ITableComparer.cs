using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.Table
{
    public interface ITableComparer
    {
        IEnumerable<TableDifference> GetTableDifferences(
            ComparerContext context,
            IDictionary<ObjectName, TableDetails> currentTables,
            IDictionary<ObjectName, TableDetails> requiredTables);
    }
}

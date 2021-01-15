using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting.Table
{
    public interface ITableComparer
    {
        IEnumerable<TableDifference> GetDifferentTables(
            ComparerContext context,
            IDictionary<TableName, TableDetails> currentTables,
            IDictionary<TableName, TableDetails> requiredTables);
    }
}

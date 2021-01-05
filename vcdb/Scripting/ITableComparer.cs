using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface ITableComparer
    {
        IEnumerable<TableDifference> GetDifferentTables(
            IDictionary<TableName, TableDetails> currentTables,
            IDictionary<TableName, TableDetails> requiredTables);
    }
}
